using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Grinder.Profile;
using Pet_Battle.Bot;
using robotManager;
using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using robotManager.Products;
using wManager;
using wManager.Wow.Bot.States;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace Grinder.Bot
{
    internal static class Bot
    {
        private static Engine _fsm = new Engine();

        static bool _blackListWhereIAmDeadDefault;

        internal static GrinderProfile Profile = new GrinderProfile();
        internal static int ZoneIdProfile;

        static MovementLoop _movementLoop = new MovementLoop {Priority = 1};
        static Grinding _grinding = new Grinding {Priority = 2};

        internal static bool Pulse()
        {
            try
            {
                _fsm = new Engine();
                Profile = new GrinderProfile();
                _movementLoop = new MovementLoop { Priority = 1, ReturnToLastPositionWhenLoop = GrinderSetting.CurrentSetting.RetLastPos };
                _grinding = new Grinding { Priority = 2 };
                ZoneIdProfile = 0;

                // If grinder School Load Profile
                if (!string.IsNullOrWhiteSpace(GrinderSetting.CurrentSetting.ProfileName) &&
                    File.Exists(Application.StartupPath + @"\Profiles\Grinder\" +
                                GrinderSetting.CurrentSetting.ProfileName))
                {
                    Logging.WriteDebug("[Grinder] Profile used: " + GrinderSetting.CurrentSetting.ProfileName);
                    Profile =
                        XmlSerializer.Deserialize<GrinderProfile>(Application.StartupPath + @"\Profiles\Grinder\" +
                                                                  GrinderSetting.CurrentSetting.ProfileName);
                    if (Profile.GrinderZones.Count <= 0)
                        return false;
                }
                else
                {
                    MessageBox.Show(Translate.Get("Please select a profile"));
                    Products.ProductNeedSettings();
                    return false;
                }

                SelectZone();

                // Attach onlevelup for spell book:
                EventsLua.AttachEventLua(LuaEventsId.PLAYER_LEVEL_UP, m => OnLevelUp());

                // Black List:
                var blackListDic =
                    Profile.GrinderZones.SelectMany(zone => zone.BlackListRadius).ToDictionary(b => b.Position,
                                                                                               b => b.Radius);
                wManagerSetting.AddRangeBlackListZone(blackListDic, true);

                // Add Npc
                foreach (var zone in Profile.GrinderZones)
                {
                    NpcDB.AddNpcRange(zone.Npc, wManagerSetting.CurrentSetting.AddToNpcDb);
                }

                // If petbattle option:
                if (GrinderSetting.CurrentSetting.PetBattle)
                {
                    try
                    {
                        // Settings Petbattle:
                        PetBattleSetting.Load();
                        PetBattles.ReviveOne = PetBattleSetting.CurrentSetting.ReviveOne;
                        FightBattlePet.CaptureAllPets = PetBattleSetting.CurrentSetting.CaptureAllPets;
                        FightBattlePet.CaptureIDontHavePets = PetBattleSetting.CurrentSetting.CaptureIDontHavePets;
                        FightBattlePet.CaptureRarePets = PetBattleSetting.CurrentSetting.CaptureRarePets;
                        FightBattlePet.PetBattlesDontFight = PetBattleSetting.CurrentSetting.DontFight;
                        FightBattlePet.AutoOrderPetByLevel = PetBattleSetting.CurrentSetting.AutoOrderPetByLevel;
                        FightBattlePet.LuaCode = PetBattleSetting.CurrentSetting.LuaHook;
                        FightBattlePet.AutoChooseBestPet = PetBattleSetting.CurrentSetting.AutoChooseBestPet;
                        FightBattlePet.AbilitiesBlackListed = PetBattleSetting.CurrentSetting.AbilitiesBlackListed;
                        FightBattlePet.FightClass = PetBattleSetting.CurrentSetting.FightClass;
                    }
                    catch { }
                    _grinding.PetBattle = true;
                }

                // Update spell list
                SpellManager.UpdateSpellBook();

                // Load CC:
                CustomClass.LoadCustomClass();

                // FSM
                _fsm.States.Clear();

                _fsm.AddState(new Relogger { Priority = 200 });

                _fsm.AddState(new Pause { Priority = 16 });
                _fsm.AddState(new BattlegrounderCombination { Priority = 15 });

                _fsm.AddState(new SelectProfileState {Priority = 14});
                _fsm.AddState(new Resurrect {Priority = 13});
                _fsm.AddState(new MyMacro { Priority = 12 });
                _fsm.AddState(new IsAttacked {Priority = 11});
                _fsm.AddState(new BattlePetState { Priority = 11 });
                _fsm.AddState(new Regeneration {Priority = 10});
                _fsm.AddState(new Looting {Priority = 9});
                _fsm.AddState(new Farming {Priority = 8});
                _fsm.AddState(new MillingState {Priority = 7});
                _fsm.AddState(new ProspectingState { Priority = 6 });
                _fsm.AddState(new FlightMasterTakeTaxiState { Priority = 6 });
                _fsm.AddState(new ToTown {Priority = 4});
                _fsm.AddState(new FlightMasterDiscoverState { Priority = 3 });
                _fsm.AddState(new Talents {Priority = 3});
                _fsm.AddState(new Trainers {Priority = 3});
                _fsm.AddState(_grinding);
                _fsm.AddState(_movementLoop);
                _fsm.AddState(new Idle {Priority = 0});

                _fsm.States.Sort();
                _fsm.StartEngine(18, "_Grinder");

                StopBotIf.LaunchNewThread();

                // NpcScan:
                if (wManagerSetting.CurrentSetting.NpcScan)
                    NPCScanState.LaunchNewThread();

                _blackListWhereIAmDeadDefault = wManagerSetting.CurrentSetting.BlackListZoneWhereDead;
                wManagerSetting.CurrentSetting.BlackListZoneWhereDead = GrinderSetting.CurrentSetting.BlackListWhereIAmDead;

                return true;
            }
            catch (Exception e)
            {
                try
                {
                    Dispose();
                }
                catch
                {
                }
                Logging.WriteError("Grinder > Bot > Bot  > Pulse(): " + e);
                return false;
            }
        }

        internal static void Dispose()
        {
            try
            {
                CustomClass.DisposeCustomClass();
                _fsm.StopEngine();
                Fight.StopFight();
                MovementManager.StopMove();
                wManagerSetting.CurrentSetting.BlackListZoneWhereDead = _blackListWhereIAmDeadDefault;
            }
            catch (Exception e)
            {
                Logging.WriteError("Grinder > Bot > Bot  > Dispose(): " + e);
            }
        }

        internal static void SelectZone()
        {
            for (int i = 0; i <= Profile.GrinderZones.Count - 1; i++)
            {
                if (Profile.GrinderZones[i].MaxLevel >= ObjectManager.Me.Level &&
                    Profile.GrinderZones[i].MinLevel <= ObjectManager.Me.Level &&
                    Profile.GrinderZones[i].IsValid())
                {
                    ZoneIdProfile = i;
                    break;
                }
            }

            if (Profile.GrinderZones[ZoneIdProfile].Hotspots)
            {
                var vectors3Temps = new List<Vector3>();
                for (int i = 0; i <= Profile.GrinderZones[ZoneIdProfile].Vectors3.Count - 1; i++)
                {
                    if (i + 1 > Profile.GrinderZones[ZoneIdProfile].Vectors3.Count - 1)
                        vectors3Temps.AddRange(PathFinder.FindPath(Profile.GrinderZones[ZoneIdProfile].Vectors3[i],
                                                                 Profile.GrinderZones[ZoneIdProfile].Vectors3[0]));
                    else
                        vectors3Temps.AddRange(PathFinder.FindPath(Profile.GrinderZones[ZoneIdProfile].Vectors3[i],
                                                                 Profile.GrinderZones[ZoneIdProfile].Vectors3[i + 1]));
                }
                Profile.GrinderZones[ZoneIdProfile].Hotspots = false;
                Profile.GrinderZones[ZoneIdProfile].Vectors3.Clear();
                Profile.GrinderZones[ZoneIdProfile].Vectors3.AddRange(vectors3Temps);
            }
            if (Profile.GrinderZones[ZoneIdProfile].NotLoop)
            {
                var path = new List<Vector3>();
                path.AddRange(Profile.GrinderZones[ZoneIdProfile].Vectors3);
                path.Reverse();
                Profile.GrinderZones[ZoneIdProfile].Vectors3.AddRange(path);
                Profile.GrinderZones[ZoneIdProfile].NotLoop = false;
            }

            _grinding.EntryTarget = Profile.GrinderZones[ZoneIdProfile].TargetEntry;
            _grinding.FactionsTarget = Profile.GrinderZones[ZoneIdProfile].TargetFactions;
            _grinding.MaxTargetLevel = Profile.GrinderZones[ZoneIdProfile].MaxTargetLevel;
            _grinding.MinTargetLevel = Profile.GrinderZones[ZoneIdProfile].MinTargetLevel;

            _movementLoop.PathLoop = Profile.GrinderZones[ZoneIdProfile].Vectors3;
        }

        private static void OnLevelUp()
        {
            Logging.Write("Level UP! Reload Fight Class.");
            // Update spell list
            SpellManager.UpdateSpellBook();

            // Load CC:
            CustomClass.ResetCustomClass();
        }
    }
}
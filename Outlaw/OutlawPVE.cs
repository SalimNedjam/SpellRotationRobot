using System;
using System.Collections.Generic;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Math = System.Math;


    public class Main : ICustomClass
    {
        public string name = "Outlaw";
        public float Range { get { return 8.5f; } }

        private bool _isRunning;


        /*
        * Initialize()
        * When product started, initialize and launch Fightclass
        */
        public void Initialize()
        {
            _isRunning = true;
            OutlawSettings.Load();
            Logging.Write(name + " Is initialized.");
            CreateStatusFrame();
            Rotation();
        }

        /*
        * Dispose()
        * When product stopped
        */
        public void Dispose()
        {
            _isRunning = false;
            Logging.Write(name + " Stop in progress.");
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Stopped!"")");
        }

        /*
        * ShowConfiguration()
        * When use click on Fightclass settings
        */
        public void ShowConfiguration()
        {
            OutlawSettings.Load();
            OutlawSettings.CurrentSetting.ToForm();
            OutlawSettings.CurrentSetting.Save();
        }

        /*
        * Spells for Rotation 
        */
        public Spell RolltheBones = new Spell("Roll the Bones");
        public Spell SinisterStrike = new Spell("Sinister Strike");
        public Spell PistolShot = new Spell("Pistol Shot");
        public Spell Dispatch = new Spell("Dispatch");
        public Spell BetweenTheEyes = new Spell("Between The Eyes");
        public Spell BladeFlurry = new Spell("Blade Flurry");
        public Spell BloodoftheEnemy = new Spell("Blood of the Enemy");
        public Spell AdrenalineRush = new Spell("Adrenaline Rush");
        public Spell Kick = new Spell("Kick");
        public Spell Blind = new Spell("Blind");
        public Spell Gouge = new Spell("Gouge");
        public Spell CrimsonVial = new Spell("Crimson Vial");
        public Spell Feint = new Spell("Feint");
        public Spell Riposte = new Spell("Riposte");
        public Spell CloakOfShadows = new Spell("Cloak of Shadows");

        /* Rotation() */
        public void Rotation()
        {
            Logging.Write(name + ": Started.");

            while (_isRunning)
            {
                try
                {
                    if (Products.InPause)
                        Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Paused!"")");
                    else if (!ObjectManager.Me.IsDeadMe && Fight.InFight && ObjectManager.Me.Target.IsNotZero())
                        CombatRotation();

                }
                catch (Exception e)
                {
                    Logging.WriteError(name + " ERROR: " + e);
                }

                Thread.Sleep(10); // Pause 10 ms to reduce the CPU usage.
            }
            Logging.Write(name + ": Stopped.");
        }
        /*
        * CreateStatusFrame()
        * InGame Status frame to see which spells casting next
        */
        public void CreateStatusFrame()
        {
            Lua.LuaDoString(@"
        if not dRotationFrame then
              dRotationFrame = CreateFrame(""Frame"", ""CrenUI_BuffFrame"", UIParent, ""BasicFrameTemplateWithInset"");
              dRotationFrame:ClearAllPoints()
              dRotationFrame:SetHeight(100)
              dRotationFrame:SetWidth(250)

              dRotationFrame.text = dRotationFrame:CreateFontString(nil, ""BACKGROUND"", ""GameFontNormal"")
              dRotationFrame.text:SetAllPoints()
              dRotationFrame.text:SetText(""Initializing..."")
              dRotationFrame.text:SetTextColor(1, 1, 0, 1)
              dRotationFrame:SetPoint(""CENTER"", 0, 200)

              dRotationFrame:SetMovable(true)
              dRotationFrame:EnableMouse(true)
              dRotationFrame:SetScript(""OnMouseDown"",function() dRotationFrame:StartMoving() end)
              dRotationFrame:SetScript(""OnMouseUp"",function() dRotationFrame:StopMovingOrSizing() end)

              dRotationFrame.Close = CreateFrame(""BUTTON"", nil, dRotationFrame, ""UIPanelCloseButton"")
              dRotationFrame.Close:SetWidth(20)
              dRotationFrame.Close:SetHeight(20)
              dRotationFrame.Close:SetPoint(""TOPRIGHT"", dRotationFrame, 3, 3)
              dRotationFrame.Close:SetScript(""OnClick"", function()
                  dRotationFrame:Hide()
                  DEFAULT_CHAT_FRAME:AddMessage(""WhatsGoingOn |cffC41F3Bclosed |cffFFFFFFWrite /WhatsGoingOn to enable again."") 	
              end)

              SLASH_WHATEVERYOURFRAMESARECALLED1=""/rotation""
              SlashCmdList.WHATEVERYOURFRAMESARECALLED = function()
                  if dRotationFrame:IsShown() then
                      dRotationFrame:Hide()
                  else
                      dRotationFrame:Show()
                  end
              end
            end
        ");
        }

        

        /*
        * CombatRotation()
        */
        public void CombatRotation()
        {

            if (OutlawSettings.CurrentSetting.EnableInterrupt)
            {
            
                WoWUnit toInterrupt = MyHelpers.InterruptableUnits();
                if (toInterrupt != null)
                {
                    ObjectManager.Me.FocusGuid = toInterrupt.Guid;
                    wManager.Wow.Helpers.MovementManager.Face(toInterrupt.Position);

                    if (Kick.IsSpellUsable && Kick.KnownSpell)
                    {
                        Kick.Launch(false, false, false, "focus");
                        Lua.LuaDoString("dRotationFrame.text:SetText(\"Kick " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                        return;
                    }
                    if (Gouge.IsSpellUsable && Gouge.KnownSpell)
                    {
                        Gouge.Launch(false, false, false, "focus");
                        Lua.LuaDoString("dRotationFrame.text:SetText(\"Gouge " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                        return;
                    }
                    if (Blind.IsSpellUsable && Blind.KnownSpell)
                    {
                        Blind.Launch(false, false, false, "focus");
                        Lua.LuaDoString("dRotationFrame.text:SetText(\"Blind " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                        return;
                    }
                }
            }

            if (ObjectManager.Me.HealthPercent < 30)
            {
                if (OutlawSettings.CurrentSetting.EnableCrimsonVial && CrimsonVial.KnownSpell && CrimsonVial.IsSpellUsable)
                {
                    CrimsonVial.Launch();
                    Lua.LuaDoString(@"dRotationFrame.text:SetText(""Crimson Vial"")");
                    return;
                }

                if (OutlawSettings.CurrentSetting.EnableFeint && Feint.KnownSpell && Feint.IsSpellUsable)
                {
                    Feint.Launch();
                    Lua.LuaDoString(@"dRotationFrame.text:SetText(""Feint"")");
                    return;
                }


                if (OutlawSettings.CurrentSetting.EnableRiposte && Riposte.KnownSpell && Riposte.IsSpellUsable)
                {
                    Riposte.Launch();
                    Lua.LuaDoString(@"dRotationFrame.text:SetText(""Riposte"")");
                    return;
                }
            }

            if (OutlawSettings.CurrentSetting.EnableCloakOfShadows
                && CloakOfShadows.KnownSpell && CloakOfShadows.IsSpellUsable && !ObjectManager.Me.CanMove)
            {
                CloakOfShadows.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Cloak Of Shadows"")");
                return;
            }

            if (OutlawSettings.CurrentSetting.EnableRazorCoral
                && EquippedItems.GetEquippedItems().Find(x => x.GetItemInfo.ItemName == ItemsManager.GetNameById(169311)) != null
                && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
                && MyHelpers.GetItemCooldown("Ashvane's Razor Coral") == 0
                && (!ObjectManager.Target.BuffCastedByAll("Razor Coral").Contains(ObjectManager.Me.Guid) || ObjectManager.Target.BuffStack("Razor Coral") >= 10))
            {
                ItemsManager.UseItem(169311);
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Razor Coral"")");
                return;

            }

            if (OutlawSettings.CurrentSetting.EnableBladeFlurry
                && BladeFlurry.KnownSpell && !MyHelpers.haveBuff("Blade Flurry") && BladeFlurry.IsSpellUsable && (MyHelpers.getAttackers(10) > 1))
            {
                BladeFlurry.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Blade Flurry"")");
                return;
            }

            if (OutlawSettings.CurrentSetting.EnableBloodoftheEnemy
                && BloodoftheEnemy.KnownSpell
                && BloodoftheEnemy.IsSpellUsable
                && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
                && (ObjectManager.Target.IsBoss
                        || ObjectManager.Target.IsElite
                        || MyHelpers.getAttackers(10) > 3)
                   )
            {
                BloodoftheEnemy.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Bloodof the Enemy"")");
                return;
            }

            if (OutlawSettings.CurrentSetting.EnableAdrenalineRush
                && AdrenalineRush.KnownSpell
                && AdrenalineRush.IsSpellUsable
                && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
                && (ObjectManager.Target.IsBoss
                        || ObjectManager.Target.IsElite
                        || MyHelpers.getAttackers(10) > 3))
            {
                AdrenalineRush.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Adrenaline Rush"")");
                return;
            }



            if (OutlawSettings.CurrentSetting.EnableRolltheBones
                && RolltheBones.KnownSpell
                && RolltheBones.IsSpellUsable
                && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
                && MyHelpers.getComboPoint() >= 4
                && MyHelpers.rollTheBonesCount() < 2 
                && !MyHelpers.haveBuff(MyHelpers.RuthlessPrecision) 
                && !MyHelpers.haveBuff(MyHelpers.GrandMelee)
                )
            {
                RolltheBones.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Roll the Bones"")");
                return;
            }


            if (OutlawSettings.CurrentSetting.EnablePistolShot
                && PistolShot.KnownSpell
                && PistolShot.IsSpellUsable
                && MyHelpers.getTargetDistance() <= 20.0f
                && ((MyHelpers.haveBuff("Opportunity") && MyHelpers.getComboPoint() <= 4) || (MyHelpers.getTargetDistance() > MyHelpers.getMeleeRange()))
                )
            {
                PistolShot.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Pistol Shot"")");
                return;
            }

            if (OutlawSettings.CurrentSetting.EnableSinisterStrike
                && SinisterStrike.KnownSpell
                && SinisterStrike.IsSpellUsable
                && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
                && MyHelpers.getComboPoint() <= 5)
            {

                SinisterStrike.Launch();
                Lua.LuaDoString("dRotationFrame.text:SetText(\"Sinister Strike " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                return;
            }



            if (OutlawSettings.CurrentSetting.EnableBetweenTheEyes
                && BetweenTheEyes.KnownSpell
                && BetweenTheEyes.IsSpellUsable
                && MyHelpers.getTargetDistance() <= 20.0f
                && MyHelpers.getComboPoint() == 6)
            {
                BetweenTheEyes.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Between the Eyes"")");

                return;
            }

            if (OutlawSettings.CurrentSetting.EnableDispatch
                && Dispatch.KnownSpell
                && Dispatch.IsSpellUsable
                && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
                && MyHelpers.getComboPoint() == 6)
            {
                Dispatch.Launch();
                Lua.LuaDoString("dRotationFrame.text:SetText(\"Dispatch " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                return;
            }

        }
    }

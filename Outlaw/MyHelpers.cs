﻿using wManager.Wow.ObjectManager;
using robotManager.Products;
using wManager.Wow.Helpers;
using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using wManager.Wow.Class;
using wManager;
public class MyHelpers
{
    public static readonly float DefaultMeleeRange = 8.0f;
    public static uint SkullAndCrossbones = 199603;
    public static uint TrueBearing = 193359;
    public static uint RuthlessPrecision = 193357;
    public static uint GrandMelee = 193358;
    public static uint BuriedTreasure = 199600;
    public static uint Broadside = 193356;
    public static Spell SinisterStrike = new Spell("Sinister Strike");


    public static float GetMeleeRangeWithTarget()
    {
        return DefaultMeleeRange + (ObjectManager.Target.CombatReach / 2.0f);
    }
    public static bool InCombat()
    {
        return !ObjectManager.Me.IsMounted
            && ObjectManager.Target.IsAttackable
            && ObjectManager.Target.IsAlive
            && ObjectManager.Me.InCombatFlagOnly;
    }

    public static bool InPull()
    {
        return Fight.InFight
            && !ObjectManager.Me.InCombatFlagOnly;
    }

    public static bool OutOfCombat()
    {
        return !ObjectManager.Me.IsMounted
            && !ObjectManager.Me.IsCast
            && !Fight.InFight
            && !ObjectManager.Me.InCombatFlagOnly;
    }

    public static bool OOCMounted()
    {
        return ObjectManager.Me.IsMounted
            && !Fight.InFight
            && !ObjectManager.Me.InCombatFlagOnly;
    }

    public static int getAttackers(int distance)
    {
        return ObjectManager.GetWoWUnitAttackables(distance).Count;
    }
    public static bool haveBuff(uint spellid)
    {
        return ObjectManager.Me.HaveBuff(spellid);
    }
    public static bool haveBuff(string spellname)
    {
        return ObjectManager.Me.HaveBuff(spellname);
    }
    public static int getComboPoint()
    {
        return ObjectManager.Me.ComboPoint;
    }
    public static float getTargetDistance()
    {
        return ObjectManager.Target.GetDistance;
    }
    public static double getMeleeRange()
    {
        return SinisterStrike.MaxRange + 4.1 < 9.0f ? 9.0f : SinisterStrike.MaxRange + 4.1;
    }

    public static double getDistanceRange()
    {
        return SinisterStrike.MaxRange + 12.1 < 20.0f ? 20.0f : SinisterStrike.MaxRange + 12.1;
    }
    public static double getMaxRange(Spell s, float standard)
    {
        return s.MaxRange + 4.1 < standard ? standard : s.MaxRange + 4.1;
    }
    public static int cooldownTimeLeft(string spellname)
    {
        return ObjectManager.Me.CooldownTimeLeft(spellname);
    }
    public static int[] rollTheBonesCount()
    {
        uint[] list = { SkullAndCrossbones, TrueBearing, RuthlessPrecision, GrandMelee, BuriedTreasure, Broadside };
        int[] ret = {0, 0};
        foreach (uint spellid in list)
        {
            if (ObjectManager.Me.HaveBuff(spellid))
            {
                ret[0] += 1;
                ret[1] = BuffTimeLeft(SpellManager.GetSpellInfo(spellid).Name);
            }

        }
        return ret;
    }
    public static bool rtbReroll()
    {
        int[] reroll = rollTheBonesCount();

        if ((getAttackers(18) < 3
            && reroll[0] < 2
            && !MyHelpers.haveBuff(MyHelpers.RuthlessPrecision))
            || reroll[1] < 5)
            return true;

        if ((getAttackers(18) >= 3
            && reroll[0] < 2
            && !MyHelpers.haveBuff(MyHelpers.RuthlessPrecision)
            && !MyHelpers.haveBuff(MyHelpers.GrandMelee)) 
            || reroll[1] < 5
            || (getAttackers(18) > 1
            && reroll[0] == 2
            && MyHelpers.haveBuff(MyHelpers.SkullAndCrossbones)
            && (MyHelpers.haveBuff(MyHelpers.TrueBearing)|| MyHelpers.haveBuff(MyHelpers.BuriedTreasure))))
            return true;

        return false;
    }
    public static void castSpell(string spellname)
    {
        Lua.LuaDoString("CastSpellByName('" + spellname + "');");
        return;
    }
    public static int cpReduction()
    {
        if (MyHelpers.haveBuff(MyHelpers.Broadside))
            return 1;
        return 0;
    }
    public static WoWUnit InterruptableUnits()
    {
        if (ObjectManager.Target.InCombat && ObjectManager.Target.IsCast && ObjectManager.Target.CanInterruptCasting)
        {
            return ObjectManager.Target;
        }

        List<WoWUnit> list = ObjectManager.GetWoWUnitAttackables(9.0f);

        foreach (WoWUnit x in list)
        {
            if (x.InCombat && x.IsCast && x.CanInterruptCasting)
                return x;
        }
        return null;

    }
    public static void searchAttackers()
    {
        List<WoWUnit> list = ObjectManager.GetWoWUnitAttackables(9.0f);

        if (list.Count > 0)
        {
            ObjectManager.Me.Target = list[0].Guid;
            MovementManager.Face(ObjectManager.Target.Position);
        }   

    }
    #region Combat

    // Stops using wand and waits for its CD to be over
    public static void StopWandWaitGCD(Spell wandSpell, Spell basicSpell)
    {
        wandSpell.Launch();
        int c = 0;
        while (!basicSpell.IsSpellUsable)
        {
            c += 50;
            Thread.Sleep(50);
            if (c >= 1500)
                return;
        }
        //Logger.LogDebug("Waited for GCD : " + c);
        if (c >= 1500)
            wandSpell.Launch();
    }

    // Returns the cooldown of the spell passed as argument
    public static float GetSpellCooldown(string spellName)
    {
        return Lua.LuaDoString<float>("local startTime, duration, enable = GetSpellCooldown('" + spellName + "'); return duration - (GetTime() - startTime)");
    }

    // Returns the cost of the spell passed as argument
    public static int GetSpellCost(string spellName)
    {
        return Lua.LuaDoString<int>("local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo('" + spellName + "'); return cost");
    }

    // Returns the cast time in milliseconds of the spell passed as argument
    public static float GetSpellCastTime(string spellName)
    {
        return Lua.LuaDoString<float>("local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo('" + spellName + "'); return castTime");
    }

    // Reactivates auto attack if it's off. Must pass the Attack spell as argument
    public static void CheckAutoAttack(Spell attack)
    {
        bool _autoAttacking = Lua.LuaDoString<bool>("isAutoRepeat = false; if IsCurrentSpell('Attack') then isAutoRepeat = true end", "isAutoRepeat");
        if (!_autoAttacking && ObjectManager.Target.IsAlive)
        {
            //Logger.LogDebug("Re-activating attack");
            attack.Launch();
        }
    }

    // Returns whether the unit can bleed or be poisoned
    public static bool CanBleed(WoWUnit unit)
    {
        return unit.CreatureTypeTarget != "Elemental" && unit.CreatureTypeTarget != "Mechanical";
    }

    // Returns whether the player is poisoned
    public static bool HasPoisonDebuff()
    {
        return Lua.LuaDoString<bool>
            (@"for i=1,25 do 
	            local _, _, _, _, d  = UnitDebuff('player',i);
	            if d == 'Poison' then
                return true
                end
            end");
    }

    // Returns whether the player has a disease
    public static bool HasDiseaseDebuff()
    {
        return Lua.LuaDoString<bool>
            (@"for i=1,25 do 
	            local _, _, _, _, d  = UnitDebuff('player',i);
	            if d == 'Disease' then
                return true
                end
            end");
    }

    // Returns whether the player has a curse
    public static bool HasCurseDebuff()
    {
        return Lua.LuaDoString<bool>
            (@"for i=1,25 do 
	            local _, _, _, _, d  = UnitDebuff('player',i);
	            if d == 'Curse' then
                return true
                end
            end");
    }

    // Returns whether the player has a magic debuff
    public static bool HasMagicDebuff()
    {
        return Lua.LuaDoString<bool>
            (@"for i=1,25 do 
	            local _, _, _, _, d  = UnitDebuff('player',i);
	            if d == 'Magic' then
                return true
                end
            end");
    }

    // Returns the type of debuff the player has as a string
    public static string GetDebuffType()
    {
        return Lua.LuaDoString<string>
            (@"for i=1,25 do 
	            local _, _, _, _, d  = UnitDebuff('player',i);
	            if (d == 'Poison' or d == 'Magic' or d == 'Curse' or d == 'Disease') then
                return d
                end
            end");
    }

    // Returns whether the player has the debuff passed as a string (ex: Weakened Soul)
    public static bool HasDebuff(string debuffName)
    {
        return Lua.LuaDoString<bool>
            ($"for i=1,25 do " +
                "local n, _, _, _, _  = UnitDebuff('player',i); " +
                "if n == '" + debuffName + "' then " +
                "return true " +
                "end " +
            "end");
    }

    // Returns the time left on a buff in seconds, buff name is passed as string
    public static int BuffTimeLeft(string buffName)
    {
        return Lua.LuaDoString<int>
            ($"for i=1,25 do " +
                "local name, icon, count, debuffType, duration, expirationTime = UnitBuff('player',i);" +
                "if name == '" + buffName + "' then " +
                "return expirationTime - GetTime()" +
                "end " +
            "end");
    }

    // Returns true if the enemy is either casting or channeling (good for interrupts)
    public static bool EnemyCasting()
    {
        int channelTimeLeft = Lua.LuaDoString<int>(@"local spell, _, _, _, endTimeMS = UnitChannelInfo('target')
                                    if spell then
                                     local finish = endTimeMS / 1000 - GetTime()
                                     return finish
                                    end");
        if (channelTimeLeft < 0 || ObjectManager.Target.CastingTimeLeft > Usefuls.Latency)
            return true;
        return false;
    }

    // Waits for GlobalCooldown to be off, must pass the most basic spell avalailable at lvl1 (ex: Smite for priest)
    public static void WaitGlobalCoolDown(Spell s)
    {
        int c = 0;
        while (!s.IsSpellUsable)
        {
            c += 50;
            Thread.Sleep(50);
            if (c >= 2000)
                return;
        }
        //Logger.LogDebug("Waited for GCD : " + c);
    }

    #endregion

    #region Misc




    // Gets Character's specialization (talents)
    public static string GetSpec()
    {
        var Talents = new Dictionary<string, int>();
        for (int i = 1; i <= 3; i++)
        {
            Talents.Add(
                Lua.LuaDoString<string>($"local name, iconTexture, pointsSpent = GetTalentTabInfo({i}); return name"),
                Lua.LuaDoString<int>($"local name, iconTexture, pointsSpent = GetTalentTabInfo({i}); return pointsSpent")
            );
        }
        var highestTalents = Talents.Max(x => x.Value);
        return Talents.Where(t => t.Value == highestTalents).FirstOrDefault().Key;
    }

    // Returns the latency
    public static int GetLatency()
    {
        int worldLatency = Lua.LuaDoString<int>($"local down, up, lagHome, lagWorld = GetNetStats(); return lagWorld");
        int homeLatency = Lua.LuaDoString<int>($"local down, up, lagHome, lagWorld = GetNetStats(); return lagHome");
        return worldLatency + homeLatency;
    }

    #endregion

    #region Items

    // Add to not sell  list
    public static void AddToDoNotSellList(string itemName)
    {
        if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(itemName))
        {
            //Logger.LogDebug($"Adding item {itemName} to Do not Sell List");
            wManagerSetting.CurrentSetting.DoNotSellList.Add(itemName);
        }
    }

    // Return Main hand weapon type as a string
    public static string GetMHWeaponType()
    {
        return Lua.LuaDoString<string>(@"local _, _, _, _, _, _, weapontype = 
                                            GetItemInfo(GetInventoryItemLink('player', 16)); return weapontype;");
    }

    // Check if range weapon (wand, bow, gun) equipped
    public static bool HaveRangedWeaponEquipped()
    {
        return ObjectManager.Me.GetEquipedItemBySlot(wManager.Wow.Enums.InventorySlot.INVSLOT_RANGED) != 0;
    }

    // Deletes items passed as string
    public static void LuaDeleteAllItems(string item)
    {
        Lua.LuaDoString("for bag = 0, 4, 1 do for slot = 1, 32, 1 do local name = GetContainerItemLink(bag, slot); " +
            "if name and string.find(name, \"" + item + "\") then PickupContainerItem(bag, slot); " +
            "DeleteCursorItem(); end; end; end", false);
    }

    // Deletes items passed as string
    public static void LuaDeleteOneItem(string item)
    {
        Lua.LuaDoString("for bag = 0, 4, 1 do for slot = 1, 32, 1 do local name = GetContainerItemLink(bag, slot); " +
            "if name and string.find(name, \"" + item + "\") then PickupContainerItem(bag, slot); " +
            "DeleteCursorItem(); return; end; end; end", false);
    }

    // Count the amount of the specified item stacks in your bags
    public static int CountItemStacks(string itemArg)
    {
        return Lua.LuaDoString<int>("local count = GetItemCount('" + itemArg + "'); return count");
    }

    // Checks if you have any of the listed items in your bags
    public static bool HaveOneInList(List<string> list)
    {
        List<WoWItem> _bagItems = Bag.GetBagItem();
        bool _haveItem = false;
        foreach (WoWItem item in _bagItems)
        {
            if (list.Contains(item.Name))
                _haveItem = true;
        }
        return _haveItem;
    }

    // Get item ID in bag from a list passed as argument (good to check CD)
    public static int GetItemID(List<string> list)
    {
        List<WoWItem> _bagItems = Bag.GetBagItem();
        foreach (WoWItem item in _bagItems)
            if (list.Contains(item.Name))
                return item.Entry;

        return 0;
    }

    // Get item ID in bag from a string passed as argument (good to check CD)
    public static int GetItemID(string itemName)
    {
        List<WoWItem> _bagItems = Bag.GetBagItem();
        foreach (WoWItem item in _bagItems)
            if (itemName.Equals(item))
                return item.Entry;

        return 0;
    }

    // Get item Cooldown (must pass item string as arg)
    public static int GetItemCooldown(string itemName)
    {
        int entry = GetItemID(itemName);
        List<WoWItem> _bagItems = Bag.GetBagItem();
        foreach (WoWItem item in _bagItems)
            if (entry == item.Entry)
                return Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + entry + "); " +
                    "return duration - (GetTime() - startTime)");

        //Logger.Log("Couldn't find item" + itemName);
        return 0;
    }

    // Get item Cooldown from list (must pass item list as arg)
    public static int GetItemCooldown(List<string> itemList)
    {
        int entry = GetItemID(itemList);
        List<WoWItem> _bagItems = Bag.GetBagItem();
        foreach (WoWItem item in _bagItems)
            if (entry == item.Entry)
                return Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + entry + "); " +
                    "return duration - (GetTime() - startTime)");

        //Logger.Log("Couldn't find item");
        return 0;
    }

    // Uses the first item found in your bags that matches any element from the list
    public static void UseFirstMatchingItem(List<string> list)
    {
        List<WoWItem> _bagItems = Bag.GetBagItem();
        foreach (WoWItem item in _bagItems)
        {
            if (list.Contains(item.Name))
            {
                ItemsManager.UseItemByNameOrId(item.Name);
                //Logger.Log("Using " + item.Name);
                return;
            }
        }
    }

    // Returns the item found in your bags that matches the latest element from the list
    public static string GetBestMatchingItem(List<string> list)
    {
        string _bestItem = null;
        int index = 0;

        List<WoWItem> _bagItems = Bag.GetBagItem();
        foreach (WoWItem item in _bagItems)
        {
            if (list.Contains(item.Name))
            {
                int itemIndex = list.IndexOf(item.Name);
                if (itemIndex >= index)
                    _bestItem = item.Name;
            }
        }
        return _bestItem;
    }

    #endregion

    #region Pet

    // Returns wether your pet knows the skill
    public static bool PetKnowsSpell(string spellName)
    {
        bool knowsSpell = false;
        knowsSpell = Lua.LuaDoString<bool>
            ($"for i=1,10 do " +
                "local name, _, _, _, _, _, _ = GetPetActionInfo(i); " +
                "if name == '" + spellName + "' then " +
                "return true " +
                "end " +
            "end");

        return knowsSpell;
    }

    // Casts pet dmg spell if he has over X focus
    public static void CastPetSpellIfEnoughForGrowl(string spellName, uint spellCost)
    {
        if (ObjectManager.Pet.Focus >= spellCost + 15
            && ObjectManager.Pet.HasTarget
            && ObjectManager.Me.InCombatFlagOnly
            && PetKnowsSpell(spellName))
            PetSpellCast(spellName);
    }

    // Returns the index of the pet spell passed as argument
    public static int GetPetSpellIndex(string spellName)
    {
        int spellindex = Lua.LuaDoString<int>
            ($"for i=1,10 do " +
                "local name, _, _, _, _, _, _ = GetPetActionInfo(i); " +
                "if name == '" + spellName + "' then " +
                "return i " +
                "end " +
            "end");

        return spellindex;
    }

    // Returns the cooldown of the pet spell passed as argument
    public static int GetPetSpellCooldown(string spellName)
    {
        int _spellIndex = GetPetSpellIndex(spellName);
        return Lua.LuaDoString<int>("local startTime, duration, enable = GetPetActionCooldown(" + _spellIndex + "); return duration - (GetTime() - startTime)");
    }

    // Returns whether a pet spell is available (off cooldown)
    public static bool GetPetSpellReady(string spellName)
    {
        return GetPetSpellCooldown(spellName) < 0;
    }

    // Casts the pet spell passed as argument
    public static void PetSpellCast(string spellName)
    {
        int spellIndex = GetPetSpellIndex(spellName);
        if (PetKnowsSpell(spellName)
            && GetPetSpellReady(spellName))
        {
            Thread.Sleep(GetLatency() + 100);
            Lua.LuaDoString("CastPetAction(" + spellIndex + ");");
        }
    }

    // Toggles Pet spell autocast (pass true as second argument to toggle on, or false to toggle off)
    public static void TogglePetSpellAuto(string spellName, bool toggle)
    {
        if (PetKnowsSpell(spellName))
        {
            string spellIndex = GetPetSpellIndex(spellName).ToString();

            if (!spellIndex.Equals("0"))
            {
                bool autoCast = Lua.LuaDoString<bool>("local _, autostate = GetSpellAutocast(" + spellIndex + ", 'pet'); " +
                    "return autostate == 1") || Lua.LuaDoString<bool>("local _, autostate = GetSpellAutocast('" + spellName + "', 'pet'); " +
                    "return autostate == 1");

                if ((toggle && !autoCast) || (!toggle && autoCast))
                {
                    //Lua.LuaDoString("ToggleSpellAutocast(" + spellIndex + ", 'pet');");
                    Lua.LuaDoString("ToggleSpellAutocast('" + spellName + "', 'pet');");
                }
            }
        }
    }

    #endregion

    #region Movement

    // get the position behind the target
    public static Vector3 BackofVector3(Vector3 from, WoWUnit targetObject, float radius)
    {
        if (from != null && from != Vector3.Empty)
        {
            float rotation = -Math.DegreeToRadian(Math.RadianToDegree(targetObject.Rotation) + 90);
            return new Vector3((System.Math.Sin(rotation) * radius) + from.X, (System.Math.Cos(rotation) * radius) + from.Y, from.Z);
        }
        return new Vector3(0, 0, 0);
    }

    // Determines if me is behind the Target
    public static bool MeBehindTarget()
    {
        var target = ObjectManager.Target;

        float Pi = (float)System.Math.PI;
        bool backLeft = false;
        bool backRight = false;
        float target_x = ObjectManager.Target.Position.X;
        float target_y = ObjectManager.Target.Position.Y;
        float target_r = ObjectManager.Target.Rotation;
        float player_x = ObjectManager.Me.Position.X;
        float player_y = ObjectManager.Me.Position.Y;
        float d = (float)System.Math.Atan2((target_y - player_y), (target_x - player_x));
        float r = d - target_r;

        if (r < 0) r = r + (Pi * 2);
        if (r > 1.5 * Pi) backLeft = true;
        if (r < 0.5 * Pi) backRight = true;
        if (backLeft || backRight) return true; else return false;
    }

    #endregion

}

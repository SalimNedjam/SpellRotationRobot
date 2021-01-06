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
    private string name = "Outlaw";
    public float Range { get { return 8.5f; } }
    private bool _isRunning;
    private Spell RolltheBones = new Spell("Roll the Bones");
    private Spell SinisterStrike = new Spell("Sinister Strike");
    private Spell PistolShot = new Spell("Pistol Shot");
    private Spell Dispatch = new Spell("Dispatch");
    private Spell BetweenTheEyes = new Spell("Between The Eyes");
    private Spell BladeFlurry = new Spell("Blade Flurry");
    private Spell BloodoftheEnemy = new Spell("Blood of the Enemy");
    private Spell AdrenalineRush = new Spell("Adrenaline Rush");
    private Spell Kick = new Spell("Kick");
    private Spell Blind = new Spell("Blind");
    private Spell Gouge = new Spell("Gouge");
    private Spell CrimsonVial = new Spell("Crimson Vial");
    private Spell Feint = new Spell("Feint");
    private Spell Riposte = new Spell("Riposte");
    private Spell CloakOfShadows = new Spell("Cloak of Shadows");
    private Spell BladeRush = new Spell("Blade Rush");
    private Spell FocusedAzeriteBeam = new Spell("Focused Azerite Beam");
    private Spell Berserking = new Spell("Berserking");

    public void Initialize()
    {
        _isRunning = true;
        OutlawSettings.Load();
        Logging.Write(name + " Is initialized.");
        CreateStatusFrame();
        Rotation();
    }
    public void Dispose()
    {
        _isRunning = false;
        Logging.Write(name + " Stop in progress.");
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Stopped!"")");
    }
    public void ShowConfiguration()
    {
        OutlawSettings.Load();
        OutlawSettings.CurrentSetting.ToForm();
        OutlawSettings.CurrentSetting.Save();
    }
    private void Rotation()
    {
        Logging.Write(name + ": Started.");

        while (_isRunning)
        {
            try
            {
                if (Products.InPause)
                    Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Paused!"")");
                else if (!ObjectManager.Me.IsDeadMe && !ObjectManager.Me.IsMounted)
                {
                    if (ObjectManager.Me.Target.IsNotZero() && ObjectManager.Target.IsAttackable)
                        CombatRotation();
                    else
                    {
                        Lua.LuaDoString(@"dRotationFrame.text:SetText(""searchAttackers!"")");
                        MyHelpers.searchAttackers();

                    }

                }
                    

            }
            catch (Exception e)
            {
                Logging.WriteError(name + " ERROR: " + e);
            }

            Thread.Sleep(100); // Pause 10 ms to reduce the CPU usage.
        }
        Logging.Write(name + ": Stopped.");
    }
    private void CreateStatusFrame()
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
    private bool doKicks()
    {
        Vector3 lastPos = ObjectManager.Me.Position;
        if (OutlawSettings.CurrentSetting.EnableInterrupt)
        {

            WoWUnit toInterrupt = MyHelpers.InterruptableUnits();
            if (toInterrupt != null)
            {
                ObjectManager.Me.FocusGuid = toInterrupt.Guid;
                if (Kick.IsSpellUsable && Kick.KnownSpell && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange())
                {
                    MovementManager.Face(toInterrupt.Position);
                    Kick.Launch(false, false, false, "focus");
                    MovementManager.Face(ObjectManager.Target.Position);
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Kick " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return true;
                }
                if (Gouge.IsSpellUsable && Gouge.KnownSpell && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange())
                {
                    MovementManager.Face(toInterrupt.Position);
                    Gouge.Launch(false, false, false, "focus");
                    MovementManager.Face(ObjectManager.Target.Position); 
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Gouge " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return true;
                }
                if (Blind.IsSpellUsable && Blind.KnownSpell && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange())
                {
                    MovementManager.Face(toInterrupt.Position);
                    Blind.Launch(false, false, false, "focus");
                    MovementManager.Face(ObjectManager.Target.Position);
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Blind " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return true;
                }
            }
        }
        return false;
    }
    private bool doBuffs()
    {
        if (ObjectManager.Me.HealthPercent < 30)
        {
            if (OutlawSettings.CurrentSetting.EnableCrimsonVial && CrimsonVial.KnownSpell && CrimsonVial.IsSpellUsable)
            {
                MyHelpers.castSpell(CrimsonVial.Name);
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Crimson Vial"")");
                return true;
            }

            if (OutlawSettings.CurrentSetting.EnableFeint && Feint.KnownSpell && Feint.IsSpellUsable)
            {
                MyHelpers.castSpell(Feint.Name);
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Feint"")");
                return true;
            }


            if (OutlawSettings.CurrentSetting.EnableRiposte && Riposte.KnownSpell && Riposte.IsSpellUsable)
            {
                MyHelpers.castSpell(Riposte.Name);
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Riposte"")");
                return true;
            }
        }


        if (OutlawSettings.CurrentSetting.EnableRazorCoral
            && EquippedItems.GetEquippedItems().Find(x => x.GetItemInfo.ItemName == ItemsManager.GetNameById(169311)) != null
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && ObjectManager.Target.IsAttackable
            && Lua.LuaDoString("result = \"\";  local cooldown = GetItemCooldown(169311) if (cooldown == 0 ) then     result = true else     result = false end ", "result") == "true"
            && (!ObjectManager.Target.BuffCastedByAll("Razor Coral").Contains(ObjectManager.Me.Guid) || ObjectManager.Target.BuffStack("Razor Coral") >= 10))
        {
            ItemsManager.UseItem(169311);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Razor Coral"")");
            return true;
        }

        if (OutlawSettings.CurrentSetting.EnableBladeFlurry
            && BladeFlurry.KnownSpell
            && !MyHelpers.haveBuff("Blade Flurry")
            && BladeFlurry.IsSpellUsable
            && !MyHelpers.rtbReroll()
            && (MyHelpers.getAttackers(18) > 2))
        {
            MyHelpers.castSpell(BladeFlurry.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Blade Flurry"")");
            return true;
        }


        if (OutlawSettings.CurrentSetting.EnableAdrenalineRush
            && AdrenalineRush.KnownSpell
            && AdrenalineRush.IsSpellUsable
            && !MyHelpers.rtbReroll()
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange())
        {
            MyHelpers.castSpell(AdrenalineRush.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Adrenaline Rush"")");
            return true;
        }

        if (Berserking.KnownSpell
            && Berserking.IsSpellUsable
            && !MyHelpers.rtbReroll()
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange())
        {
            MyHelpers.castSpell(Berserking.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Berserking"")");
            return true;
        }

        if (OutlawSettings.CurrentSetting.EnableBloodoftheEnemy
            && BloodoftheEnemy.KnownSpell
            && BloodoftheEnemy.IsSpellUsable
            && BetweenTheEyes.IsSpellUsable
            && MyHelpers.haveBuff(MyHelpers.RuthlessPrecision)
            && MyHelpers.getComboPoint() >= 6
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && (ObjectManager.Target.IsBoss
                    || ObjectManager.Target.IsElite
                    || MyHelpers.getAttackers(18) > 3
                    || ObjectManager.Target.Name.Contains("Training"))
        )
        {

            BloodoftheEnemy.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Bloodof the Enemy"")");
            return true;
        }

        if (FocusedAzeriteBeam.KnownSpell
            && FocusedAzeriteBeam.IsSpellUsable
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && MyHelpers.getAttackers(10) > 3
        )
        {
            FocusedAzeriteBeam.Launch(false, true, false, "target");
            Thread.Sleep(3000);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Bloodof the Enemy"")");
            return true;
        }

        
        return false;
    }
    private bool doBuilders()
    {
        if (OutlawSettings.CurrentSetting.EnablePistolShot
            && PistolShot.KnownSpell
            && PistolShot.IsSpellUsable
            && MyHelpers.getTargetDistance() <= MyHelpers.getDistanceRange()
            && ((MyHelpers.haveBuff("Opportunity") && MyHelpers.getComboPoint() <= 4)
            || (MyHelpers.getTargetDistance() > MyHelpers.getMeleeRange() && !(BetweenTheEyes.IsSpellUsable && MyHelpers.getComboPoint() == 6))
            || (MyHelpers.haveBuff("Deadshot") && MyHelpers.haveBuff("Seething Rage") && !BetweenTheEyes.IsSpellUsable)
            )
    )
        {
            MyHelpers.castSpell(PistolShot.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Pistol Shot"")");
            return true;
        }

        if (OutlawSettings.CurrentSetting.EnableSinisterStrike
            && SinisterStrike.KnownSpell
            && SinisterStrike.IsSpellUsable
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && MyHelpers.getComboPoint() <= 5)
        {

            MyHelpers.castSpell(SinisterStrike.Name);

            Lua.LuaDoString("dRotationFrame.text:SetText(\"Sinister Strike " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
            return true;
        }
        return false;
    }
    private bool doFinishers()
    {
        

        if (OutlawSettings.CurrentSetting.EnableBetweenTheEyes
            && BetweenTheEyes.KnownSpell
            && BetweenTheEyes.IsSpellUsable
            && MyHelpers.getTargetDistance() <= MyHelpers.getDistanceRange()
            && MyHelpers.getComboPoint() >= 6)
        {
            MyHelpers.castSpell(BetweenTheEyes.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Between the Eyes"")");
            return true;
        }

        if (OutlawSettings.CurrentSetting.EnableRolltheBones
            && RolltheBones.KnownSpell
            && RolltheBones.IsSpellUsable
            && !BetweenTheEyes.IsSpellUsable
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && MyHelpers.getComboPoint() >= 6
            && MyHelpers.rtbReroll()
            )
        {
            MyHelpers.castSpell(RolltheBones.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Roll the Bones"")");
            return true;
        }

        if (OutlawSettings.CurrentSetting.EnableDispatch
            && Dispatch.KnownSpell
            && Dispatch.IsSpellUsable
            && !BetweenTheEyes.IsSpellUsable
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && MyHelpers.getComboPoint() >= 6 - MyHelpers.cpReduction())
        {
            MyHelpers.castSpell(Dispatch.Name);
            Lua.LuaDoString("dRotationFrame.text:SetText(\"Dispatch " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
            return true;
        }

        return false;
    }
    private void CombatRotation()
    {

        if(doKicks())
            return;

        if (doBuffs())
            return;

        if (doFinishers())
            return;

        if (doBuilders())
            return;
    }
}

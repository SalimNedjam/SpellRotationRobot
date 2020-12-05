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
    public float Range { get { return 3.0f; } }

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
    public Spell GrapplingHook = new Spell("Grappling Hook");
    public Spell Sprint = new Spell("Sprint");
    public Spell SmokeBomb = new Spell("Smoke Bomb");
    public Spell GladiatorsMedallion = new Spell(208683);
    public Spell BladeRush = new Spell("Blade Rush");

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
                else if (!ObjectManager.Me.IsDeadMe  && ObjectManager.Me.Target.IsNotZero() && ObjectManager.Target.IsAttackable)
                    CombatRotation();

            }
            catch (Exception e)
            {
                Logging.WriteError(name + " ERROR: " + e);
            }

            Thread.Sleep(50); // Pause 10 ms to reduce the CPU usage.
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

                if (Kick.IsSpellUsable && Kick.KnownSpell)
                {
                    MovementManager.Face(toInterrupt.Position);
                    Kick.Launch(false, false, false, "focus");
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Kick " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return;
                }
                if (Gouge.IsSpellUsable && Gouge.KnownSpell)
                {
                    MovementManager.Face(toInterrupt.Position);
                    Gouge.Launch(false, false, false, "focus");
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Gouge " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return;
                }
                if (Blind.IsSpellUsable && Blind.KnownSpell)
                {
                    MovementManager.Face(toInterrupt.Position);
                    Blind.Launch(false, false, false, "focus");
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Blind " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return;
                }
            }
        }

        if (ObjectManager.Me.Health < ObjectManager.Me.MaxHealth*0.7)
        {
            

            if (CrimsonVial.KnownSpell && CrimsonVial.IsSpellUsable)
            {
                CrimsonVial.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Crimson Vial"")");
                return;
            }

            if (SmokeBomb.KnownSpell && SmokeBomb.IsSpellUsable)
            {
                SmokeBomb.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Smoke Bomb"")");
                return;
            }

            if (Feint.KnownSpell && Feint.IsSpellUsable)
            {
                Feint.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Feint"")");
                return;
            }


            if ( Riposte.KnownSpell && Riposte.IsSpellUsable)
            {
                Riposte.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Riposte"")");
                return;
            }

            
        }

        if (ObjectManager.Me.IsStunned || ObjectManager.Me.Rooted || ObjectManager.Me.Confused)
        {
            if (GladiatorsMedallion.KnownSpell && GladiatorsMedallion.IsSpellUsable)
            {
                GladiatorsMedallion.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Gladiators Medallion"")");
                return;
            }

            
        }
        
        if(ObjectManager.Me.SpeedMoving < 8.05 && ObjectManager.Me.SpeedMoving > 0)
        {
            if (OutlawSettings.CurrentSetting.EnableCloakOfShadows
            && CloakOfShadows.KnownSpell && CloakOfShadows.IsSpellUsable)
            {
                CloakOfShadows.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Cloak Of Shadows"")");
                return;
            }
        }
        if (OutlawSettings.CurrentSetting.EnableRazorCoral
            && EquippedItems.GetEquippedItems().Find(x => x.GetItemInfo.ItemName == ItemsManager.GetNameById(169311)) != null
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && Lua.LuaDoString("result = \"\";  local cooldown = GetItemCooldown(169311) if (cooldown == 0 ) then     result = true else     result = false end ", "result") == "true"
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
                    || ObjectManager.Target.IsLocalPlayer
                    || ObjectManager.Target.Type == WoWObjectType.Player
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
                    || ObjectManager.Target.IsLocalPlayer
                    || ObjectManager.Target.Type == WoWObjectType.Player
                    || MyHelpers.getAttackers(10) > 3))
        {
            AdrenalineRush.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Adrenaline Rush"")");
            return;
        }


        if (OutlawSettings.CurrentSetting.EnableGrapplingHook
            && !ObjectManager.Me.IsMounted
            && GrapplingHook.KnownSpell
            && GrapplingHook.IsSpellUsable
            && MyHelpers.getTargetDistance() <= 20.0f
            && MyHelpers.getTargetDistance() > MyHelpers.getMeleeRange()
            )
        {
            ClickOnTerrain.Spell(195457, ObjectManager.Target.Position);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Grappling Hook"")");
            return;

        }

        if (OutlawSettings.CurrentSetting.EnableSprint
            && !ObjectManager.Me.IsMounted 
            && Sprint.KnownSpell
            && Sprint.IsSpellUsable
            && MyHelpers.getTargetDistance() > 20.0f
            )
        {
            Sprint.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Sprint"")");
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

        if (BladeRush.KnownSpell
            && BladeRush.IsSpellUsable
            && MyHelpers.getTargetDistance() <= 20.0f
            && MyHelpers.getTargetDistance() > MyHelpers.getMeleeRange()
            && !ObjectManager.Me.IsMounted
            )
        {
            BladeRush.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Blade Rush"")");
            return;
        }

        if (OutlawSettings.CurrentSetting.EnablePistolShot
            && PistolShot.KnownSpell
            && PistolShot.IsSpellUsable
            && MyHelpers.getTargetDistance() <= 20.0f
            && ((MyHelpers.haveBuff("Opportunity") && MyHelpers.getComboPoint() <= 4) || ((MyHelpers.getTargetDistance() > MyHelpers.getMeleeRange()) && !ObjectManager.Me.IsMounted))
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

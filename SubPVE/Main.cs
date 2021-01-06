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
    public WoWUnit lastTarget;
    public bool isCheap = true;
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


    public void runCheap()
    {
        isCheap = false;
        Thread.Sleep(18000);
        isCheap = true;
        // do your job!
    }
    /*
    * Spells for Rotation 
    */
    public Spell SymbolsofDeath = new Spell("Symbols of Death");
    public Spell ConcentratedFlame = new Spell("Concentrated Flame");
    public Spell Backstab = new Spell("Backstab");
    public Spell Nightblade = new Spell("Nightblade");
    public Spell MarkedforDeath = new Spell("Marked for Death");
    public Spell Eviscerate = new Spell("Eviscerate");
    public Spell KidneyShot = new Spell("Kidney Shot");
    public Spell ShadowStep = new Spell("Shadowstep");
    public Spell ShadowDance = new Spell("Shadow Dance");
    public Spell ShadowBlades = new Spell("Shadow Blades");
    public Spell Evasion = new Spell("Evasion");
    public Spell ShadowyDuel = new Spell("Shadowy Duel");
    public Spell Shadowstrike = new Spell("Shadowstrike");
    public Spell ColdBlood = new Spell("Cold Blood");
    public Spell CheapShot = new Spell("Cheap Shot");
    public Spell Sealth = new Spell("Sealth");
    public Spell BloodoftheEnemy = new Spell("Blood of the Enemy");
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
                else if (!ObjectManager.Me.IsDeadMe && ObjectManager.Me.Target.IsNotZero() && !ObjectManager.Me.IsMounted)
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
        if (lastTarget != ObjectManager.Target)
        {
            lastTarget = ObjectManager.Target;
            isCheap = true;
        }

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

                if (Blind.IsSpellUsable && Blind.KnownSpell)
                {
                    MovementManager.Face(toInterrupt.Position);
                    Blind.Launch(false, false, false, "focus");
                    Lua.LuaDoString("dRotationFrame.text:SetText(\"Blind " + MyHelpers.getTargetDistance() + " / " + MyHelpers.GetMeleeRangeWithTarget() + "\")");
                    return;
                }
            }
        }

        if (ObjectManager.Me.Health < ObjectManager.Me.MaxHealth * 0.7)
        {
            if (CrimsonVial.KnownSpell && CrimsonVial.IsSpellUsable)
            {
                CrimsonVial.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Crimson Vial"")");
                return;
            }
            if (Feint.KnownSpell && Feint.IsSpellUsable)
            {
                Feint.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Feint"")");
                return;
            }
        }

        if (ObjectManager.Me.Health < ObjectManager.Me.MaxHealth * 0.3)
        {
            if (Evasion.KnownSpell && Evasion.IsSpellUsable)
            {
                Evasion.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Evasion"")");
                return;
            }


        }







        if (MyHelpers.sealthed()
            && ShadowBlades.KnownSpell
            && ShadowBlades.IsSpellUsable
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            ShadowBlades.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""ShadowBlades"")");
            return;

        }
        if (SymbolsofDeath.KnownSpell
            && SymbolsofDeath.IsSpellUsable
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            SymbolsofDeath.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""SymbolsofDeath"")");
            return;

        }

        if (EquippedItems.GetEquippedItems().Find(x => x.GetItemInfo.ItemName == ItemsManager.GetNameById(167383)) != null
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && Lua.LuaDoString("result = \"\";  local cooldown = GetItemCooldown(167383) if (cooldown == 0 ) then     result = true else     result = false end ", "result") == "true"
            )
        {
            ItemsManager.UseItem(167383);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""maledict"")");
            return;

        }
        if (MarkedforDeath.KnownSpell
        && MarkedforDeath.IsSpellUsable
        && MyHelpers.getComboPoint() == 0
        && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
        )
        {
            MarkedforDeath.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""MarkedforDeath"")");
            return;

        }
        if (MyHelpers.sealthed()
            && Nightblade.KnownSpell
            && Nightblade.IsSpellUsable
            && MyHelpers.getComboPoint() >= 5
            && !ObjectManager.Target.HaveBuff("Nightblade")
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            Nightblade.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Nightblade"")");
            return;

        }

        if (MyHelpers.sealthed()
            && Shadowstrike.KnownSpell
            && Shadowstrike.IsSpellUsable
            && MyHelpers.getComboPoint() < 5
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            Shadowstrike.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Shadowstrike"")");
            return;

        }

        if (!MyHelpers.sealthed()
            && ShadowDance.KnownSpell
            && ShadowDance.IsSpellUsable
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            ShadowDance.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""ShadowDance"")");
            return;

        }

        if (Eviscerate.KnownSpell
            && Eviscerate.IsSpellUsable
            && MyHelpers.getComboPoint() >= 5
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            Eviscerate.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Eviscerate"")");
            return;

        }
        if (ConcentratedFlame.KnownSpell
            && ConcentratedFlame.IsSpellUsable
            )
        {
            ConcentratedFlame.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Concentrated Flame"")");
            return;

        }
        if (!MyHelpers.sealthed()
            && Backstab.KnownSpell
            && Backstab.IsSpellUsable
            && MyHelpers.getComboPoint() < 5
            && MyHelpers.getTargetDistance() < MyHelpers.getMeleeRange()
            )
        {
            Backstab.Launch();
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""ShadowDance"")");
            return;

        }


    }
}

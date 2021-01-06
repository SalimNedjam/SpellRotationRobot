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

    }

    /*
    * Spells for Rotation 
    */
    public Spell Garrote = new Spell("Garrote");
    public Spell Rupture = new Spell("Rupture");
    public Spell Envenom = new Spell("Envenom");
    public Spell FanOfKnives = new Spell("Fan Of Knives");
    public Spell Mutilate = new Spell("Mutilate");
    public Spell PoisonedKnife = new Spell("Poisoned Knife");
    public Spell Vendetta = new Spell("Vendetta");
    public Spell ToxicBlade = new Spell("Toxic Blade");
    public Spell BloodoftheEnemy = new Spell("Blood of the Enemy");
    public Spell GuardianofAzeroth = new Spell("Guardian of Azeroth");
    public Spell MemoryofLucidDreams = new Spell("Memory of Lucid Dreams");
    public Spell Vanish = new Spell("Vanish");
    public Spell AdrenalineRush = new Spell("Adrenaline Rush");
    public Spell Kick = new Spell("Kick");
    public Spell Blind = new Spell("Blind");
    public Spell Gouge = new Spell("Gouge");
    public Spell CrimsonVial = new Spell("Crimson Vial");
    public Spell Feint = new Spell("Feint");
    public Spell Riposte = new Spell("Riposte");
    public Spell CloakOfShadows = new Spell("Cloak of Shadows");
    public Spell Sprint = new Spell("Sprint");
    public Spell SmokeBomb = new Spell("Smoke Bomb");
    public Spell GladiatorsMedallion = new Spell(208683);
    public Spell Berserking = new Spell("Berserking");
    public Spell DeadlyPoison = new Spell("Deadly Poison");
    public Spell CripplingPoison = new Spell("Crippling Poison");

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
                else if (!ObjectManager.Me.IsDeadMe  && ObjectManager.Me.Target.IsNotZero() )
                    CombatRotation();
                else if (!MyHelpers.haveBuff("Deadly Poison"))
                    DeadlyPoison.Launch();
                else if (!MyHelpers.haveBuff("Crippling Poison"))
                    CripplingPoison.Launch();

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
    private bool doResist()
    {
        if (ObjectManager.Me.HealthPercent < 30)
        {
            if (OutlawSettings.CurrentSetting.EnableCrimsonVial && CrimsonVial.KnownSpell && CrimsonVial.IsSpellUsable)
            {
                CrimsonVial.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Crimson Vial"")");
                return true;
            }

            if (OutlawSettings.CurrentSetting.EnableFeint && Feint.KnownSpell && Feint.IsSpellUsable)
            {
                Feint.Launch();
                Lua.LuaDoString(@"dRotationFrame.text:SetText(""Feint"")");
                return true;
            }


        }

        return false;
    }

    /*
    * CombatRotation()
    */
    public void CombatRotation()
    {
        if (doKicks())
            return;

        if (doResist())
            return;

        if (Garrote.KnownSpell && Garrote.IsSpellUsable
           && (!ObjectManager.Target.HaveBuff("Garrote") || MyHelpers.TargetDebuffTimeLeft("Garrote") < 2 ))
        {
            MyHelpers.castSpell(Garrote.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Garrote"")");
            return;
        }
        if (EquippedItems.GetEquippedItems().Find(x => x.GetItemInfo.ItemName == ItemsManager.GetNameById(169311)) != null
            && MyHelpers.getTargetDistance() <= MyHelpers.getMeleeRange()
            && ObjectManager.Target.IsAttackable
            && Lua.LuaDoString<bool>(@"if GetItemCooldown(169311) == 0  then return true; else return false; end")
            && (!ObjectManager.Target.BuffCastedByAll("Razor Coral").Contains(ObjectManager.Me.Guid)  || ObjectManager.Target.BuffCastedByAll("Vendetta").Contains(ObjectManager.Me.Guid)))
        {
            ItemsManager.UseItem(169311);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Razor Coral"")");
            return;
        }

        if  (Rupture.KnownSpell && Rupture.IsSpellUsable
            && MyHelpers.getComboPoint() > 4 
            && (!ObjectManager.Target.HaveBuff("Rupture") || MyHelpers.TargetDebuffTimeLeft("Rupture") < 2))
        {
            MyHelpers.castSpell(Rupture.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Rupture"")");
            return;
        }
        
        if (Vendetta.KnownSpell && Vendetta.IsSpellUsable && (MyHelpers.haveOP() > 20 || MyHelpers.haveBL()))
        {
            MyHelpers.castSpell(Vendetta.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Vendetta"")");
            return;
        }
        if (Berserking.KnownSpell && Berserking.IsSpellUsable && ObjectManager.Target.BuffCastedByAll("Vendetta").Contains(ObjectManager.Me.Guid))
        {
            MyHelpers.castSpell(Berserking.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""GuardianofAzeroth"")");
            return;
        }

        if (GuardianofAzeroth.KnownSpell && GuardianofAzeroth.IsSpellUsable && ObjectManager.Target.BuffCastedByAll("Vendetta").Contains(ObjectManager.Me.Guid))
        {
            MyHelpers.castSpell(GuardianofAzeroth.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""GuardianofAzeroth"")");
            return;
        }
        if (ToxicBlade.KnownSpell && ToxicBlade.IsSpellUsable && ((!Vendetta.IsSpellUsable && ObjectManager.Target.HaveBuff("Rupture") && MyHelpers.haveBuff("Envenom")) || ObjectManager.Target.BuffCastedByAll("Vendetta").Contains(ObjectManager.Me.Guid)))
        {
            MyHelpers.castSpell(ToxicBlade.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Toxic Blade"")");
            return;
        }
        if (Vanish.KnownSpell && Vanish.IsSpellUsable && Garrote.IsSpellUsable && ObjectManager.Target.BuffCastedByAll("Vendetta").Contains(ObjectManager.Me.Guid))
        {
            MyHelpers.castSpell(Vanish.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Garrote"")");
            return;
        }

        if (Envenom.KnownSpell && Envenom.IsSpellUsable
           && MyHelpers.getComboPoint() > 4)
        {
            MyHelpers.castSpell(Envenom.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Envenom"")");
            return;
        }

        if (Mutilate.KnownSpell && Mutilate.IsSpellUsable
           && MyHelpers.getComboPoint() <= 4)
        {
            MyHelpers.castSpell(Mutilate.Name);
            Lua.LuaDoString(@"dRotationFrame.text:SetText(""Mutilate"")");
            return;
        }
    }
}

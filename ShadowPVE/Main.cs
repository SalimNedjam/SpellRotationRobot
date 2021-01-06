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
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""Stopped!"")");
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
    public Spell Fireball = new Spell("Fireball");
    public Spell Pyroblast = new Spell("Pyroblast");
    public Spell FireBlast = new Spell("Fire Blast");
    public Spell Scorch = new Spell("Scorch");
    public Spell Combustion = new Spell("Combustion");
    public Spell Counterspell = new Spell("Counterspell");
    public Spell RuneofPower = new Spell("Rune of Power");
    public Spell Meteor = new Spell("Meteor");
    public Spell FocusedAzeriteBeam = new Spell("Focused Azerite Beam");
    public Spell Flamestrike = new Spell("Flamestrike");
    public Spell MemoryofLucidDreams = new Spell("Memory of Lucid Dreams");
    public Spell DragonsBreath = new Spell("Dragon's Breath");
    public Spell ArcaneIntellect = new Spell("Arcane Intellect");

    public Spell ShadowWordVoid = new Spell("Shadow Word: Void");
    public Spell ShadowWordPain = new Spell("Shadow Word: Pain");
    public Spell MindFly = new Spell("Mind Flay");
    public Spell VoidEruption = new Spell("Void Eruption");
    public Spell VoidBot = new Spell("Void Bolt");

    public Spell VampiricTouch = new Spell("Vampiric Touch");
    public Spell Shadowfiend = new Spell("Shadowfiend");


    /* Rotation() */
    public void Rotation()
    {
        Logging.Write(name + ": Started.");
        Lua.LuaDoString(@"dRotationFrame.text:SetText(""Started!"")");

        while (_isRunning)
        {
            try
            {
                if (Products.InPause)
                    Lua.LuaDoString(@"dRotationFrame.text:SetText(""dRotation Paused!"")");
                //(!ObjectManager.Me.IsDead && Party.IsInGroup() && !Usefuls.IsLoadingOrConnecting && Usefuls.InGame)
                else if (!ObjectManager.Me.IsDead)
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

    public void Damage()
    {
        if (VampiricTouch.IsSpellUsable && !ObjectManager.Target.HaveBuff(VampiricTouch.Name))
        {
            MyHelpers.castSpell(VampiricTouch.Name);

            return;

        }

        if ( VoidEruption.IsSpellUsable )
        {
            MyHelpers.castSpell(VoidEruption.Name);

            return;

        }

        if (ShadowWordVoid.IsSpellUsable)
        {
            MyHelpers.castSpell(ShadowWordVoid.Name);

            return;

        }
        if (Shadowfiend.IsSpellUsable)
        {
            MyHelpers.castSpell(Shadowfiend.Name);

            return;

        }
        if (MindFly.IsSpellUsable && !MyHelpers.isChanneling())
        {
            MyHelpers.castSpell(MindFly.Name);

            return;

        }





    }
    public void CombatRotation()
    {
        if (ObjectManager.Me.HasTarget && (ObjectManager.Target.IsAttackable || ObjectManager.Target.IsLocalPlayer))
            Damage();
    }
}

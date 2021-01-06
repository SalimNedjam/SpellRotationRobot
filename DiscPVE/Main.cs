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
    public Spell PowerWordShield = new Spell("Power Word: Shield");
    public Spell PowerWordFortitude = new Spell("Power Word: Fortitude");
    public Spell ShadowMend = new Spell("Shadow Mend");
    public Spell Smite = new Spell("Smite");
    public Spell PowerWordSolace = new Spell("Power Word: Solace");
    public Spell Schism = new Spell("Schism");
    public Spell Penance = new Spell("Penance");
    public Spell PurgetheWicked = new Spell("Purge the Wicked");
    public Spell Shadowfiend = new Spell("Shadowfiend");

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
                //(!ObjectManager.Me.IsDead && Party.IsInGroup() && !Usefuls.IsLoadingOrConnecting && Usefuls.InGame)
                else if (!ObjectManager.Me.IsDead)
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


    public bool needBurst(float percent)
    {
        WoWPlayer p;
        WoWObject obj;
        uint count = 0;
        List<MemoryRobot.Int128> party = Party.GetPartyGUID();
        party.Add(ObjectManager.Me.Guid);
        foreach (var playerGuid in party)
        {
            if (playerGuid.IsNotZero())
            {
                obj = ObjectManager.GetObjectByGuid(playerGuid);
                if (obj.IsValid)
                {
                    p = new WoWPlayer(obj.GetBaseAddress);

                    if (p.Health < p.MaxHealth * percent)
                    {
                        count += 1;
                    }

                }
            }
        }
        return count > 1;
    }
    public void Buff()
    {
        WoWPlayer p;
        WoWObject obj;
        List<MemoryRobot.Int128> party = Party.GetPartyGUID();
        party.Add(ObjectManager.Me.Guid);
        foreach (var playerGuid in party)
        {
            if (playerGuid.IsNotZero())
            {
                obj = ObjectManager.GetObjectByGuid(playerGuid);
                if (obj.IsValid)
                {
                    p = new WoWPlayer(obj.GetBaseAddress);
                    ObjectManager.Me.FocusGuid = p.Guid;

                    if (!p.HaveBuff("Power Word: Fortitude") && PowerWordFortitude.KnownSpell && PowerWordFortitude.IsSpellUsable)
                    {
                        PowerWordFortitude.Launch(false, false, true, "focus");
                        return;
                    }
                    if (!needBurst(0.8f) && p.Health < p.MaxHealth * 0.8 && ShadowMend.KnownSpell && ShadowMend.IsSpellUsable)
                    {
                        ShadowMend.Launch(false, false, true, "focus");
                        return;
                    }
                    if (p.Health < p.MaxHealth * 0.9 && !p.HaveBuff(194384) && PowerWordShield.KnownSpell && PowerWordShield.IsSpellUsable)
                    {
                        PowerWordShield.Launch(false, false, true, "focus");
                        return;
                    }
                   

                    

                }
            }
        }
    }
    public void Damage()
    {
        if (PurgetheWicked.KnownSpell
            && PurgetheWicked.IsSpellUsable
            && !ObjectManager.Target.HaveBuff("Purge the Wicked"))
        {
            PurgetheWicked.Launch();
            return;
        }
        if (needBurst(0.8f))
        {
            if (Shadowfiend.KnownSpell
            && Shadowfiend.IsSpellUsable)
            {
                Shadowfiend.Launch();
                return;
            }
            if (Schism.KnownSpell
            && Schism.IsSpellUsable)
            {
                Schism.Launch();
                return;
            }
            if (PowerWordSolace.KnownSpell
                && PowerWordSolace.IsSpellUsable)
            {
                PowerWordSolace.Launch();
                return;
            }
            if (Penance.KnownSpell
                && Penance.IsSpellUsable)
            {
                Penance.Launch();
                return;
            }
        }
        if (Smite.KnownSpell
            && Smite.IsSpellUsable)
        {
            Smite.Launch();
            return;
        }
    }
    public void CombatRotation()
    {
        Buff();
        if (ObjectManager.Me.HasTarget && ObjectManager.Target.IsAttackable)
            Damage();
    }
}

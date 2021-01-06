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
    private bool canBlast = false, doDecast = true, needBlast = false, buffTime = false, doBurst = true;

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
    public Spell Berserking = new Spell("Berserking");



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
    private bool needLucid()
    {
        return MemoryofLucidDreams.IsSpellUsable && buffTime;

    }

    private bool needRune()
    {
        return MyHelpers.spellCharges(RuneofPower.Name) > 0
            && (MyHelpers.haveBuff(MemoryofLucidDreams.Name) || (MyHelpers.GetSpellCooldown(Combustion.Name) > 10 && MyHelpers.spellCharges(FireBlast.Name) == 0))
            && !MyHelpers.haveBuff(RuneofPower.Name)
            && !MyHelpers.haveBuff(Combustion.Name)
            && RuneofPower.IsSpellUsable
            && ObjectManager.Me.SpeedMoving == 0
            && (MyHelpers.haveBuff(MemoryofLucidDreams.Name)
                ||
                    ((MyHelpers.spellCharges(RuneofPower.Name) == 2)
                    || (MyHelpers.spellCharges(RuneofPower.Name) == 1
                        && MyHelpers.GetSpellCooldownCharges(RuneofPower.Name) + 10 < MyHelpers.GetSpellCooldown(Combustion.Name))));
    }


    public bool needAzshara()
    {
        return Fireball.IsSpellUsable
            && !MyHelpers.haveBuff("Latent Arcana")
            && MemoryofLucidDreams.IsSpellUsable
            && Combustion.IsSpellUsable
            && doBurst
            && ObjectManager.Me.SpeedMoving == 0;


    }
    public bool needCombustion()
    {
        return Combustion.IsSpellUsable && !MyHelpers.haveBuff(Combustion.Name) && MyHelpers.haveBuff(MemoryofLucidDreams.Name);
    }
    public void burstPrepare()
    {
        buffTime = true;
        doBurst = false;
        while (ObjectManager.Me.IsCast) ;
        Thread.Sleep(1000);
        if (MyHelpers.itemIsUsable("Azshara's Font of Power"))
        {
            ItemsManager.UseItem(ItemsManager.GetIdByName("Azshara's Font of Power"));
            Thread.Sleep(4020);
        }

        if (MyHelpers.itemIsEquiped("Notorious Gladiator's Badge"))
            Thread.Sleep(13000);

        if (!MyHelpers.haveBuff("Hot Streak!") && !MyHelpers.haveBuff("Heating Up"))
        {
            MyHelpers.castSpell(Fireball.Name);
            Thread.Sleep(2300);
        }
        if (MyHelpers.itemIsEquiped("Notorious Gladiator's Badge"))
        {
            while (!MyHelpers.itemIsUsable("Notorious Gladiator's Badge")) ;

            while (MyHelpers.itemIsUsable("Notorious Gladiator's Badge"))
                ItemsManager.UseItem(ItemsManager.GetIdByName("Notorious Gladiator's Badge"));
        }

        if (!MyHelpers.haveBuff("Hot Streak!") && !MyHelpers.haveBuff("Heating Up"))
        {
            needBlast = true;
        }
        while (!needLucid() || ObjectManager.Me.IsCast) ;


        MyHelpers.castSpell(MemoryofLucidDreams.Name);
        Thread.Sleep(100);

        if (!MyHelpers.haveBuff("Hot Streak!"))
            MyHelpers.castSpell(FireBlast.Name);
        MyHelpers.castSpell(Berserking.Name);
        while (!needRune() || ObjectManager.Me.IsCast) ;

        MyHelpers.castSpell(RuneofPower.Name);
        while (!needCombustion() || ObjectManager.Me.IsCast) ;

        MyHelpers.castSpell(Combustion.Name);

        if (needBlast && !MyHelpers.haveBuff("Hot Streak!"))
        {
            MyHelpers.castSpell(FireBlast.Name);
        }
        needBlast = false;
        buffTime = false;
        return;
    }
    public void Buffs()
    {
        if (!MyHelpers.haveBuff(ArcaneIntellect.Name))
        {
            MyHelpers.castSpell(ArcaneIntellect.Name);
            return;
        }
        if (needAzshara())
        {
            burstPrepare();
            return;
        }

        if (needRune())
        {
            MyHelpers.castSpell(RuneofPower.Name);
            return;
        }
    }
    public void Damage()
    {



        if (Meteor.IsSpellUsable
            && MyHelpers.haveBuff(RuneofPower.Name)
           && (MyHelpers.haveBuffStack("Blaster Master") == 3 || (!MyHelpers.haveBuff(Combustion.Name) || MyHelpers.spellCharges(FireBlast.Name) == 0))
           && MyHelpers.getTargetDistance() <= 40.0f
           )
        {
            MyHelpers.castSpell(Meteor.Name);
            ClickOnTerrain.Pulse(ObjectManager.Target.Position);

            return;
        }
        if (MyHelpers.haveBuff("Hot Streak!") && !buffTime)
        {
            while (MyHelpers.haveBuff(Combustion.Name))
            {

                if (MyHelpers.haveBuff("Hot Streak!"))
                {
                    Pyroblast.Launch();
                    Thread.Sleep(200);

                    canBlast = true;
                    continue;

                }

                else if (!ObjectManager.Me.IsCast && MyHelpers.spellCharges(FireBlast.Name) > 0 && canBlast && !MyHelpers.haveBuff("Hot Streak!"))
                {
                    canBlast = false;
                    MyHelpers.castSpell(FireBlast.Name);
                    continue;
                }
                else if (MyHelpers.itemIsUsable("Hyperthread Wristwraps") && MyHelpers.spellCharges(FireBlast.Name) == 0)
                    ItemsManager.UseItem(ItemsManager.GetIdByName("Hyperthread Wristwraps"));
                else if (Meteor.IsSpellUsable && MyHelpers.spellCharges(FireBlast.Name) == 0)
                {
                    MyHelpers.castSpell(Meteor.Name);
                    ClickOnTerrain.Pulse(ObjectManager.Target.Position);
                    continue;
                }
                else if (!ObjectManager.Me.IsCast && doDecast && Counterspell.IsSpellUsable && MyHelpers.spellCharges(FireBlast.Name) == 0)
                {
                    MyHelpers.castSpell(Counterspell.Name);
                    continue;
                }
                else if (MyHelpers.spellCharges(FireBlast.Name) == 0
                    && MyHelpers.GetSpellCooldownCharges(FireBlast.Name) > MyHelpers.castTime(Scorch.Name) * 2
                    && !MyHelpers.haveBuff("Hot Streak!"))
                {
                    canBlast = false;
                    MyHelpers.castSpell(Scorch.Name);
                    while (ObjectManager.Me.IsCast)
                        Thread.Sleep(50);

                    continue;

                }
                canBlast = true;

            }
            if (MyHelpers.haveBuff("Hot Streak!"))
            {
                MyHelpers.castSpell(Pyroblast.Name);
            }
            return;

        }
        if (MyHelpers.spellCharges(FireBlast.Name) > 0 && !buffTime && MyHelpers.haveBuff("Heating Up") && !MyHelpers.haveBuff(Combustion.Name))
        {
            MyHelpers.castSpell(FireBlast.Name);
            return;
        }

        if (!ObjectManager.Me.IsCast && doDecast && Counterspell.IsSpellUsable && MyHelpers.spellCharges(FireBlast.Name) < 3 &&
            (MyHelpers.haveBuff(Combustion.Name) || MyHelpers.GetSpellCooldown(Combustion.Name) > 25))
        {
            MyHelpers.castSpell(Counterspell.Name);
            return;
        }


        if (!MyHelpers.haveBuff("Hot Streak!")
            && Scorch.IsSpellUsable
            && !buffTime
            && ((ObjectManager.Me.SpeedMoving > 0 || ObjectManager.Target.HealthPercent < 0.3)
                || (MyHelpers.haveBuff(Combustion.Name)
                    && MyHelpers.spellCharges(FireBlast.Name) == 0
                    && MyHelpers.GetSpellCooldownCharges(FireBlast.Name) > MyHelpers.castTime(Scorch.Name))

            ))
        {
            MyHelpers.castSpell(Scorch.Name);

            return;
        }


        if (!MyHelpers.haveBuff("Hot Streak!") && !buffTime && !MyHelpers.haveBuff(Combustion.Name))
        {

            MyHelpers.castSpell(Fireball.Name);
            return;
        }



    }
    public void CombatRotation()
    {
        if (ObjectManager.Me.HasTarget && (ObjectManager.Target.IsAttackable || ObjectManager.Target.IsLocalPlayer))
        {
            Buffs();
            Damage();
        }
    }
}

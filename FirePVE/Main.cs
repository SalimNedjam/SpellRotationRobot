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
    private bool canBlast = false,  needBlast = false, buffTime = false, doBurst = true;

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
    public Spell BlazingBarrier = new Spell("Blazing Barrier");



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
        return !MyHelpers.haveBuff("Latent Arcana")
            && MyHelpers.itemIsUsable("Notorious Gladiator's Badge")
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

        while(!MyHelpers.itemIsUsable("Azshara's Font of Power") || ObjectManager.Me.IsCast) Thread.Sleep(100);
        while (MyHelpers.itemIsUsable("Azshara's Font of Power"))
            ItemsManager.UseItem(ItemsManager.GetIdByName("Azshara's Font of Power"));
        
        Thread.Sleep(4020);

        if (!MyHelpers.inCombat())
        {
            if (MyHelpers.itemIsEquiped("Notorious Gladiator's Badge"))
                Thread.Sleep(13000);

            if (!MyHelpers.haveBuff("Hot Streak!") && !MyHelpers.haveBuff("Heating Up"))
            {
                MyHelpers.castSpell(Fireball.Name);
                Thread.Sleep(2300);
            }
        }
        if (MyHelpers.itemIsEquiped("Notorious Gladiator's Badge"))
        {
            while (!MyHelpers.itemIsUsable("Notorious Gladiator's Badge"))
            {
                if (MyHelpers.inCombat())
                {
                    
                    if(ObjectManager.Target.HealthPercent < 30 || ObjectManager.Me.SpeedMoving > 0)
                    {
                        if (!MyHelpers.haveBuff("Hot Streak!")
                            && Scorch.IsSpellUsable
                            && (ObjectManager.Me.SpeedMoving > 0 || ObjectManager.Target.HealthPercent < 30))


                        {
                            Thread.Sleep(100);
                            if (!MyHelpers.haveBuff("Hot Streak!")
                            && Scorch.IsSpellUsable
                            && (ObjectManager.Me.SpeedMoving > 0 || ObjectManager.Target.HealthPercent < 30))
                                MyHelpers.castSpell(Scorch.Name);

                            continue;
                        }
                        if(ObjectManager.Target.HealthPercent < 30 && MyHelpers.haveBuff("Hot Streak!"))
                        {
                            MyHelpers.castSpell(Pyroblast.Name);
                            continue;
                        }
                            

                    }
                    else
                    {
                        if(MyHelpers.haveBuff("Hot Streak!") && MyHelpers.BuffTimeLeft("Hot Streak!") < 10 )
                        {
                            MyHelpers.castSpell(Pyroblast.Name);
                            continue;
                        }
                        else if (MyHelpers.spellCharges(FireBlast.Name) > 0 && MyHelpers.haveBuff("Heating Up"))
                        {
                            MyHelpers.castSpell(FireBlast.Name);
                            continue;
                        }
                        else
                        {
                            MyHelpers.castSpell(Fireball.Name);
                            continue;
                        }
                    }
                    
                }
                    

            };

            while (!MyHelpers.itemIsUsable("Notorious Gladiator's Badge") || ObjectManager.Me.IsCast) Thread.Sleep(10);
            while (MyHelpers.itemIsUsable("Notorious Gladiator's Badge"))
                ItemsManager.UseItem(ItemsManager.GetIdByName("Notorious Gladiator's Badge"));
        }

        if (!MyHelpers.haveBuff("Hot Streak!") && !MyHelpers.haveBuff("Heating Up"))
        {
            needBlast = true;
        }

        while (!MemoryofLucidDreams.IsSpellUsable || ObjectManager.Me.IsCast) Thread.Sleep(10);
        while (!MyHelpers.haveBuff(MemoryofLucidDreams.Name))
            MyHelpers.castSpell(MemoryofLucidDreams.Name);

        Thread.Sleep(100);

        if (!MyHelpers.haveBuff("Hot Streak!"))
            MyHelpers.castSpell(FireBlast.Name);


        while (Berserking.IsSpellUsable)
            MyHelpers.castSpell(Berserking.Name);



        while (!RuneofPower.IsSpellUsable) Thread.Sleep(10);
        while(!MyHelpers.haveBuff(RuneofPower.Name))
            MyHelpers.castSpell(RuneofPower.Name);



        while (!Meteor.IsSpellUsable || ObjectManager.Me.IsCast) Thread.Sleep(10);
        while (Meteor.IsSpellUsable)
        {
            MyHelpers.castSpell(Meteor.Name);
            ClickOnTerrain.Pulse(ObjectManager.Target.Position);
        }
            

        while (!Combustion.IsSpellUsable || ObjectManager.Me.IsCast) Thread.Sleep(10);
        while(!MyHelpers.haveBuff(Combustion.Name))
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
                    if(MyHelpers.spellCharges(FireBlast.Name) == 0)
                    {
                        Thread.Sleep(200);
                    }

                    canBlast = true;
                    continue;

                }
                if (!ObjectManager.Me.IsCast && MyHelpers.spellCharges(FireBlast.Name) > 0  && MyHelpers.haveBuff("Heating Up"))
                {
                    canBlast = false;
                    MyHelpers.castSpell(FireBlast.Name);
                    continue;
                }
                if ( MyHelpers.itemIsUsable("Hyperthread Wristwraps") && MyHelpers.spellCharges(FireBlast.Name) == 0  && !ObjectManager.Me.IsCast)
                {
                    ItemsManager.UseItem(ItemsManager.GetIdByName("Hyperthread Wristwraps"));
                    continue;
                }
                if (MyHelpers.spellCharges(FireBlast.Name) == 0
                    && !MyHelpers.haveBuff("Hot Streak!") && !ObjectManager.Me.IsCast)
                {
                    Thread.Sleep(400);

                    if (MyHelpers.spellCharges(FireBlast.Name) == 0
                        && !MyHelpers.haveBuff("Hot Streak!") && !ObjectManager.Me.IsCast)
                    {
                        MyHelpers.castSpell(Scorch.Name);
                        while (ObjectManager.Me.IsCast)
                            Thread.Sleep(50);

                        continue;

                    }
                    else 
                        continue;
                }

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
        if (BlazingBarrier.IsSpellUsable)
        {
            MyHelpers.castSpell(BlazingBarrier.Name);

        }

        if (!MyHelpers.haveBuff("Hot Streak!")
            && Scorch.IsSpellUsable
            && !buffTime
            && (ObjectManager.Me.SpeedMoving > 0 || ObjectManager.Target.HealthPercent < 30))

            
        {
            Thread.Sleep(100);
            if (!MyHelpers.haveBuff("Hot Streak!")
            && Scorch.IsSpellUsable
            && !buffTime
            && (ObjectManager.Me.SpeedMoving > 0 || ObjectManager.Target.HealthPercent < 30))
                MyHelpers.castSpell(Scorch.Name);

            return;
        }


        if (!MyHelpers.haveBuff("Hot Streak!") && !buffTime && !MyHelpers.haveBuff(Combustion.Name) && !(ObjectManager.Me.SpeedMoving > 0 || ObjectManager.Target.HealthPercent < 30))
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

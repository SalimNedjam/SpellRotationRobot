using System;
using System.Threading;
using System.Windows.Forms;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;


public class Main : wManager.Plugin.IPlugin
{
    private static bool _launched;
    private static int timeStrafe;
    private static int isLeft;

    public void Initialize()
    {


        _launched = true;
        Logging.Write("[MoveDuringCombat] Loadded.");
        while (_launched)
        {
            try
            {
                if (!Products.InPause && Battleground.IsInBattleground())
                {
                    if (!ObjectManager.Me.IsDeadMe && Fight.InFight && ObjectManager.Me.Target.IsNotZero() && ObjectManager.Target.GetDistance < 5.0f)
                    {
                        Move.Forward(Move.MoveAction.DownKey);

                        timeStrafe = Others.Random(200, 400);
                        isLeft = Others.Random(0, 100);
                        MovementManager.Face(ObjectManager.Target.Position);

                        if (isLeft < 50)
                        {
                            Move.StrafeLeft(Move.MoveAction.DownKey);
                            Thread.Sleep(timeStrafe);
                            Move.StrafeLeft(Move.MoveAction.UpKey);
                        }
                        else
                        {
                            Move.StrafeRight(Move.MoveAction.DownKey);
                            Thread.Sleep(timeStrafe);
                            Move.StrafeRight(Move.MoveAction.UpKey);
                        }
                        Move.Forward(Move.MoveAction.UpKey);
                    }
                    else
                    {
                        Lua.LuaDoString("if (GetBattlefieldWinner()) then LeaveBattlefield() end");
                        Thread.Sleep(300);
                    }

                }
            }
            catch (Exception e)
            {
                ObjectManager.Me.ForceIsCast = false;
                Logging.WriteError("MoveDuringCombat.Routine.Pulse(): " + e);
            }
        }
        Logging.Write("[MoveDuringCombat] Disposed.");
    }


    public void Dispose()
    {
        _launched = false;
        Logging.Write("[MoveDuringCombat] Disposed.");

    }

    public void Settings()
    {

    }
}

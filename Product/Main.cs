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
    public enum MoveAction
    {
        Left = 0,
        Right = 1,
        Forward = 2
    }
    public void strafe(MoveAction direction, int timeStrafe)
    {
        int portionSleep = timeStrafe / 20;
        switch (direction)
        {
            case MoveAction.Left:
                Move.StrafeLeft(Move.MoveAction.DownKey);
                for (int i = 0; i < timeStrafe; i += portionSleep)
                {
                    Thread.Sleep(portionSleep);
                    MovementManager.Face(ObjectManager.Target.Position);

                }
                Move.StrafeLeft(Move.MoveAction.UpKey);
                break;
            case MoveAction.Right:

                Move.StrafeRight(Move.MoveAction.DownKey);
                for (int i = 0; i < timeStrafe; i += portionSleep)
                {
                    Thread.Sleep(portionSleep);
                    MovementManager.Face(ObjectManager.Target.Position);

                }
                Move.StrafeRight(Move.MoveAction.UpKey);

                break;

        }
    }
    public void Initialize()
    {


        _launched = true;
        Logging.Write("[MoveDuringCombat] Loadded.");
        while (_launched)
        {
            try
            {
                if (!Products.InPause)
                {
                    if (!ObjectManager.Me.IsDeadMe && Fight.InFight && ObjectManager.Me.Target.IsNotZero() && ObjectManager.Target.GetDistance < 5.0f)
                    {

                        while (ObjectManager.Target.GetDistance < 5.0f)
                        {
                            timeStrafe = Others.Random(200, 400);
                            isLeft = Others.Random(0, 100);
                            if (isLeft < 50)
                            {
                                strafe(MoveAction.Left, timeStrafe);
                            }
                            else
                            {
                                strafe(MoveAction.Right, timeStrafe);

                            }
                            
                        }

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

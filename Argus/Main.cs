
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using wManager.Wow.Enums;
public class Main : wManager.Plugin.IPlugin
{
    private static bool _launched;



    List<Vector3> firstPlace = new List<Vector3>();
    List<Vector3> firstPlace2 = new List<Vector3>();



    public void Initialize()
    {


        _launched = true;
        Logging.Write("[GuildInvite] Loadded.");
        firstPlace.Add(new Vector3(5428.075, 10821.4, 20.17536));
        firstPlace.Add(new Vector3(5447.377, 10849.8, 20.11224));
        firstPlace.Add(new Vector3(5500.092, 10869.47, 20.11204));
        firstPlace.Add(new Vector3(5545.043, 10858.2, 20.11127));
        firstPlace.Add(new Vector3(5583.926, 10838.98, 20.11079));
        firstPlace.Add(new Vector3(5588.163, 10789.27, 20.13315));
        firstPlace.Add(new Vector3(5580.211, 10755.97, 20.11228));
        firstPlace2.Add(new Vector3(5570.698, 10722.34, 21.61471));
        firstPlace2.Add(new Vector3(5541.527, 10720.99, 21.44868));
        firstPlace2.Add(new Vector3(5499.408, 10730.73, 20.12903));
        firstPlace2.Add(new Vector3(5470.442, 10738.67, 20.11287));
        firstPlace2.Add(new Vector3(5451.601, 10755.5, 20.11287));
        firstPlace2.Add(new Vector3(5438.486, 10777.81, 20.11041));
        firstPlace2.Add(new Vector3(5473.602, 10787.64, 17.24798));
        firstPlace2.Add(new Vector3(5474.695, 10827.89, 17.16621));
        firstPlace2.Add(new Vector3(5523.902, 10785.93, 17.16895));

        Start();
        Logging.Write("[GuildInvite] Disposed.");
    }


    public void Dispose()
    {
        _launched = false;

        Logging.Write("[MoveDuringCombat] Disposed.");

    }
    public void Settings()
    {

    }
    public bool isInInstance()
    {
        return Lua.LuaDoString<bool>(@"local ret = IsInInstance(); return ret");
    }
    public void Start()
    {
        GoIn();
        doPath(firstPlace);
        doPath(firstPlace2);
        GoOut();


    }
    public void GoIn()
    {
        MountUp();
        Vector3 entrance = new Vector3(5410.303, 10815.26, 20.4625);
        MovementManager.MoveTo(entrance);
        Thread.Sleep(5000);
    }
    public void GoOut()
    {
        MountUp();

        Vector3 exit = new Vector3(5423.4, 10815.26, 20.4625);
        MovementManager.MoveTo(exit);
        Thread.Sleep(5000);

    }
    public void MountUp()
    {
        while (!ObjectManager.Me.IsMounted || Bag.GetContainerNumFreeSlots < 70)
        {
            wManager.Wow.Bot.Tasks.MountTask.Mount();
            WoWUnit repair = ObjectManager.GetObjectWoWUnit().Find(g => g.Entry == 32642);
            if (repair != null)
                wManager.Wow.Bot.Tasks.GoToTask.ToPositionAndIntecractWithNpc(ObjectManager.Me.Position, repair.Entry);

        }
    }
    
    public void doPath(List<Vector3> path)
    {
        MountUp();


        foreach (Vector3 next in path)
        {
            if (!ObjectManager.Me.IsMounted)
            {
                if (ObjectManager.Me.InCombat)
                {
                    MovementManager.StopMoveTo();
                    MovementManager.Face(ObjectManager.Target.Position);
                    Thread.Sleep(100);
                }
            }
            MovementManager.MoveTo(next);
            Thread.Sleep(100);
            while (ObjectManager.Me.SpeedMoving > 0)
            {
                if (!ObjectManager.Me.IsMounted)
                {
                    if (ObjectManager.Me.InCombat)
                    {
                        MovementManager.StopMoveTo();
                        MovementManager.Face(ObjectManager.Target.Position);
                        Thread.Sleep(5000);
                    }
                }
            }
            MovementManager.MoveTo(next);


        }

        Thread.Sleep(2000);
        wManager.Wow.Bot.Tasks.MountTask.DismountMount();
        while (ObjectManager.Me.InCombat)
        {
            MovementManager.StopMoveTo();
            MovementManager.Face(ObjectManager.Target.Position);
            Thread.Sleep(100);
        }
        Thread.Sleep(1000);
        MountUp();

    }
}
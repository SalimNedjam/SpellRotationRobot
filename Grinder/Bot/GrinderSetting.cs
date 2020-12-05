using System;
using System.IO;
using robotManager.Helpful;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace Grinder.Bot
{
    [Serializable]
    public class GrinderSetting : Settings
    {
        public static GrinderSetting CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("Grinder", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("GrinderSetting > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("Grinder", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<GrinderSetting>(AdviserFilePathAndName("Grinder",
                                                                    ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new GrinderSetting();
            }
            catch (Exception e)
            {
                Logging.WriteError("GrinderSetting > Load(): " + e);
            }
            return false;
        }

        public string ProfileName = "";
        public bool BlackListWhereIAmDead;
        public bool RetLastPos;
        public bool PetBattle;
    }
}
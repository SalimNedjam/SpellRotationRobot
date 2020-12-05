using System;
using System.Collections.Generic;
using robotManager.Helpful;
using wManager.Wow.Class;
using wManager.Wow.Enums;

namespace Grinder.Profile
{
    [Serializable]
    public class GrinderProfile
    {
        public List<GrinderZone> GrinderZones = new List<GrinderZone>();
    }

    [Serializable]
    public class GrinderZone
    {
        public string Name = "";
        public bool Hotspots;
        public uint MinLevel;
        public uint MaxLevel = 999;
        public uint MinTargetLevel;
        public uint MaxTargetLevel = 999;
        public List<int> TargetEntry = new List<int>();
        public List<uint> TargetFactions = new List<uint>();
        public List<Vector3> Vectors3 = new List<Vector3>();
        public List<Npc> Npc = new List<Npc>();
        public List<GrinderBlackListRadius> BlackListRadius = new List<GrinderBlackListRadius>();

        private bool _notLoop;
        public bool NotLoop
        {
            get { return _notLoop; }
            set { _notLoop = value; }
        }

        internal bool IsValid()
        {
            try
            {
                return Vectors3.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    [Serializable]
    public class GrinderBlackListRadius
    {
        public Vector3 Position = new Vector3();
        public float Radius;
        public ContinentId Continent = ContinentId.None;
    }
}
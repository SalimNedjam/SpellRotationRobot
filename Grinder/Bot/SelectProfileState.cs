using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using wManager.Wow.ObjectManager;

namespace Grinder.Bot
{
    internal class SelectProfileState : State
    {
        public override string DisplayName
        {
            get { return "SelectProfileState"; }
        }

        private uint _lastLevel;

        public override bool NeedToRun
        {
            get
            {
                if (_lastLevel != ObjectManager.Me.Level)
                    return true;
                return false;
            }
        }

        public override void Run()
        {
            Bot.SelectZone();
            _lastLevel = ObjectManager.Me.Level;
            Logging.Write("[Grinder] Select zone: " + Bot.Profile.GrinderZones[Bot.ZoneIdProfile].Name);
        }
    }
}
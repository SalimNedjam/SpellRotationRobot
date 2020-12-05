using MoveDuringCombat;
using robotManager.Helpful;

public class Main : wManager.Plugin.IPlugin
{
    public void Initialize()
    {
        Logging.Write("[MoveDuringCombat] Loading.");
        Routine.Pulse();
    }

    public void Dispose()
    {
        Routine.Dispose();
        Logging.Write("[MoveDuringCombat] Dispose.");
    }

    public void Settings()
    {

    }
}

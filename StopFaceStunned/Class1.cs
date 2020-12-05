using wManager.Wow.ObjectManager;

public class Main : wManager.Plugin.IPlugin
{
    public void Initialize()
    {
        wManager.Events.MovementEvents.OnPulseStuckResolver += MovementEvents_OnPulseStuckResolver;
    }

    public void Dispose()
    {
    }

    public void Settings()
    {
    }

    WoWLocalPlayer m { get { return ObjectManager.Me; } }

    private void MovementEvents_OnPulseStuckResolver(System.ComponentModel.CancelEventArgs cancelable)
    {
        try
        {
            if (m.IsStunned || m.Rooted || m.Confused)
                cancelable.Cancel = true;
        }
        catch { }
    }
}
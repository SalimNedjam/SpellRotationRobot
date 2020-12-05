// ReSharper disable CheckNamespace

using System;
using System.IO;
using System.Windows.Forms;
using Grinder;
using Grinder.Bot;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Plugin;

public class Main : IProduct
// ReSharper restore CheckNamespace
{
    #region IProduct Members

    public void Initialize()
    {
        try
        {
            Directory.CreateDirectory(Application.StartupPath + @"\Profiles\Grinder\");

            GrinderSetting.Load();

            if (!string.IsNullOrWhiteSpace(ArgsParser.Profile) && ArgsParser.Product.ToLower() == Products.ProductName.ToLower())
                GrinderSetting.CurrentSetting.ProfileName = ArgsParser.Profile;

            Logging.Status = "[Grinder] Loaded";
            Logging.Write("[Grinder] Loaded");
        }
        catch (Exception e)
        {
            Logging.WriteError("Grinder > Main > Initialize(): " + e);
        }
    }

    public void Dispose()
    {
        try
        {
            Stop();
            Logging.Status = "[Grinder] Closed";
            Logging.Write("[Grinder] Closed");
        }
        catch (Exception e)
        {
            Logging.WriteError("Grinder > Main > Dispose(): " + e);
        }
    }

    public void Start()
    {
        try
        {
            _isStarted = true;
            if (Bot.Pulse())
            {
                PluginsManager.LoadAllPlugins();
                Logging.Status = "[Grinder] Started";
                Logging.Write("[Grinder] Started");
            }
            else
            {
                _isStarted = false;
                Logging.Status = "[Grinder] Failed to start";
                Logging.Write("[Grinder] Failed to start");
            }
        }
        catch (Exception e)
        {
            _isStarted = false;
            Logging.WriteError("Grinder > Main > Start(): " + e);
        }
    }

    public void Stop()
    {
        try
        {
            Bot.Dispose();
            _isStarted = false;
            PluginsManager.DisposeAllPlugins();
            Logging.Status = "[Grinder] Stopped";
            Logging.Write("[Grinder] Stopped");
        }
        catch (Exception e)
        {
            Logging.WriteError("Grinder > Main > Stop(): " + e);
        }
    }

    SettingsUserControl _settingsUserControl;
    public System.Windows.Controls.UserControl Settings
    {
        get
        {
            try
            {
                if (_settingsUserControl == null)
                    _settingsUserControl = new SettingsUserControl();
                return _settingsUserControl;
            }
            catch (Exception e)
            {
                Logging.WriteError("Grinder > Main > Settings(): " + e);
            }
            return null;
        }
    }

    public bool IsStarted
    {
        get { return _isStarted; }
    }

    private bool _isStarted;

    #endregion
}
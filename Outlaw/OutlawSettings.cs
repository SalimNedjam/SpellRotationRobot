using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class OutlawSettings : Settings
{
    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Sinister Strike")]
    public bool EnableSinisterStrike { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Pistol Shot")]
    public bool EnablePistolShot { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Between the Eyes")]
    public bool EnableBetweenTheEyes { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Dispatch")]
    public bool EnableDispatch { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Roll the Bones")]
    public bool EnableRolltheBones { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Adrenaline Rush")]
    public bool EnableAdrenalineRush { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Blood of the Enemy")]
    public bool EnableBloodoftheEnemy { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Blade Flurry")]
    public bool EnableBladeFlurry { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Razor Coral")]
    public bool EnableRazorCoral { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Interrupt")]
    public bool EnableInterrupt { get; set; }


    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Riposte")]
    public bool EnableRiposte { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Feint")]
    public bool EnableFeint { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("Crimson Vial")]
    public bool EnableCrimsonVial { get; set; }



    [Setting]
    [DefaultValue(true)]
    [Category("Spells")]
    [DisplayName("CloakOfShadows")]
    public bool EnableCloakOfShadows { get; set; }
    private OutlawSettings()
    {
        EnableSinisterStrike = true;
        EnablePistolShot = true;
        EnableBetweenTheEyes = true;
        EnableDispatch = true;
        EnableBloodoftheEnemy = true;
        EnableAdrenalineRush = true;
        EnableRolltheBones = true;
        EnableBladeFlurry = true;
        EnableRazorCoral = true;
        EnableInterrupt = true;
        EnableCrimsonVial = true;
        EnableFeint = true;
        EnableRiposte = true;
        EnableCloakOfShadows = true;

    }
    public static OutlawSettings CurrentSetting { get; set; }
    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("OutlawSettings", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("OutlawSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("OutlawSettings", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<OutlawSettings>(AdviserFilePathAndName("OutlawSettings",
                                                                 ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new OutlawSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("OutlawSettings > Load(): " + e);
        }
        return false;
    }
}

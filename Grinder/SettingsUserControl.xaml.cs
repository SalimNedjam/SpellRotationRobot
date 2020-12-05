using System;
using System.Windows.Forms;
using System.Windows.Input;
using Grinder.Bot;
using Grinder.Profile;
using robotManager.Helpful;
using robotManager.Helpful.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace Grinder
{
    /// <summary>
    /// Logique d'interaction pour SettingsUserControl.xaml
    /// </summary>
    public partial class SettingsUserControl : UserControl
    {
        public SettingsUserControl()
        {
            InitializeComponent();
            Translation();
            Load();
        }

        void Translation()
        {
            ProfileLabel.Translate();
            ProfileCreatorButton.Translate();
            BlackListWhereIAmDeadCheckbox.Translate();
            PetBattle.Translate();
            DownloadMoreLabel.Translate();
            ReturnLastPosCheckBox.Translate();
        }

        void Save()
        {
            try
            {
                GrinderSetting.CurrentSetting.ProfileName = ProfileComboBox.Text;
                GrinderSetting.CurrentSetting.BlackListWhereIAmDead = BlackListWhereIAmDeadCheckbox.IsChecked == true;
                GrinderSetting.CurrentSetting.RetLastPos = ReturnLastPosCheckBox.IsChecked == true;
                GrinderSetting.CurrentSetting.PetBattle = PetBattle.IsChecked == true;
                GrinderSetting.CurrentSetting.Save();
            }
            catch (Exception e)
            {
                Logging.WriteError("Save(): " + e);
            }
        }

        void Load()
        {
            try
            {
                ProfileComboBox.Text = GrinderSetting.CurrentSetting.ProfileName;
                BlackListWhereIAmDeadCheckbox.IsChecked = GrinderSetting.CurrentSetting.BlackListWhereIAmDead;
                ReturnLastPosCheckBox.IsChecked = GrinderSetting.CurrentSetting.RetLastPos;
                PetBattle.IsChecked = GrinderSetting.CurrentSetting.PetBattle;
            }
            catch (Exception e)
            {
                Logging.WriteError("Load(): " + e);
            }
        }
        void RefreshProfileList()
        {
            try
            {
                var profileName = ProfileComboBox.Text;
                ProfileComboBox.Items.Clear();

                foreach (var f in Others.GetFilesDirectoryAndSubDirectory(Application.StartupPath + "\\Profiles\\Grinder\\", "*.xml"))
                {
                        ProfileComboBox.Items.Add(f);
                }
                ProfileComboBox.Text = profileName;
            }
            catch (Exception ex)
            {
                Logging.WriteError("RefreshProfileList: " + ex);
            }
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            Save();
        }

        private void UserControlKeyUp(object sender, KeyEventArgs e)
        {
            Save();
        }

        private void ComboBoxMouseEnter(object sender, MouseEventArgs e)
        {
            RefreshProfileList();
        }

        private void ProfileCreatorButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var f = new ProfileMakerWindow { Topmost = true };
            f.Show();
        }

        private void DownloadContentsOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DownloadContents.ShowWindowNewThread("WRobot > Profiles > Grinder");
        }
    }
}

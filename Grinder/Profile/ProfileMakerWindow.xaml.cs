using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using robotManager;
using robotManager.Helpful;
using wManager;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Application = System.Windows.Forms.Application;

namespace Grinder.Profile
{
    /// <summary>
    /// Logique d'interaction pour ProfileMakerWindow.xaml
    /// </summary>
    public partial class ProfileMakerWindow : Window
    {
        public ProfileMakerWindow()
        {
            InitializeComponent();
            Translation();
            AddNewZone();
            _lastSerialized = _profile.Serialize();
        }

        void Translation()
        {
            ZonesLabel.Translate();
            AddZoneButton.Translate();
            RemoveZoneButton.Translate();
            ZoneNameLabel.Translate();
            CharacterLevelLabel.Translate();
            ToLabel.Translate();
            To2Label.Translate();
            AttackLevelLabel.Translate();

            OthersTextBlock.Translate();
            NpcTextBlock.Translate();
            BlacklistTextBlock.Translate();
            PathTextBlock.Translate();
            OpenTextBlock.Translate();
            SaveTextblock.Translate();
            CombineWithTextblock.Translate();
            NoLoopCheckBox.Translate();
            HotspotsCheckBox.Translate();

            FactionTextBlock.Translate();
            FactionInfoTextBlock.Translate();
            FactionInfo2TextBlock.Translate();
            AddFactionButton.Translate();
        }

        int _selectedIndex = -1;
        GrinderProfile _profile = new GrinderProfile();
        private string _lastSerialized = (new GrinderProfile()).Serialize();
        private string _defaultTitle;
        public string LastPath
        {
            get { return _lastPath; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _lastPath = Application.StartupPath + @"\Profiles\Grinder\";

                else
                {
                    if (string.IsNullOrWhiteSpace(_defaultTitle))
                        _defaultTitle = Title;
                    _lastPath = value;
                    var fileName = Path.GetFileName(value);
                    if (!string.IsNullOrWhiteSpace(fileName))
                        Title = _defaultTitle + " - " + fileName;
                    else
                        Title = _defaultTitle;
                }
            }
        }
        private string _lastPath = Application.StartupPath + @"\Profiles\Grinder\";

        private void MenuItemLoadClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var cancel = new System.ComponentModel.CancelEventArgs();
                DoYouWantSaveChange(cancel);

                if (cancel.Cancel)
                    return;

                string file =
                    Others.DialogBoxOpenFile(Application.StartupPath + @"\Profiles\Grinder\",
                                             "Profile files (*.xml)|*.xml|All files (*.*)|*.*");

                if (File.Exists(file))
                {
                    LastPath = file;
                    _profile = XmlSerializer.Deserialize<GrinderProfile>(file) ?? new GrinderProfile();
                    _lastSerialized = _profile.Serialize();
                    if (_profile.GrinderZones == null)
                        _profile.GrinderZones = new List<GrinderZone>();
                    if (_profile.GrinderZones.Count <= 0)
                        AddNewZone();

                    _selectedIndex = -1;
                    LoadAndSelectSubProfile(0);
                    RefreshSubZonesList();
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("ProfileMakerWindow > MenuItemLoadClick(object sender, EventArgs ex): " + ex);
            }
        }

        private void LoadAndSelectSubProfile(int index, bool saveBeforeLoad = false)
        {
            if (index != _selectedIndex && index <= _profile.GrinderZones.Count - 1 && index >= 0)
            {
                if (saveBeforeLoad)
                    SaveSubProfile();

                _selectedIndex = index;

                ZoneNameTextBox.Text = _profile.GrinderZones[_selectedIndex].Name;
                CharMinLvlTextBox.Text = _profile.GrinderZones[_selectedIndex].MinLevel.ToString(CultureInfo.InvariantCulture);
                CharMaxLvlTextBox.Text = _profile.GrinderZones[_selectedIndex].MaxLevel.ToString(CultureInfo.InvariantCulture);
                TargetMinLvlTextBox.Text = _profile.GrinderZones[_selectedIndex].MinTargetLevel.ToString(CultureInfo.InvariantCulture);
                TargetMaxLvlTextBox.Text = _profile.GrinderZones[_selectedIndex].MaxTargetLevel.ToString(CultureInfo.InvariantCulture);
                AddTargetControl.TargetEntries = _profile.GrinderZones[_selectedIndex].TargetEntry;
                NoLoopCheckBox.IsChecked = _profile.GrinderZones[_selectedIndex].NotLoop;
                HotspotsCheckBox.IsChecked = _profile.GrinderZones[_selectedIndex].Hotspots;
                NpcControl.NpcList = _profile.GrinderZones[_selectedIndex].Npc;
                ProfileMakerControl.Path = _profile.GrinderZones[_selectedIndex].Vectors3;
                BlacklistControl.Blacklist = _profile.GrinderZones[_selectedIndex].BlackListRadius.Select(b => new BlackListSerializable.Blackspot { X = b.Position.X, Y = b.Position.Y, Z = b.Position.Z, Radius = b.Radius, Continent = b.Continent }).ToList();
                FactionTextBox.Text = string.Empty;
                foreach (var f in _profile.GrinderZones[_selectedIndex].TargetFactions)
                {
                    FactionTextBox.Text += f + Environment.NewLine;
                }
            }
        }

        private void SaveSubProfile()
        {
            if (_selectedIndex <= _profile.GrinderZones.Count - 1 && _selectedIndex >= 0)
            {
                _profile.GrinderZones[_selectedIndex].Name = ZoneNameTextBox.Text;
                uint level;
                if (Others.ParseUInt(CharMinLvlTextBox.Text, out level))
                    _profile.GrinderZones[_selectedIndex].MinLevel = level;
                if (Others.ParseUInt(CharMaxLvlTextBox.Text, out level))
                    _profile.GrinderZones[_selectedIndex].MaxLevel = level;
                if (Others.ParseUInt(TargetMinLvlTextBox.Text, out level))
                    _profile.GrinderZones[_selectedIndex].MinTargetLevel = level;
                if (Others.ParseUInt(TargetMaxLvlTextBox.Text, out level))
                    _profile.GrinderZones[_selectedIndex].MaxTargetLevel = level;
                _profile.GrinderZones[_selectedIndex].TargetEntry = AddTargetControl.TargetEntries;
                _profile.GrinderZones[_selectedIndex].NotLoop = NoLoopCheckBox.IsChecked == true;
                _profile.GrinderZones[_selectedIndex].Hotspots = HotspotsCheckBox.IsChecked == true;
                _profile.GrinderZones[_selectedIndex].Npc = NpcControl.NpcList;
                _profile.GrinderZones[_selectedIndex].Vectors3 = ProfileMakerControl.Path;
                _profile.GrinderZones[_selectedIndex].BlackListRadius = BlacklistControl.Blacklist.Select(b => new GrinderBlackListRadius { Position = new Vector3(b.X, b.Y, b.Z), Radius = b.Radius, Continent = b.Continent }).ToList();
                var factionsString = Others.TextToArrayByLine(FactionTextBox.Text);
                uint f;
                foreach (var fs in factionsString)
                {
                    if (Others.ParseUInt(fs, out f) && !_profile.GrinderZones[_selectedIndex].TargetFactions.Contains(f))
                        _profile.GrinderZones[_selectedIndex].TargetFactions.Add(f);
                }
            }
        }

        private void AddNewZone(bool saveCurrentZone = false)
        {
            _profile.GrinderZones.Add(new GrinderZone { Name = Usefuls.MapZoneName });
            LoadAndSelectSubProfile(_profile.GrinderZones.Count - 1, saveCurrentZone);
            RefreshSubZonesList();
        }

        void RefreshSubZonesList()
        {
            ZonesComboBox.Items.Clear();
            foreach (var grinderZone in _profile.GrinderZones)
            {
                ZonesComboBox.Items.Add(grinderZone.Name + " - Lvl: " + grinderZone.MinLevel + " > " + grinderZone.MaxLevel);
            }
        }

        bool ProfileHasChanged()
        {
            try
            {
                UpdateProfile();
                return _lastSerialized != _profile.Serialize();
            }
            catch (Exception)
            {
            }
            return false;
        }

        void UpdateProfile()
        {
            SaveSubProfile();
        }

        private void MenuItemSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string file = Others.DialogBoxSaveFile(LastPath, "Profile files (*.xml)|*.xml|All files (*.*)|*.*");

                if (!string.IsNullOrWhiteSpace(file))
                {
                    UpdateProfile();
                    XmlSerializer.Serialize(file, _profile);
                    _lastSerialized = _profile.Serialize();
                    LastPath = file;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("ProfileMakerWindow > MenuItemSaveClick(object sender, EventArgs e): " + ex);
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DoYouWantSaveChange(e);
        }

        void DoYouWantSaveChange(System.ComponentModel.CancelEventArgs e)
        {
            if (ProfileHasChanged())
            {
                var r =
                    MessageBox.Show(
                        Translate.Get("Do you want save changes?"), "Exit", MessageBoxButton.YesNoCancel);
                if (r == MessageBoxResult.No)
                {
                    return;
                }
                else if (r == MessageBoxResult.Yes)
                {
                    MenuItemSaveClick(null, null);
                    if (!ProfileHasChanged())
                        return;
                }
                e.Cancel = true;
            }
        }

        private void RemoveZoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (_profile.GrinderZones.Count <= 1)
            {
                MessageBox.Show(Translate.Get("You need to keep one zone."));
            }

            if (_selectedIndex <= _profile.GrinderZones.Count - 1 && _selectedIndex >= 0)
            {
                _profile.GrinderZones.RemoveAt(_selectedIndex);
                LoadAndSelectSubProfile(0);
                RefreshSubZonesList();
            }
        }

        private void AddZoneButtonClick(object sender, RoutedEventArgs e)
        {
            AddNewZone(true);
        }

        private void ZonesComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadAndSelectSubProfile(ZonesComboBox.SelectedIndex, true); 
        }

        private void MenuItemCombineWithClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string file =
                    Others.DialogBoxOpenFile(Application.StartupPath + @"\Profiles\Grinder\",
                                             "Profile files (*.xml)|*.xml|All files (*.*)|*.*");

                if (File.Exists(file))
                {
                    var profile = XmlSerializer.Deserialize<GrinderProfile>(file) ?? new GrinderProfile();

                    if (profile.GrinderZones == null || profile.GrinderZones.Count <= 0)
                    {
                        MessageBox.Show(Translate.Get("This profile is empty or doesn't contain any zones."));
                        return;
                    }

                    _profile.GrinderZones.AddRange(profile.GrinderZones);

                    MessageBox.Show(profile.GrinderZones.Count + " " + Translate.Get("zones added to your profile."));

                    RefreshSubZonesList();
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("ProfileMakerWindow > MenuItemCombineWithClick(object sender, EventArgs ex): " + ex);
            }
        }

        private void AddFactionButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ObjectManager.Target)
                {
                    if (!string.IsNullOrWhiteSpace(FactionTextBox.Text) && !FactionTextBox.Text.EndsWith(Environment.NewLine))
                        FactionTextBox.Text += Environment.NewLine;
                    FactionTextBox.Text += ObjectManager.Target.Faction;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteError("ProfileMakerWindow >  AddFactionButtonClick(object sender, RoutedEventArgs e): " + ex);
            }
        }
    }
}

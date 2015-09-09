using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Linq;
using System.IO;

namespace FtpWpf
{
    public partial class ProfileManagerWindow : Window
    {
        public ProfileManagerWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            var profileManager = ProfileManager.Instance;
            lvProfiles.ItemsSource = profileManager.ProfilesCollection;
        }

        private ProfileManager.Profile GetSelectedProfile()
        {
            var index = lvProfiles.SelectedIndex;
            if (index < 0)
                return null;

            return ProfileManager.Instance.ProfilesCollection.ElementAtOrDefault(index);
        }

        private void SetUIItemSelected()
        {
            btnRemove.IsEnabled = true;
            btnSelect.IsEnabled = true;
            btnUpdate.IsEnabled = true;
        }

        private void SetUIItemNotSelected()
        {
            btnRemove.IsEnabled = false;
            btnSelect.IsEnabled = false;
            btnUpdate.IsEnabled = false;
        }

        private void SetUIDisplayProfile()
        {
            var selectedProfile = GetSelectedProfile();

            tbHost.Text = selectedProfile.Host;
            tbPort.Text = selectedProfile.GetUri().Port.ToString();
            tbUsername.Text = selectedProfile.Username ?? "";
            tbPassword.Text = selectedProfile.Password ?? "";
        }

        private void SetUIDisplayProfileClear()
        {
            tbHost.Clear();
            tbPort.Clear();
            tbUsername.Clear();
            tbPassword.Clear();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            ProfileManager.Instance.AddProfile(new ProfileManager.Profile
            {
                Host = tbHost.Text,
                Port = (tbPort.Text.Length > 0) ?  Convert.ToInt32(tbPort.Text) : -1,
                Username = tbUsername.Text,
                Password = tbPassword.Text
            });
        }

        private void lvProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvProfiles.SelectedIndex >= 0)
            {
                SetUIItemSelected();
                SetUIDisplayProfile();
            }
            else
            {
                SetUIItemNotSelected();
                SetUIDisplayProfileClear();
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = GetSelectedProfile();
            if (selectedProfile == null)
                return;

            ProfileManager.Instance.RemoveProfile(selectedProfile);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = GetSelectedProfile();
            if (selectedProfile == null)
                return;

            var con = new FtpConnection(selectedProfile);
            var list = con.GetDirectoryList();

            MainWindow mw = new MainWindow(list);
            mw.Show();
            Close();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = GetSelectedProfile();
            if (selectedProfile == null)
                return;

            selectedProfile.Host = tbHost.Text;
            selectedProfile.Port = (tbPort.Text.Length > 0) ? Convert.ToInt32(tbPort.Text) : -1;
            selectedProfile.Username = tbUsername.Text;
            selectedProfile.Password = tbPassword.Text;
            ProfileManager.Instance.UpdateProfile(selectedProfile);
        }
    }
}

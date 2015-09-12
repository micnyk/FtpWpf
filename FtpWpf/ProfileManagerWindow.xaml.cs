using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            lvProfiles.ItemsSource = ProfileManager.Instance.ProfilesCollection;
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
            tbPort.Text = (selectedProfile.Port > 0) ? selectedProfile.Port.ToString() : "21";
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
                Port = (tbPort.Text.Length > 0) ? Convert.ToInt32(tbPort.Text) : -1,
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

            var mw = new MainWindow(selectedProfile);
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
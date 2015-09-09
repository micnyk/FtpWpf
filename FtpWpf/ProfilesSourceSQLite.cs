using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace FtpWpf
{
    class ProfilesSourceSQLite : IProfilesSource
    {
        private SQLiteConnection dbConnection;
        private DataContext dbContext;

        private ObservableTable<ProfileManager.Profile> Profiles;
        public IEnumerable<ProfileManager.Profile> ProfilesCollection => Profiles;

        public ProfilesSourceSQLite()
        {
            string dbPath = Directory.GetCurrentDirectory() + @"\FtpWpf.sqlite";
            bool exists = false;

            if (File.Exists(dbPath))
                exists = true;
            else
                SQLiteConnection.CreateFile(dbPath);

            dbConnection = new SQLiteConnection(string.Format(@"Data Source={0};Version=3", dbPath));
            dbConnection.Open();
            
            if (!exists)
                CreateTable();

            dbContext = new DataContext(dbConnection);
            Profiles = new ObservableTable<ProfileManager.Profile>(ref dbContext);
        }

        private void CreateTable()
        {
            var cmd = new SQLiteCommand(@"create table profiles (id int, host text, port int, username text, password text, primary key(id))", dbConnection);
            cmd.ExecuteNonQuery();
        }

        public bool AddProfile(ProfileManager.Profile profile)
        {
            try
            {
                Profiles.InsertOnSubmit(profile);
                dbContext.SubmitChanges();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

        public bool GetProfiles(out List<ProfileManager.Profile> profiles)
        {
            throw new NotImplementedException();
        }

        public ProfileManager.Profile GetProfile(int? id)
        {
            var queryProfiles = (from p in Profiles where p.Id == id select p).ToArray();

            if (!queryProfiles.Any())
                return null;

            return queryProfiles[0];
        }

        public bool RemoveProfile(ProfileManager.Profile profile)
        {
            try
            {
                Profiles.DeleteOnSubmit(profile);
                dbContext.SubmitChanges();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

        public bool UpdateProfile(ProfileManager.Profile profile)
        {
            dbContext.SubmitChanges();
            return true;
        }
    }
}

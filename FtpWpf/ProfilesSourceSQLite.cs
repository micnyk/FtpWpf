using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Windows;

namespace FtpWpf
{
    internal class ProfilesSourceSQLite : IProfilesSource
    {
        private readonly SQLiteConnection _dbConnection;
        private readonly DataContext _dbContext;

        private readonly ObservableTable<ProfileManager.Profile> _profiles;
        public IEnumerable<ProfileManager.Profile> ProfilesCollection => _profiles;

        public ProfilesSourceSQLite()
        {
            var dbPath = Directory.GetCurrentDirectory() + @"\FtpWpf.sqlite";
            var exists = false;

            if (File.Exists(dbPath))
                exists = true;
            else
                SQLiteConnection.CreateFile(dbPath);

            _dbConnection = new SQLiteConnection($@"Data Source={dbPath};Version=3");
            _dbConnection.Open();

            if (!exists)
                CreateTable();

            _dbContext = new DataContext(_dbConnection);
            _profiles = new ObservableTable<ProfileManager.Profile>(ref _dbContext);
        }


        public bool AddProfile(ProfileManager.Profile profile)
        {
            try
            {
                _profiles.InsertOnSubmit(profile);
                _dbContext.SubmitChanges();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            return true;
        }

        public bool RemoveProfile(ProfileManager.Profile profile)
        {
            try
            {
                _profiles.DeleteOnSubmit(profile);
                _dbContext.SubmitChanges();
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
            _dbContext.SubmitChanges();
            return true;
        }

        private void CreateTable()
        {
            var cmd =
                new SQLiteCommand(
                    @"create table profiles (id int, host text, port int, username text, password text, primary key(id))",
                    _dbConnection);
            cmd.ExecuteNonQuery();
        }
    }
}
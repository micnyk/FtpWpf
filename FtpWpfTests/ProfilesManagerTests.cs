using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FtpWpf;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FtpWpfTests
{
    [TestClass]
    public class ProfilesManagerTests
    {
        private class TestProfilesSource : IProfilesSource
        {
            private List<ProfileManager.Profile> profiles = new List<ProfileManager.Profile>
            {
              new ProfileManager.Profile { Id = 3212, Host = "test.pl" },
              new ProfileManager.Profile { Id = 1235, Host = "innnastrona.com", Username = "testowy", Password = "haslo" }
            };

            public bool AddProfile(ProfileManager.Profile profile)
            {
                profiles.Add(profile);
                return true;
            }

            public ProfileManager.Profile GetProfile(int? id)
            {
                return profiles.FirstOrDefault(x => x.Id == id);
            }

            public List<ProfileManager.Profile> GetProfiles()
            {
                return profiles;
            }

            public bool RemoveProfile(ProfileManager.Profile profile)
            {
                return profiles.RemoveAll(x => x.Id == profile.Id) > 0;
            }

            public bool UpdateProfile(ProfileManager.Profile profile)
            {
                var prof = profiles.FirstOrDefault(x => x.Id == profile.Id);

                if (prof == null)
                    return false;

                prof = profile;
                return true;
            }
        }

        private ProfileManager profileManager;

        [TestInitialize]
        public void TestInitialize()
        {
            profileManager = new ProfileManager(new TestProfilesSource());
        }

        [TestMethod]
        public void TestProfileManager()
        {
            List<ProfileManager.Profile> list = profileManager.GetProfiles();

            foreach(var profile in list)
            {
                Debug.WriteLine(profile.ToString());
            }
        }
    }
}

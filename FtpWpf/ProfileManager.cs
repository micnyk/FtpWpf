using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using System.Net;
using System.Runtime.CompilerServices;
using FtpWpf.Annotations;

namespace FtpWpf
{

    public class ProfileManager
    {
        private IProfilesSource profilesSource;
        public IEnumerable<Profile> ProfilesCollection => profilesSource.ProfilesCollection;

        private static ProfileManager instance;
        public static ProfileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ProfileManager();

                return instance;
            }
        }

        public ProfileManager(IProfilesSource profilesSource = null)
        {
            if (profilesSource != null)
                this.profilesSource = profilesSource;
            else
                this.profilesSource = new ProfilesSourceSQLite();
        }

        public bool AddProfile(Profile profile)
        {
            Random random = new Random();
            profile.Id = random.Next(1001, 9999);

            return profilesSource.AddProfile(profile);
        }

        public bool RemoveProfile(Profile profile)
        {
            return profilesSource.RemoveProfile(profile);
        }

        public bool UpdateProfile(Profile profile)
        {
            return profilesSource.UpdateProfile(profile);
        }

        [Table(Name = "profiles")]
        public class Profile : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private int? _id;
            private string _host;
            private int? _port;
            private string _username;
            private string _password;

            [Key]
            [Column(Name = "id", IsPrimaryKey = true, CanBeNull = false, IsDbGenerated = false)]
            public int? Id
            {
                get { return _id; }
                set
                {
                    if (value == _id) return;
                    _id = value;
                    OnPropertyChanged();
                }
            }

            [Column(Name = "host")]
            public string Host
            {
                get { return _host; }
                set
                {
                    if (value == _host) return;
                    _host = value;
                    OnPropertyChanged();
                }
            }

            [Column(Name = "port")]
            public int? Port
            {
                get { return _port; }
                set
                {
                    if (value == _port) return;
                    _port = value;
                    OnPropertyChanged();
                }
            }

            [Column(Name = "username")]
            public string Username
            {
                get { return _username; }
                set
                {
                    if (value == _username) return;
                    _username = value;
                    OnPropertyChanged();
                }
            }

            [Column(Name = "password")]
            public string Password
            {
                get { return _password; }
                set
                {
                    if (value == _password) return;
                    _password = value;
                    OnPropertyChanged();
                }
            }

            public Uri GetUri()
            {
                UriBuilder builder = new UriBuilder
                {
                    Scheme = "FTP",
                    Host = Host,
                    Port = Convert.ToInt32(Port ?? -1)
                };

                return builder.Uri;
            }

            public NetworkCredential GetNetworkCredentials()
            {
                if (Username == null || Password == null)
                    return null;

                return new NetworkCredential(Username, Password);
            }

            public override string ToString()
            {
                string ret = Id + ": ";

                var nc = GetNetworkCredentials();
                if (nc != null)
                    ret += nc.UserName + "@";


                ret += GetUri().ToString();
                return ret;
            }
        }
    }
}

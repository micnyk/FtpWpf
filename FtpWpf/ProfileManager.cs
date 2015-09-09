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
        private static ProfileManager _instance;
        public static ProfileManager Instance => _instance ?? (_instance = new ProfileManager());

        private readonly IProfilesSource _profilesSource;
        public IEnumerable<Profile> ProfilesCollection => _profilesSource.ProfilesCollection;
        
        public ProfileManager(IProfilesSource profilesSource = null)
        {
            _profilesSource = profilesSource ?? new ProfilesSourceSQLite();
        }

        public bool AddProfile(Profile profile)
        {
            var random = new Random();
            //TODO: check if id already exists
            profile.Id = random.Next(1001, 9999);

            return _profilesSource.AddProfile(profile);
        }

        public bool RemoveProfile(Profile profile)
        {
            return _profilesSource.RemoveProfile(profile);
        }

        public bool UpdateProfile(Profile profile)
        {
            return _profilesSource.UpdateProfile(profile);
        }

        [Table(Name = "profiles")]
        public class Profile : INotifyPropertyChanged
        {
            private string _host;

            private int? _id;
            private string _password;
            private int? _port;
            private string _username;

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

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public Uri GetUri()
            {
                var builder = new UriBuilder
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
                var ret = Id + ": ";

                var nc = GetNetworkCredentials();
                if (nc != null)
                    ret += nc.UserName + "@";


                ret += GetUri().ToString();
                return ret;
            }
        }
    }
}
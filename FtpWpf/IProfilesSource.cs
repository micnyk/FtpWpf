using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace FtpWpf
{
    public interface IProfilesSource
    {
        IEnumerable<ProfileManager.Profile> ProfilesCollection { get; } 

        bool AddProfile(ProfileManager.Profile profile);
        bool UpdateProfile(ProfileManager.Profile profile);
        bool RemoveProfile(ProfileManager.Profile profile);
    }
}

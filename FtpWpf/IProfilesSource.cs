using System.Collections.Generic;

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
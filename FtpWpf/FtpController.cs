using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpWpf
{
    class FtpController
    {
        private FtpController _instance;
        public FtpController Instance => _instance ?? (_instance = new FtpController());

        public ProfileManager.Profile Profile { get; set; }

        private FtpController()
        {
            //TODO: bawic sie z watkami
        }
    }
}

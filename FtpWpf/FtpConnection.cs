using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FtpWpf.FileSystemModel;

namespace FtpWpf
{
    public class FtpConnection
    {
        private Uri uri;
        private NetworkCredential networkCredentials;

        public FtpConnection(ProfileManager.Profile profile)
        {
            uri = profile.GetUri();

            var credentials = profile.GetNetworkCredentials();
            if (credentials != null)
                networkCredentials = credentials;
        }

        public List<FileSystemModel.FileSystemModelItem> GetDirectoryList(string relativePath = "/")
        {
            var requestUri = new Uri(uri, relativePath);
            relativePath = uri.MakeRelativeUri(requestUri).ToString();

            var ftpRequest = (FtpWebRequest) WebRequest.Create(requestUri);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            ftpRequest.Proxy = null;
            ftpRequest.KeepAlive = true;

            if (networkCredentials != null)
                ftpRequest.Credentials = networkCredentials;

            List<FileSystemModelItem> items;

            try
            {
                var request = Stopwatch.StartNew();

                using (var response = (FtpWebResponse) ftpRequest.GetResponse())
                {
                    request.Stop();
                    Debug.WriteLine("request " + relativePath + ": " + request.ElapsedMilliseconds);

                    var provider = Stopwatch.StartNew();

                    items = ItemsProvider.GetItems(response.GetResponseStream(), relativePath);

                    provider.Stop();
                    Debug.WriteLine("provider " + relativePath + ": " + provider.ElapsedMilliseconds);
                }
            }
            catch (Exception)
            {
                return new List<FileSystemModelItem>();
            }

            foreach (var item in items)
            {
                if (item is FileSystemModel.Directory)
                    ((FileSystemModel.Directory) item).Items = GetDirectoryList(relativePath + "/" + item.Name);
            }

            return items;
        }
    }
}

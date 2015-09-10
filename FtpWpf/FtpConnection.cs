using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using FtpWpf.FileSystemModel;

namespace FtpWpf
{
    public class FtpConnection
    {
        private readonly NetworkCredential _networkCredentials;
        private readonly Uri _uri;

        public FtpConnection(ProfileManager.Profile profile)
        {
            _uri = profile.GetUri();

            var credentials = profile.GetNetworkCredentials();
            if (credentials != null)
                _networkCredentials = credentials;
        }

        public List<Item> GetDirectoryList(string relativePath = "/")
        {
            var requestUri = new Uri(_uri, relativePath);
            relativePath = _uri.MakeRelativeUri(requestUri).ToString();

            var ftpRequest = (FtpWebRequest) WebRequest.Create(requestUri);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            ftpRequest.Proxy = null;
            ftpRequest.KeepAlive = true;

            if (_networkCredentials != null)
                ftpRequest.Credentials = _networkCredentials;

            List<Item> items;

            try
            {
                using (var response = (FtpWebResponse) ftpRequest.GetResponse())
                {
                    items = ItemsProvider.GetItems(response.GetResponseStream(), relativePath);
                }
            }
            catch (Exception)
            {
                return new List<Item>();
            }

            foreach (var item in items)
            {
               // var directory = item as Directory;
               // if (directory != null)
               //     directory.Items = GetDirectoryList(relativePath + "/" + directory.Name);
            }

            return items;
        }
    }
}
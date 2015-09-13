using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using FtpWpf.FileSystemModel;
using File = FtpWpf.FileSystemModel.File;

namespace FtpWpf
{
    public class FtpConnection
    {
        private readonly NetworkCredential _networkCredentials;
        private readonly Uri _uri;

        public event EventHandler<FileProgressEventArgs> FileProgress;
        public string RelativePath { get; private set; }

        public FtpConnection(ProfileManager.Profile profile)
        {
            _uri = profile.GetUri();

            var credentials = profile.GetNetworkCredentials();
            if (credentials != null)
                _networkCredentials = credentials;
        }

        private FtpWebRequest CreateRequest(Uri requestUri)
        {
            var request = (FtpWebRequest) WebRequest.Create(requestUri);
            request.Proxy = null;
            request.KeepAlive = true;

            if (_networkCredentials != null)
                request.Credentials = _networkCredentials;

            return request;
        }

        private Uri CreateRequestUri(string path)
        {
            var requestUri = new Uri(_uri, path);
            RelativePath = _uri.MakeRelativeUri(requestUri).ToString();

            return requestUri;
        }

        public Stream ListDirectory(string path)
        {
            var requestUri = CreateRequestUri(path);
            var request = CreateRequest(requestUri);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            Stream stream;

            try
            {
                var response = request.GetResponse();
                stream = response.GetResponseStream();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, $"FTP protocol error on path='{path}': {e.Message}");
                stream = new MemoryStream();
            }

            return stream;
        }

        public bool DownloadFile(File file, Stream output)
        {
            // TODO: uporzadkowac w ItemsProvider path zeby bylo jednakowo
            var path = file.Path;

            //var fileStream = new BinaryWriter(System.IO.File.OpenWrite(@"C:\Users\Michał\Desktop\file"));

            var len = path.Length;
            var lastChar = path[len - 1];
            if(len > 1 && (lastChar != '/' || lastChar != '\\'))
                path += "/";
            path += file.Name;

            int fileBufferSize = 2048;

            var requestUri = CreateRequestUri(path);
            var request = CreateRequest(requestUri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;

            using (var response = request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                    return false;

                var reader = new BinaryReader(responseStream);
                var writer = new BinaryWriter(output);

                long offset = 0;

                while (offset < response.ContentLength)
                {
                    var buffer = reader.ReadBytes(fileBufferSize);
                    writer.Write(buffer, 0, buffer.Length);
                    offset += buffer.LongLength;

                    FileProgress?.Invoke(this,
                        new FileProgressEventArgs {TotalSize = response.ContentLength, Actual = offset});
                }

                writer.Close();
            }

            return true;
        }
    }

    public class FileProgressEventArgs : EventArgs
    {
        public long TotalSize { get; set; }
        public long Actual { get; set; }
        public int Progress => (int) (((double) Actual/(double) TotalSize)*100);
    }
}
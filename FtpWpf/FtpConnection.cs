using System;
using System.IO;
using System.Net;
using FtpWpf.FileSystemModel;
using Directory = FtpWpf.FileSystemModel.Directory;
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
            request.UseBinary = true;

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
                throw;
            }

            return stream;
        }

        public bool DownloadFile(File file, Stream output)
        {
            const int fileBufferSize = 2048;

            var path = file.Path + file.Name;
            var requestUri = CreateRequestUri(path);
            var request = CreateRequest(requestUri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            try
            {
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
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, $"FTP protocol downloading file='{file.Name}': {e.Message}");
                throw;
            }

            return true;
        }

        public bool Rename(Item item, string name)
        {
            var path = item.Path + item.Name;
            var requestUri = CreateRequestUri(path);
            var request = CreateRequest(requestUri);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.RenameTo = name;

            try
            {
                request.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, "FTP Error renaming item: " + e.Message, this);
                return false;
            }

            item.Name = name;
            return true;
        }

        public bool Remove(Item item)
        {
            var path = item.Path + item.Name;
            var requestUri = CreateRequestUri(path);
            var request = CreateRequest(requestUri);

            request.Method = (item is Directory)
                ? WebRequestMethods.Ftp.RemoveDirectory
                : WebRequestMethods.Ftp.DeleteFile;

            try
            {
                request.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, "FTP Error deleting item: " + e.Message, this);
                return false;
            }

            return true;
        }

        public bool New(Item item)
        {
            if (item == null)
                return false;

            if (item is Directory)
                return CreateDirectory((Directory) item);

            return CreateFile(item as File);
        }

        private bool CreateDirectory(Directory directory)
        {
            var requestUri = CreateRequestUri(directory.Path + directory.Name);
            var request = CreateRequest(requestUri);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            try
            {
                request.GetResponse();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, "FTP Error create directory: " + e.Message, this);
                return false;
            }

            return true;
        }

        private bool CreateFile(File file)
        {
            var requestUri = CreateRequestUri(file.Path + file.Name);
            var request = CreateRequest(requestUri);

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.ContentLength = 0;

            try
            {
                var stream = request.GetRequestStream();
                stream.Close();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, "FTP Error create file: " + e.Message, this);
                return false;
            }

            return true;
        }

        public bool UploadFile(File item, string localPath)
        {
            const int bufferSize = 4096;

            var fileInfo = new FileInfo(localPath);

            if (!fileInfo.Exists)
                return false;

            var requestUri = CreateRequestUri(item.Path + item.Name);
            var request = CreateRequest(requestUri);

            var fileTotalBytes = fileInfo.Length;

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.ContentLength = fileTotalBytes;

            try
            {
                var fileReader = new BinaryReader(fileInfo.OpenRead());
                var requestWriter= new BinaryWriter(request.GetRequestStream());

                long offset = 0;
                var buffer = new byte[bufferSize];

                while (offset < fileTotalBytes)
                {
                    var read = fileReader.Read(buffer, 0, bufferSize);
                    requestWriter.Write(buffer, 0, read);

                    offset += read;
                    FileProgress?.Invoke(this, new FileProgressEventArgs {Actual = offset, TotalSize = fileTotalBytes});
                }

                fileReader.Close();
                requestWriter.Close();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, "FTP Error upload file: " + e.Message, this);
                return false;
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FtpWpf.FileSystemModel;
using Directory = FtpWpf.FileSystemModel.Directory;
using File = FtpWpf.FileSystemModel.File;

namespace FtpWpf
{
    public class FtpController
    {
        private static FtpController _instance;
        public static FtpController Instance => _instance ?? (_instance = new FtpController());

        public ProfileManager.Profile Profile { get; set; }
        public ObservableCollection<Item> Items { get; } 
        public Dispatcher Dispatcher { get; set; }

        private FtpController()
        {
            Items = new ObservableCollection<Item>();
        }

        public bool DownloadFile(File file)
        {
            if (Profile == null)
                return false;

            FtpConnection connection = new FtpConnection(Profile);
            connection.FileProgress += delegate(object sender, FileProgressEventArgs args)
            {
                Logger.Log(Logger.Type.Info, "Progress: " + args.Progress, sender);
            };

            connection.DownloadFile(file, new MemoryStream());

            return true;
        }

        public bool ListDirectory(string path, ObservableCollection<Item> collection = null)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            if (collection == null)
                collection = Items;

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                using (var stream = connection.ListDirectory(path))
                {
                    SafeAppendItems(collection, path, stream);
                }

                foreach (var item in collection)
                {
                    if (item is Directory)
                    {
                        var relPath = connection.RelativePath + "/" + item.Name;
                        ListDirectory(relPath, item.Items);
                    }
                }
            });

            return true;
        }

        private void SafeAppendItems(ObservableCollection<Item> collection, string path, Stream stream)
        {
            Dispatcher.Invoke(() => { ItemsProvider.AppendItems(stream, path, collection); });
        }
    }
}

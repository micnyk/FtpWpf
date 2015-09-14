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
        public event EventHandler<FileProgressEventArgs> FileProgress;
        public event EventHandler<ActionStartedEventArgs> ActionStarted; // TODO: ActionFailed and ActionSucceed events

        private FtpController()
        {
            Items = new ObservableCollection<Item>();
        }

        public bool DownloadFile(File file)
        {
            if (Profile == null)
                return false;

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                connection.FileProgress += delegate(object sender, FileProgressEventArgs args)
                {
                    FileProgress?.Invoke(sender, args);
                };

                ActionStarted?.Invoke(this, new ActionStartedEventArgs {Action = Action.DownloadFile, Target = file});
                connection.DownloadFile(file, new MemoryStream());
            });
       
            return true;
        }

        public bool ListDirectory(string path, ObservableCollection<Item> collection = null)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            if (collection == null)
            {
                collection = Items;
                ActionStarted?.Invoke(this,
                    new ActionStartedEventArgs
                    {
                        Action = Action.ListDirectory,
                        Target = new Directory {Name = "/", Path = "/"}
                    });
            }

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
                        ActionStarted?.Invoke(this, new ActionStartedEventArgs {Action = Action.ListDirectory, Target = item});
                        ListDirectory(relPath, item.Items);
                    }
                }
            });

            return true;
        }

        public bool Rename(Item item, string name)
        {
            if(Profile == null)
                return false;

            ActionStarted?.Invoke(this, new ActionStartedEventArgs {Action = Action.RenameFile, Target = item});

            Task.Run(() =>
            {
                FtpConnection connection;
                lock(Profile) { connection = new FtpConnection(Profile); }

                connection.Rename(item, name);
            });

            return true;
        }

        public bool Delete(Item item)
        {
            if (Profile == null)
                return false;

            ActionStarted?.Invoke(this, new ActionStartedEventArgs {Action = Action.DeleteFile, Target = item});
            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                connection.Remove(item);
            });

            return true;
        }

        public bool NewDirectory(Directory directory)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            ActionStarted?.Invoke(this, new ActionStartedEventArgs { Action = Action.UploadFile, Target = directory });
            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                connection.New(directory);

                Dispatcher.Invoke(() => { Items.Add(directory); });
            });

            return true;
        }

        private void SafeAppendItems(ObservableCollection<Item> collection, string path, Stream stream)
        {
            Dispatcher.Invoke(() => { ItemsProvider.AppendItems(stream, path, collection); });
        }

        public enum Action
        {
            ListDirectory,
            DownloadFile,
            UploadFile,
            RenameFile,
            DeleteFile
        }

        public class ActionStartedEventArgs : EventArgs
        {
            public Action Action { get; set; }
            public Item Target { get; set; }
        }
    }
}

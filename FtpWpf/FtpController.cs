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
        public event EventHandler<ActionEventArgs> ActionEvent;

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

                    if(args.Progress > 99)
                        ActionEvent?.Invoke(this, ActionEventArgs.DownloadSucceed(file));
                };

                ActionEvent?.Invoke(this, ActionEventArgs.DownloadStarted(file));
                connection.DownloadFile(file, new MemoryStream());
            });
       
            return true;
        }

        public bool ListDirectory(Directory directory = null, ObservableCollection<Item> collection = null)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            if (collection == null)
                collection = Items;

            if (directory == null)
                directory = new Directory {Name = "", Path = "/"};

            Task.Run(() =>
            {
                ActionEvent?.Invoke(this, ActionEventArgs.ListStarted(directory));

                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                var path = directory.Path + directory.Name;

                try
                {
                    using (var stream = connection.ListDirectory(path))
                    {
                        SafeAppendItems(collection, path, stream);
                        ActionEvent?.Invoke(this, ActionEventArgs.ListSucceed(directory));
                    }
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.ListFailed(directory));
                }

                foreach (var item in collection.OfType<Directory>())
                {
                    ListDirectory(item, item.Items);
                }
            });

            return true;
        }

        public bool Rename(Item item, string name)
        {
            if(Profile == null)
                return false;

           // ActionStarted?.Invoke(this, new ActionEventArgs {Action = Action.Rename, Target = item});

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

           // ActionStarted?.Invoke(this, new ActionEventArgs {Action = Action.Delete, Target = item});
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

            //ActionStarted?.Invoke(this, new ActionEventArgs { Action = Action.UploadFile, Target = directory });
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

        
        public class ActionEventArgs : EventArgs
        {
            public enum ActionType
            {
                ListDirectory,
                DownloadFile,
                UploadFile,
                Rename,
                Delete,
                New
            }

            public enum ActionStatus
            {
                Started,
                Succeed,
                Failed
            }

            public ActionStatus Status { get; set; }
            public ActionType Action { get; set; }
            public Item Target { get; set; }

            public static ActionEventArgs ListStarted(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.ListDirectory,
                    Status = ActionStatus.Started,
                    Target = item
                };
            }

            public static ActionEventArgs ListSucceed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.ListDirectory,
                    Status = ActionStatus.Succeed,
                    Target = item
                };
            }

            public static ActionEventArgs ListFailed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.ListDirectory,
                    Status = ActionStatus.Failed,
                    Target = item
                };
            }

            public static ActionEventArgs DownloadStarted(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.DownloadFile,
                    Status = ActionStatus.Started,
                    Target = item
                };
            }

            public static ActionEventArgs DownloadSucceed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.DownloadFile,
                    Status = ActionStatus.Succeed,
                    Target = item
                };
            }

            public static ActionEventArgs DownloadFailed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.DownloadFile,
                    Status = ActionStatus.Failed,
                    Target = item
                };
            }
        }
    }
}

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

        public bool DownloadFile(File file, Stream outputStream = null)
        {
            if (Profile == null)
                return false;

            outputStream = new MemoryStream();

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                connection.FileProgress += delegate(object sender, FileProgressEventArgs args)
                {
                    FileProgress?.Invoke(sender, args);
                };

                ActionEvent?.Invoke(this, ActionEventArgs.DownloadStarted(file));

                try
                {
                    connection.DownloadFile(file, outputStream);
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.DownloadFailed(file));
                }
                finally
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.DownloadSucceed(file));
                }
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

                try
                {
                    var path = directory.Path + directory.Name;

                    using (var stream = connection.ListDirectory(path))
                    {
                        SafeAppendItems(collection, path, stream);
                    }
                }
                catch (Exception)
                {
                    // TODO: obsluga bledow
                    ActionEvent?.Invoke(this, ActionEventArgs.ListFailed(directory));
                }
                finally
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.ListSucceed(directory));
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

            Task.Run(() =>
            {
                ActionEvent?.Invoke(this, ActionEventArgs.RenameStarted(item));

                FtpConnection connection;
                lock(Profile) { connection = new FtpConnection(Profile); }

                try
                {
                    if (!connection.Rename(item, name))
                        throw new Exception();
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.RenameFailed(item));
                }
                finally
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.RenameSucceed(item));
                }
            });

            return true;
        }

        public bool Delete(Item item)
        {
            if (Profile == null)
                return false;

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                ActionEvent?.Invoke(this, ActionEventArgs.DeleteStarted(item));

                try
                {
                    connection.Remove(item);
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.DeleteFailed(item));
                }
                finally
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.DeleteSucceed(item));
                }
            });

            return true;
        }

        public bool NewDirectory(Directory directory, ObservableCollection<Item> parentCollection)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                ActionEvent?.Invoke(this, ActionEventArgs.NewStarted(directory));

                try
                {
                    connection.New(directory);
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.NewFailed(directory));
                }
                finally
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.NewSucceed(directory));
                }

                Dispatcher.Invoke(() => { parentCollection.Add(directory); });
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

            public static ActionEventArgs RenameStarted(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.Rename,
                    Status = ActionStatus.Started,
                    Target = item
                };
            }

            public static ActionEventArgs RenameSucceed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.Rename,
                    Status = ActionStatus.Succeed,
                    Target = item
                };
            }

            public static ActionEventArgs RenameFailed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.Rename,
                    Status = ActionStatus.Failed,
                    Target = item
                };
            }

            public static ActionEventArgs DeleteStarted(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.Delete,
                    Status = ActionStatus.Started,
                    Target = item
                };
            }

            public static ActionEventArgs DeleteSucceed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.Delete,
                    Status = ActionStatus.Succeed,
                    Target = item
                };
            }

            public static ActionEventArgs DeleteFailed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.Delete,
                    Status = ActionStatus.Failed,
                    Target = item
                };
            }

            public static ActionEventArgs NewStarted(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.New,
                    Status = ActionStatus.Started,
                    Target = item
                };
            }

            public static ActionEventArgs NewSucceed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.New,
                    Status = ActionStatus.Succeed,
                    Target = item
                };
            }

            public static ActionEventArgs NewFailed(Item item)
            {
                return new ActionEventArgs
                {
                    Action = ActionType.New,
                    Status = ActionStatus.Failed,
                    Target = item
                };
            }
        }
    }
}

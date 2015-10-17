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

            //TODO: save to file
            outputStream = new MemoryStream();

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                connection.FileProgress += delegate (object sender, FileProgressEventArgs args)
                {
                    FileProgress?.Invoke(sender, args);
                };

                try
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.DownloadStarted(file));
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
                directory = new Directory { Name = "", Path = "/" };

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
            if (Profile == null)
                return false;

            Task.Run(() =>
            {
                ActionEvent?.Invoke(this, ActionEventArgs.RenameStarted(item));

                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

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

        public bool Delete(Item item, ObservableCollection<Item> parentCollection = null)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            if (parentCollection == null)
                parentCollection = Items;

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
                    Dispatcher.Invoke(() => { parentCollection.Remove(item); });
                    ActionEvent?.Invoke(this, ActionEventArgs.DeleteSucceed(item));
                }
            });

            return true;
        }

        public bool New(Item item, ObservableCollection<Item> parentCollection)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            if (parentCollection == null)
                parentCollection = Items;

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                ActionEvent?.Invoke(this, ActionEventArgs.NewStarted(item));

                try
                {
                    connection.New(item);
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.NewFailed(item));
                }
                finally
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.NewSucceed(item));
                }

                Dispatcher.Invoke(() => { parentCollection.Add(item); });
            });

            return true;
        }

        public bool UploadFile(File file, Directory parent, string localPath)
        {
            if (Profile == null || Dispatcher == null)
                return false;

            Task.Run(() =>
            {
                FtpConnection connection;
                lock (Profile) { connection = new FtpConnection(Profile); }

                connection.FileProgress += delegate (object sender, FileProgressEventArgs args)
                {
                    FileProgress?.Invoke(sender, args);
                };


                ActionEvent?.Invoke(this, ActionEventArgs.UploadStarted(file));

                try
                {
                    connection.UploadFile(file, localPath);
                }
                catch (Exception)
                {
                    ActionEvent?.Invoke(this, ActionEventArgs.UploadFailed(file));
                }
                finally
                {
                    if (parent != null)
                        Dispatcher.Invoke(() => { parent.Items.Add(file); });
                    else
                        Dispatcher.Invoke(() => { Items.Add(file); });

                    ActionEvent?.Invoke(this, ActionEventArgs.UploadSucceed(file));
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

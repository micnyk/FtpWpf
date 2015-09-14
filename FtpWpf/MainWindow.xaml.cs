using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FtpWpf.FileSystemModel;
using Directory = FtpWpf.FileSystemModel.Directory;
using File = FtpWpf.FileSystemModel.File;

namespace FtpWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(ProfileManager.Profile profile)
        {
            InitializeComponent();
            tbLog.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            Logger.LogEvent += AppendLogList;

            var controller = FtpController.Instance;
            controller.FileProgress += delegate(object sender, FileProgressEventArgs args)
            {
                ProgressBar.Dispatcher.Invoke(delegate { ProgressBar.Value = args.Progress; });
            };
            controller.ActionStarted += LogActionStarted;
            controller.Profile = profile;
            controller.Dispatcher = treeView.Dispatcher;
            treeView.ItemsSource = controller.Items;
            controller.ListDirectory("/");
        }

        private void LogActionStarted(object sneder, FtpController.ActionStartedEventArgs args)
        {
            tbLog.Dispatcher.Invoke(() =>
            {
                tbLog.Text += args.Action + ": '" + args.Target.Path;
                tbLog.Text += (args.Target.Name.Equals("/") ? "" : args.Target.Name) + "'\n";
                tbLog.ScrollToEnd();
            });
        }

        private void AppendLogList(object sender, LogEventArgs args)
        {
            tbLog.Dispatcher.Invoke(() => { tbLog.Text += args.Message + "\n"; tbLog.ScrollToEnd(); });
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = treeView.SelectedItem;
            if (!(selectedItem is File))
                return;

            FtpController.Instance.DownloadFile(selectedItem as File);
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Item) treeView.SelectedItem;
            if (selectedItem == null)
                return;

            FtpController.Instance.Rename(selectedItem, "hello world!");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Item)treeView.SelectedItem;
            if (selectedItem == null)
                return;

            FtpController.Instance.Delete(selectedItem);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            var dir = new Directory {Path = "/", Name = "hello!"};
            FtpController.Instance.NewDirectory(dir);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FtpWpf.FileSystemModel;
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

            Logger.LogEvent += AppendLogList;

            var controller = FtpController.Instance;
            controller.Profile = profile;
            controller.Dispatcher = treeView.Dispatcher;

            treeView.ItemsSource = controller.Items;

            controller.ListDirectory("/");
        }

        private void AppendLogList(object sender, LogEventArgs args)
        {
            tbLog.Dispatcher.Invoke(() => { tbLog.Text += args.Message + "\n"; });
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = treeView.SelectedItem;
            if (!(selectedItem is File))
                return;

            FtpController.Instance.DownloadFile(selectedItem as File);
        }
    }
}
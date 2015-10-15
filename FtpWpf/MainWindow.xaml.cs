using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FtpWpf.FileSystemModel;
using Microsoft.Win32;
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

            var ctxMenuNew = FindResource("cmNewButton") as ContextMenu;
            if(ctxMenuNew == null)
                throw new Exception("Cant find resource!");
            ctxMenuNew.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(btnNew_MenuItemClick));

            Logger.LogEvent += AppendLogList;

            var controller = FtpController.Instance;
            controller.FileProgress += delegate(object sender, FileProgressEventArgs args)
            {
                ProgressBar.Dispatcher.Invoke(delegate { ProgressBar.Value = args.Progress; });
            };

            controller.ActionEvent += LogAction;
            controller.Profile = profile;
            controller.Dispatcher = treeView.Dispatcher;
            treeView.ItemsSource = controller.Items;
            controller.ListDirectory();
        }

        private void LogAction(object sneder, ActionEventArgs args)
        {
            tbLog.Dispatcher.Invoke(() =>
            {
                tbLog.Text += args.Action + " " + args.Status + ": '" + args.Target.Path;
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
            var btnNewMenu = FindResource("cmNewButton") as ContextMenu;
            if (btnNewMenu == null)
                throw new Exception("Resource 'cmNewButton' not found!");

            btnNewMenu.PlacementTarget = btnNew;
            btnNewMenu.IsOpen = true;
        }

        private void btnNew_MenuItemClick(object sender, RoutedEventArgs e)
        {
            var source = e.Source as MenuItem;
            if (source == null)
                return;

            var selectedDirectory = treeView.SelectedItem as Directory;
            if (selectedDirectory == null)
                return;

            var file = false;

            switch (source.Name)
            {
                case "NewFile":
                    file = true;
                    break;

                case "NewDirectory":
                    file = false;
                    break;
            }

            var dialog = new FileNameDialog();
            if (dialog.ShowDialog() == true)
            {
                var name = dialog.ResponseText;

                
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Directory selectedDirectory = null;

            if (treeView.SelectedItem is Directory)
                selectedDirectory = (Directory) treeView.SelectedItem;

            var fileDialog = new OpenFileDialog {Multiselect = true};
            if (fileDialog.ShowDialog(this) != true)
                return;

            int index = 0;
            foreach (var file in fileDialog.SafeFileNames.Select(fileName => new File { Name = fileName}))
            {
                if (selectedDirectory != null)
                    file.Path = selectedDirectory.Path + selectedDirectory.Name + "/";
                else
                    file.Path = "/";

                var localPath = fileDialog.FileNames[index];
                index++;

                FtpController.Instance.UploadFile(file, selectedDirectory, localPath);
            }
        }
    }
}
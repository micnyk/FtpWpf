using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FtpWpf.FileSystemModel;

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

            var controller = FtpController.Instance;
            controller.Profile = profile;
            controller.Dispatcher = treeView.Dispatcher;

            treeView.ItemsSource = controller.Items;

            controller.ListDirectory("/");
        }
    }
}
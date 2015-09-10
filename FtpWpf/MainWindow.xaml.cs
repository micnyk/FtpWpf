using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using FtpWpf.FileSystemModel;

namespace FtpWpf
{
    public partial class MainWindow : Window
    {
        private volatile ObservableCollection<Item> items;
         
        public MainWindow()
        {
            InitializeComponent();

            items = new ObservableCollection<Item>
            {
                new Directory {Name = "raz" },
                new Directory {Name = "dwa"},
                new Directory { Name = "trzy" }
            };

            items[0].Items.Add(new File { Name = "tazPlik.pdf" });

            treeView.ItemsSource = items;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
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

        public MainWindow(List<Item> items)
        {
            InitializeComponent();

            foreach (var item in items)
            {
                treeView.Items.Add(GetItems(item));
            }
        }

        private TreeViewItem GetItems(Item item)
        {
            var tvItem = new TreeViewItem {Header = item.Name};

            var directory = item as Directory;
            if (directory == null || !directory.Items.Any())
                return tvItem;

            foreach (var subItem in directory.Items)
                tvItem.Items.Add(GetItems(subItem));

            return tvItem;
        }
    }
}
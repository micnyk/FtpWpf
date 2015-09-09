using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FtpWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(List<FileSystemModel.FileSystemModelItem> items)
        {
            InitializeComponent();

            foreach (var item in items)
            {
                treeView.Items.Add(GetItems(item));
            }
        }

        private TreeViewItem GetItems(FileSystemModel.FileSystemModelItem item)
        {
            var tvItem = new TreeViewItem { Header = item.Name };

            if (item is FileSystemModel.Directory && (((FileSystemModel.Directory) item).Items.Any()))
                foreach (var subItem in ((FileSystemModel.Directory) item).Items) tvItem.Items.Add(GetItems(subItem));

            return tvItem;
        }
    }
}

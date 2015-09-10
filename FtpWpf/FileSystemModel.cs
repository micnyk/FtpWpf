using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FtpWpf.Annotations;

namespace FtpWpf
{
    namespace FileSystemModel
    {
        public class Item : INotifyCollectionChanged, INotifyPropertyChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            private string _name;
            private string _path;
            private readonly ObservableCollection<Item> _items;

            public Item()
            {
                _items = new ObservableCollection<Item>();
                _items.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs action)
                {
                    CollectionChanged?.Invoke(sender, action);
                };
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; NotifyPropertyChanged("Name"); }
            }

            public string Path
            {
                get { return _path; }
                set { _path = value; NotifyPropertyChanged("Path"); }
            }

            public ObservableCollection<Item> Items => _items;

            protected void NotifyPropertyChanged(string property)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        public class File : Item
        { }

        public class Directory : Item
        { }

        public static class ItemsProvider
        {
            private static readonly Regex LsRegex1 =
                new Regex(
                    @"^(?<dir>[\-ld])(?<permission>([\-r][\-w][\-xs]){3})\s+(?<filecode>\d+)\s+(?<owner>\w+)\s+(?<group>\w+)\s+(?<size>\d+)\s+(?<timestamp>((?<month>\w{3})\s+(?<day>\d{2})\s+(?<hour>\d{1,2}):(?<minute>\d{2}))|((?<month>\w{3})\s+(?<day>\d{2})\s+(?<year>\d{4})))\s+(?<name>.+)$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

            private static readonly Regex LsRegex2 =
                new Regex(
                    @"^((?<dir>([dD]{1}))|)(?<attribs>(.*))\s(?<size>([0-9]{1,}))\s(?<date>((?<monthday>((?<month>(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec))\s(?<day>([0-9\s]{2}))))\s(\s(?<year>([0-9]{4}))|(?<time>([0-9]{2}\:[0-9]{2})))))\s(?<name>([A-Za-z0-9\-\._\s]{1,}))$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

            public static List<Item> GetItems(Stream ftpResponseStream, string relativePath)
            {
                var items = new List<Item>();
                var streamReader = new StreamReader(ftpResponseStream);

                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var matches = LsRegex1.Match(line);
                    if (!matches.Success)
                    {
                        matches = LsRegex2.Match(line);
                        if (!matches.Success)
                            return null;
                    }

                    if (matches.Groups["dir"].Value.ToLower() == "d")
                    {
                        items.Add(new Directory
                        {
                            Name = matches.Groups["name"].Value,
                            Path = relativePath
                        });
                    }
                    else
                    {
                        items.Add(new File
                        {
                            Name = matches.Groups["name"].Value,
                            Path = relativePath
                        });
                    }
                }

                return items;
            }
        }
    }
}
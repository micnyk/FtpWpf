﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
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

            public bool IsSelected { get; set; }

            public Item()
            {
                Items = new ObservableCollection<Item>();
                Items.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs action)
                {
                    CollectionChanged?.Invoke(sender, action);
                };
            }

            public Directory Parent { get; set; }

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

            public ObservableCollection<Item> Items { get; }

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

            public static Item TranslateItem(string lsLine, string relativePath)
            {
                Item item;

                var l = lsLine.Trim(' ', '\t');

                var matches = LsRegex1.Match(lsLine);

                if (!matches.Success)
                {
                    matches = LsRegex2.Match(lsLine);
                    if(!matches.Success)
                        throw new Exception("REGEX DO NOT MATCH!!!");
                }

                if (relativePath[relativePath.Length - 1] != '/')
                    relativePath += "/";

                var name = matches.Groups["name"].Value;

                if (matches.Groups["dir"].Value.ToLower() == "d")
                    item = new Directory
                    {
                        Name = name,
                        Path = relativePath
                    };
                else
                    item = new File
                    {
                        Name = name,
                        Path = relativePath
                    };

                return item;
            }


            // TODO: zmienic items na Directory, zintegrowac w kontrolerze
            public static void AppendItems(Stream ftpResponseStream, string relativePath, ObservableCollection<Item> items)
            {
                using (var streamReader = new StreamReader(ftpResponseStream))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        items.Add(TranslateItem(line, relativePath));
                    }
                }
            }

            public static ObservableCollection<Item> GetItems(Stream ftpResponseStream, string relativePath)
            {
                var items = new ObservableCollection<Item>();
                AppendItems(ftpResponseStream, relativePath, items);
                return items;
            }
        }
    }
}
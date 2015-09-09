using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FtpWpf.FileSystemModel
{
    public class FileSystemModelItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class File : FileSystemModelItem
    { }

    public class Directory : FileSystemModelItem
    {
        public List<FileSystemModelItem> Items { get; set; }

        public Directory()
        {
            Items = new List<FileSystemModelItem>();
        }
    }

    public static class ItemsProvider
    {
        private static Regex lsRegex1 = new Regex(@"^(?<dir>[\-ld])(?<permission>([\-r][\-w][\-xs]){3})\s+(?<filecode>\d+)\s+(?<owner>\w+)\s+(?<group>\w+)\s+(?<size>\d+)\s+(?<timestamp>((?<month>\w{3})\s+(?<day>\d{2})\s+(?<hour>\d{1,2}):(?<minute>\d{2}))|((?<month>\w{3})\s+(?<day>\d{2})\s+(?<year>\d{4})))\s+(?<name>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex lsRegex2 = new Regex(@"^((?<dir>([dD]{1}))|)(?<attribs>(.*))\s(?<size>([0-9]{1,}))\s(?<date>((?<monthday>((?<month>(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec))\s(?<day>([0-9\s]{2}))))\s(\s(?<year>([0-9]{4}))|(?<time>([0-9]{2}\:[0-9]{2})))))\s(?<name>([A-Za-z0-9\-\._\s]{1,}))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<FileSystemModelItem> GetItems(Stream ftpResponseStream, string relativePath)
        {
            var items = new List<FileSystemModelItem>();
            var streamReader = new StreamReader(ftpResponseStream);

            string line;

            while ((line = streamReader.ReadLine()) != null)
            {
                var matches = lsRegex1.Match(line);
                if (!matches.Success)
                {
                    matches = lsRegex2.Match(line);
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

            return items; ;
        }
    }
}

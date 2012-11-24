using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TouchAt
{
    class MainClass
    {
        private const string Usage = "Usage: TouchAt YYYYMMDD file";
        private enum ExitCode : int
        {
            Failure = -1,
            Succeed = 0
        };

        public static void Main(string[] args)
        {
            var main = new MainClass();
            Environment.Exit((int)main.Start(args));
        }

        private ExitCode Start(string[] args)
        {
            if (args.Length < 2)
            {
                this.ShowUsage();
                return ExitCode.Failure;
            }

            var timestamp = this.ParseYYYYMMDD(args.First());
            if (!timestamp.HasValue)
            {
                this.ShowUsage();
                return ExitCode.Succeed;
            }

            var entries = new HashSet<string>();
            foreach (var entry in args.Skip(1))
            {
                if (Directory.Exists(entry))
                {
                    entries.UnionWith(this.GetEntries(entry));
                }
                else if (File.Exists(entry))
                {
                    entries.Add(Path.GetFullPath(entry));
                }
                else
                {
                    // not directory and not file
                    continue;
                }
            }

            this.SetTimeStamp(entries, timestamp.Value);
            return ExitCode.Succeed;
        }

        private void SetTimeStamp(HashSet<string> entries, DateTime timestamp)
        {
            foreach (var entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    Directory.SetLastWriteTime(entry, timestamp);
                }
                else if (File.Exists(entry))
                {
                    File.SetLastWriteTime(entry, timestamp);
                }
                else
                {
                    // not directory and not file
                    continue;
                }

                Console.Out.WriteLine(entry);
            }
        }

        private HashSet<string> GetEntries(string dir)
        {
            var entries = new HashSet<string>();
            entries.Add(Path.GetFullPath(dir));

            foreach (var f in Directory.GetFiles(dir))
            {
                entries.Add(Path.GetFullPath(f));
            }

            foreach (var d in Directory.GetDirectories(dir))
            {
                if (!entries.Contains(Path.GetFullPath(d)))
                {
                    entries.UnionWith(this.GetEntries(d));
                }
            }

            return entries;
        }

        private void ShowUsage()
        {
            Console.Error.WriteLine(Usage);
        }

        private DateTime? ParseYYYYMMDD(string s)
        {
            string format;
            if (Regex.IsMatch(s, "^\\d{8}$"))
            {
                format = "yyyyMMdd";
            }
            else if (Regex.IsMatch(s, "^\\d{6}$"))
            {
                format = "yyMMdd";
            }
            else if (Regex.IsMatch(s, "^\\d{4}$"))
            {
                format = "MMdd";
            }
            else
            {
                return null;
            }

            DateTime result = default(DateTime);
            if (!DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out result))
            {
                return null;
            }

            return result;
        }
    }
}

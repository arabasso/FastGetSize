using System;
using System.IO;
using System.Linq;

namespace FastGetSize
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Directory.GetCurrentDirectory();
            var pattern = "*.*";
            var unit = "";
            var ignoreLinks = false;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-") || args[i].StartsWith("/"))
                {
                    switch (args[i].ToLower())
                    {
                        case "/unit":
                        case "--unit":
                        case "-u":
                        case "/u":
                            unit = args[++i];
                            break;

                        case "/pattern":
                        case "--pattern":
                        case "-p":
                        case "/p":
                            pattern = args[++i];
                            break;

                        case "/ignore-links":
                        case "--ignore-links":
                        case "-il":
                        case "/il":
                            ignoreLinks = true;
                            break;
                    }
                }

                else
                {
                    path = args[i];
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!FileOrDirectoryExists(path))
            {
                return;
            }

            var size = GetSize(path, pattern, ignoreLinks);

            Console.WriteLine(FormatFileSize(size, unit.ToUpper()));

#if DEBUG
            Console.ReadKey();
            Console.WriteLine("Press any key to continue. . .");
#endif
        }

        internal static bool FileOrDirectoryExists(
            string name)
        {
            return Directory.Exists(name) || File.Exists(name);
        }

        static string FormatFileSize(
            long size,
            string unit)
        {
            var units = new[] { "B", "KB", "MB", "GB", "TB" };

            double s = size;
            var index = 0;

            while (s > 1024.0)
            {
                if (units[index] == unit) break;

                s /= 1024.0;

                index++;
            }

            return $"{s:N} {units[index]}";
        }

        private static long GetSize(
            string path,
            string pattern,
            bool ignoreLinks)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                var dirInfo = new DirectoryInfo(path);

                var dirs = dirInfo.EnumerateDirectories();

                if (ignoreLinks)
                {
                    dirs = dirs.Where(w => !w.Attributes.HasFlag(FileAttributes.ReparsePoint));
                }

                return dirs
                    .AsParallel()
                    .SelectMany(di => di.EnumerateFiles(pattern, SearchOption.AllDirectories))
                    .Sum(s => s.Length) + dirInfo.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly).Sum(s => s.Length);
            }

            return new FileInfo(path).Length;
        }
    }
}

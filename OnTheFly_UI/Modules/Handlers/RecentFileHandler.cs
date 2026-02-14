using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.Handlers
{
    public static class RecentFileHandler
    {
        private static readonly string RecentFilesPath = "recent_files.txt";
        public static List<string> GetRecentFiles()
        {
            if (!System.IO.File.Exists(RecentFilesPath))
            {
                return new List<string>();
            }
            return System.IO.File.ReadAllLines(RecentFilesPath).ToList();
        }
        public static void AddRecentFile(string filePath)
        {
            var recentFiles = GetRecentFiles();
            if (recentFiles.Contains(filePath))
            {
                recentFiles.Remove(filePath);
            }
            recentFiles.Insert(0, filePath);
           
            System.IO.File.WriteAllLines(RecentFilesPath, recentFiles);
        }
    }
}

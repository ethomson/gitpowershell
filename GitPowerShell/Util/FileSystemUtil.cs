using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GitPowerShell.Util
{
    public static class FileSystemUtil
    {
        public static IEnumerable<String> GetFilesRecursive(String directory)
        {
            List<String> files = new List<String>();

            foreach (String file in Directory.GetFiles(directory))
            {
                files.Add(file);
            }

            foreach (String subdirectory in Directory.GetDirectories(directory))
            {
                files.AddRange(GetFilesRecursive(subdirectory));
            }

            return files;
        }
    }
}

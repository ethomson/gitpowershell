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
                if (!IsHidden(file))
                {
                    files.Add(file);
                }
            }

            foreach (String subdirectory in Directory.GetDirectories(directory))
            {
                if (!IsHidden(subdirectory))
                {
                    files.AddRange(GetFilesRecursive(subdirectory));
                }
            }

            return files;
        }

        private static bool IsHidden(String filename)
        {
            return Path.GetFileName(filename).StartsWith(".");
        }

        public static String MakeRelative(String path, String fromPath)
        {
            List<String> pathComponents = new List<String>();

            if (fromPath == null)
            {
                return path;
            }

            /* Use FileSystemInfo to canonicalize paths, ensure fromPath ends in a trailing \ to fromPath. */
            path = new FileInfo(path).FullName;
            fromPath = new FileInfo(fromPath + @"\").FullName;

            if (fromPath.Equals(new FileInfo(path + @"\").FullName))
            {
                return ".";
            }

            /* Find the longest common base directory */
            String baseDirectory = null;

            for (int i = 0, lastSeparatorIdx = -1; ; i++)
            {
                if (i == path.Length || i == fromPath.Length || path[i] != fromPath[i])
                {
                    baseDirectory = (lastSeparatorIdx >= 0) ? path.Substring(0, lastSeparatorIdx + 1) : null;
                    break;
                }
                else if (path[i] == Path.DirectorySeparatorChar)
                {
                    lastSeparatorIdx = i;
                }
            }

            /* No common directories (on different drives), no relativization possible */
            if (baseDirectory == null)
            {
                return path;
            }

            /* Find the distance from the relativeFromPath to the baseDirectory */
            String fromRelativeToBase = fromPath.Substring(baseDirectory.Length);
            for (int i = 0; i < fromRelativeToBase.Length; i++)
            {
                if (fromRelativeToBase[i] == Path.DirectorySeparatorChar)
                {
                    pathComponents.Add("..");
                }
            }

            /* Add the path portion relative to the base */
            pathComponents.Add(path.Substring(baseDirectory.Length));

            return String.Join(new String(new char[] { Path.DirectorySeparatorChar }), pathComponents.ToArray());
        }
    }
}

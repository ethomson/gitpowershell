using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using LibGit2Sharp;

using GitPowerShell.Util;

namespace GitPowerShell.Output
{
    public class GitFileSystemStatusEntry
    {
        private readonly String repositoryWorkingDirectory;
        private readonly String filesystemWorkingDirectory;
        private readonly String filePath;
        private readonly FileStatus status;

        public GitFileSystemStatusEntry(String repositoryWorkingDirectory, String filesystemWorkingDirectory, String filePath, FileStatus status)
        {
            this.repositoryWorkingDirectory = repositoryWorkingDirectory;
            this.filesystemWorkingDirectory = filesystemWorkingDirectory;
            this.filePath = filePath;
            this.status = status;
        }

        public String RepositoryWorkingDirectory
        {
            get
            {
                return repositoryWorkingDirectory;
            }
        }

        public String Filename
        {
            get
            {
                return Path.Combine(repositoryWorkingDirectory, filePath);
            }
        }

        public String FilesystemRelativePath
        {
            get
            {
                return FileSystemUtil.MakeRelative(Filename, filesystemWorkingDirectory);
            }
        }

        public String RepositoryRelativeFileName
        {
            get
            {
                return filePath;
            }
        }

        public FileStatus FileStatus
        {
            get
            {
                return status;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

using GitPowerShell.Parameters;

namespace GitPowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "GitStatus")]
    [OutputType(typeof(GitFileStatus))]
    public class GetGitStatusCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromRemainingArguments = true, Position = 0), PathArrayTransformation(Recursive = true)]
        public String[] Path
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            Repository repository = null;
            bool shouldDispose = true;

            try
            {
                RepositoryParameter repositoryParam = Repository;

                if (repositoryParam == null)
                {
                    String repositoryPath = LibGit2Sharp.Repository.Discover(SessionState.Path.CurrentFileSystemLocation.Path);

                    if (repositoryPath == null)
                    {
                        throw new FileNotFoundException("Could not locate git repository based on the current file system location.  Specify -Repository to indicate the repository location.");
                    }

                    repository = new Repository(repositoryPath);
                }
                else
                {
                    repository = Repository.Repository;
                    shouldDispose = Repository.ShouldDispose;
                }

                if(Path != null)
                {
                    foreach (String path in Path)
                    {
                        FileStatus state = repository.Index.RetrieveStatus(path);
                        WriteObject(new GitFileStatus(path, state));
                    }
                }
                else
                {
                    RepositoryStatus status = repository.Index.RetrieveStatus();

                    foreach (StatusEntry entry in status)
                    {
                        WriteObject(new GitFileStatus(System.IO.Path.Combine(repository.Info.WorkingDirectory, entry.FilePath), entry.State));
                    }
                }
            }
            finally
            {
                if (repository != null && shouldDispose)
                {
                    repository.Dispose();
                }
            }
        }
    }

    public class GitFileStatus
    {
        private readonly String filePath;
        private readonly FileStatus state;

        public GitFileStatus(String filePath, FileStatus state)
        {
            this.filePath = filePath;
            this.state = state;
        }

        public String FilePath
        {
            get
            {
                return filePath;
            }
        }

        public FileStatus State
        {
            get
            {
                return state;
            }
        }
    }
}

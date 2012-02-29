using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

using GitPowerShell.Parameters;
using GitPowerShell.Output;
using GitPowerShell.Util;

namespace GitPowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "GitIndexEntry")]
    [OutputType(typeof(GitFileSystemStatusEntry))]
    public class AddGitIndexEntryCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true), PathArrayTransformation(Recursive = true, MustExist = true)]
        public String[] Path
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true), PathArrayTransformation(Recursive = true, Literal = true, MustExist = true)]
        public String[] LiteralPath
        {
            get;
            set;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter All
        {
            get;
            set;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter Update
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            Repository repository = null;
            bool shouldDispose = true;

            String[] addPaths = ArrayUtil.Combine(Path, LiteralPath);

            if (addPaths == null && ! All && ! Update)
            {
                throw new ArgumentException("You must specify paths to add using -Path or -LiteralPath, or use the -All or -Update parameter");
            }
            else if(All && Update)
            {
                throw new ArgumentException("You cannot specify both the -All and -Update parameters");
            }

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

                if(addPaths != null)
                {
                    foreach(String path in addPaths)
                    {
                        String repoRelativePath = FileSystemUtil.MakeRelative(path, repository.Info.WorkingDirectory);

                        WriteVerbose(String.Format("Adding {0}", repoRelativePath));
                        repository.Index.Stage(path);

                        WriteObject(new GitFileSystemStatusEntry(repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, path, repository.Index.RetrieveStatus(path)));
                    }
                }
                else
                {
                    foreach (StatusEntry statusEntry in repository.Index.RetrieveStatus())
                    {
                        if(
                            (statusEntry.State == FileStatus.Untracked && All) ||
                            (statusEntry.State == FileStatus.Missing) ||
                            (statusEntry.State == FileStatus.Modified)
                          )
                        {
                            String repoRelativePath = FileSystemUtil.MakeRelative(statusEntry.FilePath, repository.Info.WorkingDirectory);

                            WriteVerbose(String.Format("Adding {0}", statusEntry.FilePath));
                            repository.Index.Stage(statusEntry.FilePath);

                            WriteObject(new GitFileSystemStatusEntry(repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, statusEntry.FilePath, repository.Index.RetrieveStatus(statusEntry.FilePath)));
                        }
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
}

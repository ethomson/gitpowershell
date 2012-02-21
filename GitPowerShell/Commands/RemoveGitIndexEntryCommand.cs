using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

using GitPowerShell.Parameters;
using GitPowerShell.Util;

namespace GitPowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "GitIndexEntry")]
    public class RemoveGitIndexEntryCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true), PathArrayTransformation(Recursive = true)]
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

                repository.Index.Unstage(Path);
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

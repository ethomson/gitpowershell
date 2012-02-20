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
    public class GetGitStatusCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
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


                WriteObject(repository.Index.RetrieveStatus());
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

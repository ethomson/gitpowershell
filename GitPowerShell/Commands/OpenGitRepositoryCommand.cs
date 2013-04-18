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
    [Cmdlet(VerbsCommon.Open, "GitRepository")]
    [OutputType(typeof(Repository))]
    public class OpenGitRepositoryCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, HelpMessage = "The directory that will contain the git repository.  It will be created if it does not exist."), PathTransformation]
        public String Directory
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            Repository repository;

            if (Directory != null)
            {
                repository = new Repository(Directory);
            }
            else
            {
                String repositoryPath = LibGit2Sharp.Repository.Discover(SessionState.Path.CurrentFileSystemLocation.Path);

                if (repositoryPath == null)
                {
                    throw new FileNotFoundException("Could not locate git repository based on the current file system location.  Specify -Repository to indicate the repository location.");
                }

                repository = new Repository(repositoryPath);
            }

            WriteObject(repository);
        }
    }
}

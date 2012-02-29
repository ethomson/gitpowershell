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
    [Cmdlet(VerbsCommon.Get, "GitRepository")]
    [OutputType(typeof(Repository))]
    public class GetGitRepositoryCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, HelpMessage = "The starting directory to search for the git repository in."), PathTransformation]
        public String Directory
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            /* Get an absolute directory based on the powershell current working directory. */
            String startingDirectory = Directory != null ?
                Directory :
                SessionState.Path.CurrentFileSystemLocation.Path;

            String repositoryDirectory = Repository.Discover(startingDirectory);

            if (repositoryDirectory == null)
            {
                if (Directory == null)
                {
                    throw new FileNotFoundException("Could not locate git repository based on the current file system location.  Specify -Directory to indicate the repository location.");
                }
                else
                {
                    throw new FileNotFoundException(String.Format("Could not locate git repository that contains {0}.", Directory));
                }
            }

            WriteObject(new Repository(repositoryDirectory));
        }
    }
}

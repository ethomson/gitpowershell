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
    public abstract class GitCmdlet : PSCmdlet
    {
        protected RepositoryParameter DiscoverRepository()
        {
            String repositoryPath = LibGit2Sharp.Repository.Discover(SessionState.Path.CurrentFileSystemLocation.Path);

            if (repositoryPath == null)
            {
                throw new FileNotFoundException("Could not locate git repository based on the current file system location.  Specify -Repository to indicate the repository location.");
            }

            return new RepositoryParameter(new Repository(repositoryPath), true);
        }

        protected RepositoryParameter UseOrDiscoverRepository(RepositoryParameter provided)
        {
            if (provided == null)
            {
                provided = DiscoverRepository();
            }

            return provided;
        }
    }
}

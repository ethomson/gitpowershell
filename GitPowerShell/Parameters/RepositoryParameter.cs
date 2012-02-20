using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

namespace GitPowerShell.Parameters
{
    public class RepositoryParameter
    {
        public Repository Repository
        {
            get;
            set;
        }

        public bool ShouldDispose
        {
            get;
            set;
        }

        public RepositoryParameter(Repository repository, bool shouldDispose)
        {
            Repository = repository;
            ShouldDispose = shouldDispose;
        }
    }
}

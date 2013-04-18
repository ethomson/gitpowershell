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
    [Cmdlet(VerbsCommon.Get, "GitBranches")]
    [OutputType(typeof(GitFileSystemStatusEntry))]
    public class GetGitBranchesCommand : GitCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            using (RepositoryParameter container = UseOrDiscoverRepository(Repository))
            {
                foreach (Branch branch in container.Repository.Branches)
                {
                    WriteObject(branch);
                }
            }
        }
    }
}

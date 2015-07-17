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
    [Cmdlet(VerbsCommon.Get, "GitStatus")]
    [OutputType(typeof(GitFileSystemStatusEntry))]
    public class GetGitStatusCommand : GitCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, ValueFromRemainingArguments = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true), PathArrayTransformation(Recursive = true, MustExist = false)]
        public String[] Path
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true), PathArrayTransformation(Recursive = true, Literal = true, MustExist = false)]
        public String[] LiteralPath
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            using (RepositoryParameter container = UseOrDiscoverRepository(Repository))
            {
                String[] paths = ArrayUtil.Combine(Path, LiteralPath);

                if (paths != null)
                {
                    foreach (String path in paths)
                    {
                        FileStatus state = container.Repository.RetrieveStatus(path);
                        WriteObject(new GitFileSystemStatusEntry(container.Repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, path, state));
                    }
                }
                else
                {
                    RepositoryStatus status = container.Repository.RetrieveStatus();

                    foreach (StatusEntry entry in status)
                    {
                        WriteObject(new GitFileSystemStatusEntry(container.Repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, entry.FilePath, entry.State));
                    }
                }
            }
        }
    }
}

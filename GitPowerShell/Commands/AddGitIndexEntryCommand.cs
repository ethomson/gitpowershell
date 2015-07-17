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
    public class AddGitIndexEntryCommand : GitCmdlet
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
            String[] addPaths = ArrayUtil.Combine(Path, LiteralPath);

            if (addPaths == null && !All && !Update)
            {
                throw new ArgumentException("You must specify paths to add using -Path or -LiteralPath, or use the -All or -Update parameter");
            }
            else if(All && Update)
            {
                throw new ArgumentException("You cannot specify both the -All and -Update parameters");
            }

            using (RepositoryParameter container = UseOrDiscoverRepository(Repository))
            {
                if (addPaths != null)
                {
                    foreach (String path in addPaths)
                    {
                        String repoRelativePath = FileSystemUtil.MakeRelative(path, container.Repository.Info.WorkingDirectory);

                        WriteVerbose(String.Format("Adding {0}", repoRelativePath));
                        container.Repository.Stage(path);

                        WriteObject(new GitFileSystemStatusEntry(container.Repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, path, container.Repository.RetrieveStatus(path)));
                    }
                }
                else
                {
                    foreach (StatusEntry statusEntry in container.Repository.RetrieveStatus())
                    {
                        if (
                            (statusEntry.State == FileStatus.Untracked && All) ||
                            (statusEntry.State == FileStatus.Missing) ||
                            (statusEntry.State == FileStatus.Modified)
                          )
                        {
                            String repoRelativePath = FileSystemUtil.MakeRelative(statusEntry.FilePath, container.Repository.Info.WorkingDirectory);

                            WriteVerbose(String.Format("Adding {0}", statusEntry.FilePath));
                            container.Repository.Stage(statusEntry.FilePath);

                            WriteObject(new GitFileSystemStatusEntry(container.Repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, statusEntry.FilePath, container.Repository.RetrieveStatus(statusEntry.FilePath)));
                        }
                    }
                }
            }
        }
    }
}

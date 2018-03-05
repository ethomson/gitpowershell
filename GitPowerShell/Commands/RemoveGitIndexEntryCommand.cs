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
    [Cmdlet(VerbsCommon.Remove, "GitIndexEntry")]
    public class RemoveGitIndexEntryCommand : GitCmdlet, IDynamicParameters
    {
        [Parameter(Mandatory = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter Recursive
        {
            get;
            set;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter Cached
        {
            get;
            set;
        }

        /* Use dynamic parameters for -Path and -LiteralPath so that they can support the -Recursive switch. */
        private PathDynamicParameters pathParameters;
        public Object GetDynamicParameters()
        {
            if (Recursive)
            {
                pathParameters = new RecursivePathDynamicParameters();
            }
            else
            {
                pathParameters = new NonRecursivePathDynamicParameters();
            }

            return pathParameters;
        }

        public interface PathDynamicParameters
        {
            String[] Path
            {
                get;
                set;
            }

            String[] LiteralPath
            {
                get;
                set;
            }
        }

        public class NonRecursivePathDynamicParameters : PathDynamicParameters
        {
            [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true), PathArrayTransformation(MustExist = true)]
            public String[] Path
            {
                get;
                set;
            }

            [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true), PathArrayTransformation(Literal = true, MustExist = true)]
            public String[] LiteralPath
            {
                get;
                set;
            }
        }

        public class RecursivePathDynamicParameters : PathDynamicParameters
        {
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
        }

        protected override void ProcessRecord()
        {
            String[] removePaths = ArrayUtil.Combine(pathParameters.Path, pathParameters.LiteralPath);

            if (removePaths == null)
            {
                throw new ArgumentException("You must specify paths to add using -Path or -LiteralPath");
            }

            using (RepositoryParameter container = UseOrDiscoverRepository(Repository))
            {
                /* Sanity check input, ensure it's in the index */
                foreach (String path in removePaths)
                {
                    FileStatus state = container.Repository.RetrieveStatus(path);

                    if (state == FileStatus.Nonexistent || state == FileStatus.NewInWorkdir)
                    {
                        throw new ArgumentException(String.Format("The item {0} is not tracked", path));
                    }
                }

                foreach (String path in removePaths)
                {
                    if (!Cached)
                    {
                        File.Delete(path);
                    }

                    String repoRelativePath = FileSystemUtil.MakeRelative(path, container.Repository.Info.WorkingDirectory);

                    WriteVerbose(String.Format("Removing {0}", repoRelativePath));
                    LibGit2Sharp.Commands.Unstage(container.Repository, path);

                    WriteObject(new GitFileSystemStatusEntry(container.Repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, path, container.Repository.RetrieveStatus(path)));
                }
            }
        }
    }
}

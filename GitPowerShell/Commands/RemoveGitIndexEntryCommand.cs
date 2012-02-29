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
    public class RemoveGitIndexEntryCommand : PSCmdlet, IDynamicParameters
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
            Repository repository = null;
            bool shouldDispose = true;

            String[] removePaths = ArrayUtil.Combine(pathParameters.Path, pathParameters.LiteralPath);

            if (removePaths == null)
            {
                throw new ArgumentException("You must specify paths to add using -Path or -LiteralPath");
            }

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

                /* Sanity check input, ensure it's in the index */
                foreach (String path in removePaths)
                {
                    FileStatus state = repository.Index.RetrieveStatus(path);

                    if (state == FileStatus.Nonexistent || state == FileStatus.Untracked)
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

                    String repoRelativePath = FileSystemUtil.MakeRelative(path, repository.Info.WorkingDirectory);

                    WriteVerbose(String.Format("Removing {0}", repoRelativePath));
                    repository.Index.Unstage(path);

                    WriteObject(new GitFileSystemStatusEntry(repository.Info.WorkingDirectory, SessionState.Path.CurrentFileSystemLocation.Path, path, repository.Index.RetrieveStatus(path)));
                }
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

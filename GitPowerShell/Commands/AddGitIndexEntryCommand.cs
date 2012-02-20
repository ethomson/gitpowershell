using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

using GitPowerShell.Parameters;
using GitPowerShell.Util;

namespace GitPowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "GitIndexEntry")]
    public class AddGitIndexEntryCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true), AbsolutePathTransformation]
        public String[] Path
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            Repository repository = null;
            bool shouldDispose = true;

            List<String> filesToAdd = new List<String>();

            foreach (String file in Path)
            {
                if (Directory.Exists(file))
                {
                    IEnumerable<String> expandedPaths = FileSystemUtil.GetFilesRecursive(file);

                    foreach (String expandedPath in expandedPaths)
                    {
                        WriteVerbose(String.Format("Adding {0} to staging.", expandedPath));
                    }

                    filesToAdd.AddRange(expandedPaths);
                }
                else if (System.IO.File.Exists(file))
                {
                    WriteVerbose(String.Format("Adding {0} to staging.", file));
                    filesToAdd.Add(file);
                }
                else
                {
                    throw new FileNotFoundException(String.Format("The path {0} was not found", file));
                }
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

                repository.Index.Stage(filesToAdd);
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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Security.Principal;
using System.Management.Automation;
using System.DirectoryServices.AccountManagement;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

using GitPowerShell.Parameters;
using GitPowerShell.Output;
using GitPowerShell.Util;

namespace GitPowerShell.Commands
{
    [Cmdlet("Commit", "GitRepository")]
    [OutputType(typeof(Commit))]
    public class CommitGitRepositoryCommand : PSCmdlet
    {
        private UserPrincipal currentUserPrincipal;
        private String currentUserDisplayName;
        private String currentUserEmail;

        [Parameter(Mandatory = false, HelpMessage = "The repository to query status for."), RepositoryTransformation]
        public RepositoryParameter Repository
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "If set, the created Commit will be returned.")]
        public SwitchParameter PassThru
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public String Message
        {
            get;
            set;
        }

        [Parameter]
        public SwitchParameter Amend
        {
            get;
            set;
        }

        [Parameter]
        public String Author
        {
            get;
            set;
        }

        [Parameter]
        public Nullable<DateTimeOffset> Date
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            Repository repository = null;
            bool shouldDispose = true;

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
                    shouldDispose = PassThru;
                }
                else
                {
                    repository = Repository.Repository;
                    shouldDispose = Repository.ShouldDispose || PassThru;
                }

                LibGit2Sharp.Signature author;
                LibGit2Sharp.Signature committer;

                DateTimeOffset commitTime = DateTimeOffset.Now;
                DateTimeOffset authoredTime = Date.HasValue ? Date.Value : commitTime;

                if (Author != null)
                {
                    author = ParseSignature(Author, authoredTime);
                }
                else
                {
                    author = GetDefaultSignature(repository, authoredTime);
                }

                committer = GetDefaultSignature(repository, commitTime);

                Commit commit = repository.Commit(Message, author, committer, Amend);

                if (PassThru)
                {
                    WriteObject(commit);
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

        private UserPrincipal CurrentUserPrincipal
        {
            get
            {
                if (currentUserPrincipal == null)
                {
                    WriteWarning("Attempting to query directory services for your identity.  Please set the 'user.name' and 'user.email' git configuration options to avoid this (potentially slow) lookup.");

                    currentUserPrincipal = UserPrincipal.Current;
                }

                return currentUserPrincipal;
            }
        }

        private String CurrentUserDisplayName
        {
            get
            {
                if (currentUserDisplayName == null)
                {
                    currentUserDisplayName = CurrentUserPrincipal.DisplayName;
                }

                return currentUserDisplayName;
            }
        }

        private String CurrentUserEmail
        {
            get
            {
                if (currentUserEmail == null)
                {
                    currentUserEmail = CurrentUserPrincipal.EmailAddress;
                }

                return currentUserEmail;
            }
        }

        private LibGit2Sharp.Signature ParseSignature(String author, DateTimeOffset time)
        {
            Debug.Assert(author != null, "author != null");

            int emailStart, emailEnd;

            if((emailStart = author.IndexOf(" <")) < 0)
            {
                throw new Exception("Malformed author name: must be 'Full Name <email@address>'");
            }

            String name = author.Substring(0, emailStart).Trim();
            String email = author.Substring(emailStart + 2);

            if ((emailEnd = email.IndexOf('>')) < 0)
            {
                throw new Exception("Malformed author name: must be 'Full Name <email@address>'");
            }

            /* Try to match the parsing of the git command line.  This is entirely empirical. */
            email = email.Substring(0, emailEnd).TrimStart();

            if ((emailEnd = email.IndexOfAny(new char[] { ' ', '\t', '\r', '\n' })) > 0)
            {
                email = email.Substring(0, emailEnd);
            }

            if (name == null || name.Length == 0 || email == null || email.Length == 0)
            {
                throw new Exception("Malformed author name: must be 'Full Name <email@address>'");
            }

            return new LibGit2Sharp.Signature(name, email, time);
        }

        private LibGit2Sharp.Signature GetDefaultSignature(Repository repository, DateTimeOffset time)
        {
            Debug.Assert(repository != null, "repository != null");

            String name = repository.Config.Get<String>("user.name", null);
            String email = repository.Config.Get<String>("user.email", null);

            if (name == null)
            {
                name = CurrentUserDisplayName;
            }

            if (email == null)
            {
                email = CurrentUserEmail;
            }

            if (name == null || email == null)
            {
                throw new Exception("Could not determine your name or email address from directory services.  Please set the 'user.name' and 'user.email' git configuration options.");
            }

            return new LibGit2Sharp.Signature(name, email, time);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using LibGit2Sharp;

namespace GitPowerShell.Parameters
{
    public class RepositoryTransformationAttribute : ArgumentTransformationAttribute
    {
        public override Object Transform(EngineIntrinsics engineIntrinsics, Object input)
        {
            if (input is PSObject && ((PSObject)input).BaseObject is Repository)
            {
                return new RepositoryParameter((Repository)((PSObject)input).BaseObject, false);
            }

            /* 
             * Create absolute path based on SessionState.Path.CurrentFileSystemLocation,
             * not based on the current working directory, which does not reflect the
             * directory the user is in.
             */
            String directoryPath = Path.GetFullPath(Path.Combine(engineIntrinsics.SessionState.Path.CurrentFileSystemLocation.Path, input.ToString()));

            return new RepositoryParameter(new Repository(directoryPath), true);
        }
    }
}

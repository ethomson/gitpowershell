using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace GitPowerShell.Parameters
{
    public class FileSystemInfoTransformationAttribute : ArgumentTransformationAttribute
    {
        public override Object Transform(EngineIntrinsics engineIntrinsics, Object input)
        {
            if (input is PSObject && ((PSObject)input).BaseObject is FileSystemInfo)
            {
                return ((PSObject)input).BaseObject;
            }

            /* 
             * Create absolute path based on SessionState.Path.CurrentFileSystemLocation,
             * not based on the current working directory, which does not reflect the
             * directory the user is in.
             */
            String filePath = Path.GetFullPath(Path.Combine(engineIntrinsics.SessionState.Path.CurrentFileSystemLocation.Path, input.ToString()));

            if (Directory.Exists(filePath))
            {
                return new DirectoryInfo(filePath);
            }

            return new FileInfo(filePath);
        }
    }
}

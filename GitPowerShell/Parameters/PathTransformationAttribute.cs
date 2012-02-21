using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;

using Microsoft.PowerShell.Commands;

using GitPowerShell.Util;

namespace GitPowerShell.Parameters
{
    public class PathTransformationAttribute : ArgumentTransformationAttribute
    {
        public bool MustExist
        {
            get;
            set;
        }

        public override Object Transform(EngineIntrinsics engineIntrinsics, Object input)
        {
            ProviderInfo provider;
            PSDriveInfo drive;

            String inputPath = engineIntrinsics.SessionState.Path.GetUnresolvedProviderPathFromPSPath(input.ToString(), out provider, out drive);

            if (provider.ImplementingType != typeof(FileSystemProvider))
            {
                throw new ArgumentException(String.Format("Files must be located on the filesystem, they cannot be provided by {0}", provider.ImplementingType.Name));
            }

            if (!File.Exists(inputPath) && MustExist)
            {
                throw new FileNotFoundException(String.Format("The file {0} does not exist", inputPath));
            }

            return inputPath;
        }
    }
}

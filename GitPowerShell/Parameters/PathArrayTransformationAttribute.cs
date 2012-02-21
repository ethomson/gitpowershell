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
    public class PathArrayTransformationAttribute : ArgumentTransformationAttribute
    {
        public bool Recursive
        {
            get;
            set;
        }

        public bool Literal
        {
            get;
            set;
        }

        public bool MustExist
        {
            get;
            set;
        }

        public override Object Transform(EngineIntrinsics engineIntrinsics, Object input)
        {
            List<String> transformedPaths = new List<String>();

            if (input is ICollection)
            {
                foreach (Object inputPath in (ICollection)input)
                {
                    transformedPaths.AddRange((ICollection<String>) Transform(engineIntrinsics, inputPath));
                }
            }
            else
            {
                IEnumerable<String> inputPaths;

                ProviderInfo provider;
                PSDriveInfo drive;

                if (Literal)
                {
                    inputPaths = new String[] { 
                        engineIntrinsics.SessionState.Path.GetUnresolvedProviderPathFromPSPath(input.ToString(), out provider, out drive)
                    };
                }
                else
                {
                    /* Expand wildcards, recurse */
                    inputPaths = engineIntrinsics.SessionState.Path.GetResolvedProviderPathFromPSPath(input.ToString(), out provider);
                }

                if (provider.ImplementingType != typeof(FileSystemProvider))
                {
                    throw new ArgumentException(String.Format("Files must be located on the filesystem, they cannot be provided by {0}", provider.ImplementingType.Name));
                }

                foreach (String inputPath in inputPaths)
                {
                    if (Directory.Exists(inputPath) && Recursive)
                    {
                        transformedPaths.AddRange(FileSystemUtil.GetFilesRecursive(inputPath));
                    }
                    else if (!File.Exists(inputPath) && MustExist)
                    {
                        throw new FileNotFoundException(String.Format("The file {0} does not exist", inputPath));
                    }
                    else
                    {
                        transformedPaths.Add(inputPath);
                    }
                }
            }

            return transformedPaths;
        }
    }
}

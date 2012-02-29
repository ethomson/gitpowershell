using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
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
            bool useResolvedPath = ! Literal;

            if (input is ICollection)
            {
                foreach (Object inputPath in (ICollection)input)
                {
                    transformedPaths.AddRange((ICollection<String>) Transform(engineIntrinsics, inputPath));
                }
            }
            else
            {
                IEnumerable<String> inputPaths = null;

                ProviderInfo provider = null;
                PSDriveInfo drive;

                if(useResolvedPath)
                {
                    /* Expand wildcards, recurse */
                    try
                    {
                        inputPaths = engineIntrinsics.SessionState.Path.GetResolvedProviderPathFromPSPath(input.ToString(), out provider);
                    }
                    catch (ItemNotFoundException)
                    {
                        /*
                         * This will occur when a user specifies a path to a file that no longer exists.  We should try to
                         * use a literal path here to retry.  If the caller does not want to allow nonexistant paths, we
                         * will error below instead.
                         */

                        useResolvedPath = false; /* retry below */
                    }
                }

                if (! useResolvedPath)
                {
                    inputPaths = new String[] { 
                        engineIntrinsics.SessionState.Path.GetUnresolvedProviderPathFromPSPath(input.ToString(), out provider, out drive)
                    };
                }

                Debug.Assert(provider != null);
                Debug.Assert(inputPaths != null);

                if (provider == null || inputPaths == null)
                {
                    throw new Exception("Invalid path transformation");
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

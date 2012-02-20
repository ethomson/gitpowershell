using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace GitPowerShell.Parameters
{
    public class AbsolutePathTransformationAttribute : ArgumentTransformationAttribute
    {
        public bool MustExist
        {
            get;
            set;
        }

        public override Object Transform(EngineIntrinsics engineIntrinsics, Object input)
        {
            if (input is ICollection)
            {
                List<Object> absolutePaths = new List<Object>();

                foreach (Object subInput in (IEnumerable)input)
                {
                    absolutePaths.Add(Transform(engineIntrinsics, subInput));
                }

                return absolutePaths;
            }
            else
            {
                String fullPath;

                if (input is PSObject && ((PSObject)input).BaseObject is FileSystemInfo)
                {
                    fullPath = ((FileSystemInfo)(((PSObject)input).BaseObject)).FullName;
                }
                else
                {
                    /* 
                     * Create absolute path based on SessionState.Path.CurrentFileSystemLocation,
                     * not based on the current working directory, which does not reflect the
                     * directory the user is in.
                     */
                    fullPath = Path.GetFullPath(Path.Combine(engineIntrinsics.SessionState.Path.CurrentFileSystemLocation.Path, input.ToString()));
                }

                if (MustExist && ! File.Exists(fullPath))
                {
                    throw new FileNotFoundException(String.Format("The file {0} does not exist.", fullPath));
                }

                return fullPath;
            }
        }
    }
}

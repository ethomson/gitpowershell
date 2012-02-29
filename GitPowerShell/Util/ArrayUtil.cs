using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitPowerShell.Util
{
    public static class ArrayUtil
    {
        public static T[] Combine<T>(T[] one, T[] two)
        {
            if (one != null && two != null)
            {
                T[] all = new T[one.Length + two.Length];
                Array.Copy(one, 0, all, 0, one.Length);
                Array.Copy(two, 0, all, one.Length, two.Length);
                return all;
            }
            else if (one != null)
            {
                return one;
            }
            else if (two != null)
            {
                return two;
            }

            return null;
        }
    }
}

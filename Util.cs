using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview
{
    public static class Util
    {
        public static IEnumerable<T> Enumerate<T>(T arg0)
        {
            yield return arg0;
        }

        public static IEnumerable<T> Enumerate<T>(T arg0, T arg1)
        {
            yield return arg0;
            yield return arg1;
        }

        public static IEnumerable<T> Enumerate<T>(T arg0, T arg1, T arg2)
        {
            yield return arg0;
            yield return arg1;
            yield return arg2;
        }

        public static IEnumerable<T> Enumerate<T>(T arg0, T arg1, T arg2, T arg3)
        {
            yield return arg0;
            yield return arg1;
            yield return arg2;
            yield return arg3;
        }
    }
}

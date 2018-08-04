using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace X.ObjectStore {
    internal static class Log {
        public static void P (string message, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) {
            Console.WriteLine ("{0} ({1}): {2}", Path.GetFileName (file), line, message);
        }
    }
}

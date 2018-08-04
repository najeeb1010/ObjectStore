using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectStore {
    public static class Rand {
        private static string[] strs = new string[] {
            "lorem ipsum dolor", "sit amet consectetur", "adipiscing elit sed", "do eiusmod tempor",
            "incididunt ut labore", "et dolore magna", "aliqua. Ut enim", "ad minim veniam",
            "quis nostrud exercitation", "ullamco laboris nisi", "ut aliquip ex", "ea commodo consequat.",
            "Duis aute irure", "dolor in reprehenderit", "in voluptate velit", "esse cillum dolore",
            "eu fugiat nulla", "pariatur. Excepteur sint", "occaecat cupidatat non", "proident sunt in",
            "culpa qui officia", "deserunt mollit anim", "id est laborum"
        };
        private static int pointer = 0;

        public static string AnyName {
            get {
                return "N" + Guid.NewGuid ().ToString ().Substring (0, 9).ToUpper ();
            }
        }

        public static string AnyDescription {
            get {
                if (pointer == strs.Length) {
                    pointer = 0;
                }
                return strs[pointer++];
            }
        }

        public static string AnyText {
            get {
                return "X" + Guid.NewGuid ().ToString ().Substring (0, 9).ToUpper ();
            }
        }

        private static int singleSessionVal = 0;
        public static int AnyNumberLargerThanLast {
            get {
                DateTime now = DateTime.Now;
                return (now.Year - 2000)
                    + now.DayOfYear
                    + now.Hour
                    + now.Minute
                    + singleSessionVal++;
            }
        }

        public static string AnyComment {
            get {
                if (pointer == strs.Length) {
                    pointer = 0;
                }
                return strs[pointer++];
            }
        }

        public static string AnyTitle {
            get {
                return "T" + Guid.NewGuid ().ToString ().Substring (0, 9).ToUpper ();
            }
        }
    }
}

using System;
using System.Text;

namespace X.ObjectStore {
    internal static class StringUtils {
        /*
        We don't want to use GUIDs since they are way too long.

        ID: D M YY H M S MS CCC -> 12 chars long

        D -> 1-31 (Base-62 digits: 1 to V)
        M -> 1-12 (Base-62 digits: 1 to C)
        YY -> 01-1295 (Two Base-62 digits can represent 3,843 years: from year 1 CE to year 3843 CE); 2017 will be WX
        H -> 0-23 (Base-62 digits: 0 to N)
        M -> 0-59 (Base-62 digits: 0 to x)
        S -> 0-59 (Base-62 digits: 0 to x)
        MS -> 0-999 (Base-62 digits: 00 to G7)

        CCC -> 3844-238327 (Base-62 digits: 000 to zzz) In decimal, the number range should be between 3844 and 238327 (both values included)

        This means that till the year 3843 CE, we can generate 238,328 unique ids / millisecond. Each id will be of 12 chars.

        This roughly translates to (more than) 857.98 trillion 12-digit ids generated / hour.
        */
        public static string Generate12DigitAppID () {
            StringBuilder opStr = new StringBuilder ();

            // First, get this moment's date and time:
            DateTime now = DateTime.Now;

            int day = now.Day;
            int month = now.Month;
            int year = now.Year;

            int hour = now.Hour;
            int min = now.Minute;
            int sec = now.Second;
            int msec = now.Millisecond;

            // Now convert these values into their HrxaTriDecimal equivalents:

            string htdDay = Base62.Convert (day);

            string htdMonth = Base62.Convert (month);

            string htdYear = Base62.Convert (year);

            string htdHour = Base62.Convert (hour);

            string htdMin = Base62.Convert (min);

            string htdSec = Base62.Convert (sec);

            string htdMSec = Base62.Convert (msec);
            if (htdMSec.Length < 2) {
                htdMSec = "0" + htdMSec;
            }

            string ccc = Guid.NewGuid ().ToString ().Replace ("-", string.Empty).Substring (0, 8);

            opStr.Append (htdMSec);
            opStr.Append (ccc);

            opStr.Append (htdHour);
            opStr.Append (htdDay);

            opStr.Append (htdMin);
            opStr.Append (htdMonth);

            opStr.Append (htdYear);
            opStr.Append (htdSec);

            // TODO
            char randChar = 'a';

            string op = string.Format ("{0}{1}", randChar, opStr.ToString ());

            return op;
        }

        public static string Generate12DigitUniqueID (string append = null, bool uppercase = true) {
            string op = Generate12DigitAppID ();
            if (append != null) {
                op = append + op;
            }
            if (uppercase) {
                return op.ToUpper ();
            }
            return op;
        }
    }
}

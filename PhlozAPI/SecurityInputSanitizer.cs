using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib{
    public class SecurityInputSanitizer
    {
        public const int SQLSAFE = 0;
        public const int USERNAME = 1;
        public const int PASSWORD = 2;
        public const int EMAIL = 3;
        

        public static Boolean SafetyCheck(int sanitytype, string Unvalidated)
        {
            Boolean result = true;

            switch (sanitytype)
            {
                case SQLSAFE: result = SqlSafeCheck(Unvalidated); break;
            }

            return result;
        }

        private static Boolean SqlSafeCheck(string Unvalidated)
        {
            Boolean result = true;
            char[] unallowed = { ';', '*', '<', '>', '/', '%', '\'', '"' };
            foreach (char check in unallowed)
            {
                if (Unvalidated.Contains(check)) result = false;
            }
            return result;
        }
    }
}

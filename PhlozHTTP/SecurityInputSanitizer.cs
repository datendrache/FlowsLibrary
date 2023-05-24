//   Flows Libraries -- Flows Common Classes and Methods
//
//   Copyright (C) 2003-2023 Eric Knight
//   This software is distributed under the GNU Public v3 License
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Proliferation.Flows
{
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

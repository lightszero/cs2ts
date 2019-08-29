using System;
using System.Collections.Generic;
using System.Text;

namespace CodeConverter
{
    static class NameConverter
    {
        public static string ConvertMemberName(string type, string membername)
        {
            if (type == "System.Console")
            {
                if (membername == "WriteLine")
                {
                    return "log";
                }
            }
            if (type == "number")
            {
                if (membername == "ToString")
                    return "toString";
            }
            if (type == "string")
            {
                if (membername == "Length")
                    return "length";
            }
            return null;
        }
        public static string ConvertTypeName(string type)
        {
            if (type == "System.Console")
            {
                return "console";
            }

            return null;
        }
    }
}

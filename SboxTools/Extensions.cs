using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SboxTools
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) => input.First().ToString().ToUpper() + input.Substring(1).ToLower();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string StringJoin(this IEnumerable<object> strings, string separator = "")
        {
            return string.Join(separator, strings);
        }
    }
}

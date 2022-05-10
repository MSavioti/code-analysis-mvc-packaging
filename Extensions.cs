using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP6
{
    public static class Extensions
    {
        public static bool Contains<T>(this T[] self, T target)
        {
            foreach (var item in self)
            {
                if (item.Equals(target))
                    return true;
            }

            return false;
        }

        public static void AddMultiple<T>(this List<T> list, params T[] items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}

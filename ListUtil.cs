using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ndst {

    /// <summary>
    /// List utility.
    /// </summary>
    public static class ListUtil {

        public static IEnumerable<T> ConvertTo<T>(this IEnumerable items) {
            return items.Cast<object>().Select(x => (T)Convert.ChangeType(x, typeof(T)));
        }

        public static List<T> ConvertToList<T>(this IEnumerable items) {
            return items.ConvertTo<T>().ToList();
        }

        public static IList ConvertToList(this IEnumerable items, Type targetType) {
            var method = typeof(ListUtil).GetMethod(
                "ConvertToList",
                new[] { typeof(IEnumerable) });
            var generic = method.MakeGenericMethod(targetType);
            return (IList)generic.Invoke(null, new[] { items });

        }

        public static T[] SubArray<T>(this T[] data, int index, int length) {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

    }

}
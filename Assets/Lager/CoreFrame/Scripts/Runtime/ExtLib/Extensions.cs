using System.Collections.Generic;

namespace CoreFrame.Exts
{
    public delegate void ActionIdx<in T>(T obj, int idx);

    public static class Extensions
    {
        #region List Exts
        /// <summary>
        /// Return last of list element with remove
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                T temp = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return temp;
            }

            return default;
        }

        /// <summary>
        /// Return fist of list element with remove
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Shift<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                T temp = list[0];
                list.RemoveAt(0);
                return temp;
            }

            return default;
        }

        public static void ForEach<T>(this List<T> list, ActionIdx<T> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }
        #endregion

        #region string Exts
        /// <summary>
        /// Get the string slice between the two indexes.
        /// Inclusive for start index, exclusive for end index.
        /// </summary>
        public static string Slice(this string source, int start, int end)
        {
            if (end < 0) // Keep this for negative end support
            {
                end = source.Length + end;
            }
            int len = end - start;               // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }

        public static bool IsNullOrZeroEmpty(this string str)
        {
            if (string.IsNullOrEmpty(str) || str == "0") return true;

            return false;
        }
        #endregion
    }
}
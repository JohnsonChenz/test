using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSys
{
    public delegate void ActionIdx<in T>(T obj, int idx);

    public static class Extensions
    {
        public static void ForEach<T>(this List<T> list, ActionIdx<T> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }

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
        /// Destroy all children
        /// </summary>
        /// <param name="trans"></param>
        public static void DestroyAllChildren(this Transform trans)
        {
            foreach (Transform t in trans)
            {
                UnityEngine.Object.Destroy(t.gameObject);
            }
        }

        public static bool IsNullOrZeroEmpty(this string str)
        {
            if (string.IsNullOrEmpty(str) || str == "0") return true;

            return false;
        }

        public static T SelectToken<T>(this JObject jObject, params object[] keys)
        {
            string path = "";
            for (int i = 0; i < keys.Length; i++)
            {
                if (i == (keys.Length - 1)) path += keys[i].ToString();
                else path += keys[i].ToString() + ".";
            }

            JToken value = jObject.SelectToken(path, false);
            if (value == null) return default;

            return value.Value<T>();
        }
    }
}

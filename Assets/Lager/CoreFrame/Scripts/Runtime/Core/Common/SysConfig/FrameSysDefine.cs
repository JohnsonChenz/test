using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    public class FrameSysDefine
    {
        /** 規範符號 */
        public static readonly char BIND_PREFIX = '_';
        public static readonly char BIND_SEPARATOR = '@';
        public static readonly char BIND_END = '#';

        public static readonly Dictionary<string, string> dictComponentFinder = new Dictionary<string, string>()
        {
            { "_Node", "GameObject"}
        };
    }
}
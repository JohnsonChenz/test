using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    namespace ResFrame
    {
        /// <summary>
        /// 資源類型
        /// </summary>
        public enum ResNodeType
        {
            /** 普通資源 */
            Normal = 1,
            /** 常駐資源 */
            Permanent = 2
        }

        public class ResType
        {
            public ResNodeType resNodeType = ResNodeType.Normal;

            /// <summary>
            /// resNodeType => 屬於種節點類型
            /// </summary>
            /// <param name="resNodeType"></param>
            public ResType(ResNodeType resNodeType = ResNodeType.Normal)
            {
                this.resNodeType = resNodeType;
            }
        }

        public class ResSysDefine
        {
            /* 路徑常量 */
            public static readonly string RES_ROOT_NAME = "ResRoot";

            /* 節點常量 */
            public static readonly string RES_NORMAL_NODE = "Normal";
            public static readonly string RES_PERMANENT_NODE = "Permanent";
        }
    }
}

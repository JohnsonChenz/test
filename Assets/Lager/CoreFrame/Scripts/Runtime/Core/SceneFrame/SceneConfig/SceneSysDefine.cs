using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    namespace SceneFrame
    {
        /// <summary>
        /// 場景類型
        /// </summary>
        public enum SceneNodeType
        {
            /** 普通場景 */
            Normal = 1,
            /** 常駐場景 */
            Permanent = 2
        }

        public class SceneType
        {
            public SceneNodeType sceneNodeType = SceneNodeType.Normal;

            /// <summary>
            /// sceneNodeType => 屬於種節點類型
            /// </summary>
            /// <param name="sceneNodeType"></param>
            public SceneType(SceneNodeType sceneNodeType = SceneNodeType.Normal)
            {
                this.sceneNodeType = sceneNodeType;
            }
        }

        public class SceneSysDefine
        {
            /* 路徑常量 */
            public static readonly string SCENE_ROOT_NAME = "SceneRoot";

            /* 節點常量 */
            public static readonly string SCENE_NORMAL_NODE = "Normal";
            public static readonly string SCENE_PERMANENT_NODE = "Permanent";
        }
    }
}

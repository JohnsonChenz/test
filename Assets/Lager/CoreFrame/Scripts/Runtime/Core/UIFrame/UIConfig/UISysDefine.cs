using System.Collections.Generic;

namespace CoreFrame
{
    namespace UIFrame
    {
        public delegate void MaskEventFunc();

        /// <summary>
        /// 窗體類型
        /// </summary>
        public enum UIFormType
        {
            /** 普通窗口 */
            Normal = 1,
            /** 固定窗口 */
            Fixed = 2,
            /** 彈出窗口 */
            Popup = 3,
            /** 獨立彈出窗口 */
            IndiePopup = 4,
            /** 獨立窗口 */
            Independent = 5,
            /** 【常駐】系統專用彈出窗口 */
            SysPopup = 6,
            /** 【常駐】系統專用訊息窗口 */
            SysMsg = 7
        }

        /// <summary>
        /// <para>None: 沒有Mask, 可以穿透</para>
        /// <para>OpacityZero: 完全透明, 不能穿透</para>
        /// <para>OpacityLow: 高透明度, 不能穿透</para>
        /// <para>OpacityHalf: 半透明度, 不能穿透</para>
        /// <para>OpacityHigh: 低透明度, 不能穿透</para>
        /// <para>OpacityFull: 完全不透明, 不能穿透</para>
        /// </summary>
        public enum UIMaskOpacity
        {
            /** 沒有Mask, 可以穿透 */
            None,
            /** 完全透明, 不能穿透 */
            OpacityZero,
            /** 高透明度, 不能穿透 */
            OpacityLow,
            /** 半透明度, 不能穿透 */
            OpacityHalf,
            /** 低透明度, 不能穿透 */
            OpacityHigh,
            /** 完全不透明, 不能穿透 */
            OpacityFull
        }

        public class UIType
        {
            public UIFormType uiFormType = UIFormType.Normal;
            public int order = 0;

            /// <summary>
            /// <para>
            /// uiFormType => 屬於哪種節點類型
            /// </para>
            /// <para>
            /// order => 基於此類型的階層進行排序設置(值愈大會最後渲染), 超過閥值後將會設置為此階層的最大值【Popup系列此參數會無效】
            /// </para>
            /// </summary>
            /// <param name="uiFormType"></param>
            /// <param name="order"></param>
            public UIType(UIFormType uiFormType = UIFormType.Normal, int order = 0)
            {
                this.uiFormType = uiFormType;
                this.order = order;
            }
        }

        public class MaskType
        {
            public UIMaskOpacity opacity = UIMaskOpacity.OpacityHalf;
            public bool isClickMaskToClose = true;

            /// <summary>
            /// <para>
            /// opacity => (UIMaskOpacity) Mask類型
            /// </para>
            /// <para>
            /// isClickMaskToClose => 是否點擊Mask關閉UI, 預設true
            /// </para>
            /// </summary>
            /// <param name="opacity"></param>
            /// <param name="isClickMaskToClose"></param>
            public MaskType(UIMaskOpacity opacity = UIMaskOpacity.OpacityHalf, bool isClickMaskToClose = true)
            {
                this.opacity = opacity;
                this.isClickMaskToClose = isClickMaskToClose;
            }
        }

        public class UISysDefine
        {
            public class UINode
            {
                public string uid;
                public int order;

                /// <summary>
                /// Init UINode
                /// </summary>
                /// <param name="uid">Node Name</param>
                /// <param name="order">Sorting Order</param>
                public UINode(string uid, int order)
                {
                    this.uid = uid;
                    this.order = order;
                }
            }

            /* 路徑常量 */
            public static readonly string UI_CANVAS = "Canvas";
            public static readonly string UI_ROOT_NAME = "UIRoot";
            public static readonly string UI_MASK_NAME = "UIMaskMgr";
            public static readonly string UI_FREEZE_NAME = "UIFreezeMgr";

            /* 節點常量 【UID + ORDER, 排序小(後層) -> 大(前層)】 */
            public static readonly UINode UI_NORMAL_NODE = new UINode("Normal", -7000);
            public static readonly UINode UI_FIXED_NODE = new UINode("Fixed", -6000);
            public static readonly UINode UI_POPUP_NODE = new UINode("Popup", -5000);
            public static readonly UINode UI_INDIE_POPUP_NODE = new UINode("IndiePopup", -4000);
            public static readonly UINode UI_INDEPENDENT_NODE = new UINode("Independent", -3000);
            public static readonly UINode UI_SYS_POPUP_NODE = new UINode("SysPopup", -2000);
            public static readonly UINode UI_SYS_MSG_NODE = new UINode("SysMsg", -1000);

            /* 節點之間的排序差值 - ORDER DIFFERENCE */
            public static readonly int ORDER_DIFFERENCE = 999;

            /* Mask & Freeze Image Size */
            public static readonly int RECT_SIZE = 4096;
        }
    }
}
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreFrame
{
    namespace UIFrame
    {
        public delegate void AnimEndCb();

        public class UIBase : FrameBase
        {
            [HideInInspector] public string uiName = "";                    // 用於存放UIPath整個路徑名稱
            [HideInInspector] public UIType uiType = null;                  // 定義UI類型, 用於取決於要新增至UIRoot中哪個對應的節點
            [HideInInspector] public MaskType maskType = null;              // 定義Mask類型 (Popup系列)

            /// <summary>
            /// 僅執行一次, 只交由UIManager加載資源時呼叫初始參數 (由各個子類定義類型)
            /// </summary>
            public override void InitThis() { }

            /// <summary>
            /// 僅執行一次, 只交由UIManager加載資源時呼叫初始相關綁定組件
            /// </summary>
            public sealed override void InitFirst()
            {
                base.InitFirst();
            }

            protected override async UniTask OpenSub()
            {
                await UniTask.Yield();
            }
            protected override void CloseSub() { }

            /// <summary>
            /// UI初始相關UI組件, 僅初始一次
            /// </summary>
            protected override void InitOnceComponents() { }

            /// <summary>
            /// UI初始相關註冊按鈕事件等等, 僅初始一次
            /// </summary>
            protected override void InitOnceEvents() { }

            /// <summary>
            /// 每次開啟UI時都會被執行, 子類override
            /// </summary>
            /// <param name="obj"></param>
            protected override void OnShow(object obj) { }

            /// <summary>
            /// 會由Mud調用控制, 收到封包後的一個刷新點, 可以由FuncId去判斷欲刷新的Protocol
            /// </summary>
            /// <param name="funcId"></param>
            public override void OnUpdateOnceAfterProtocol(int funcId = 0) { }

            protected override void OnUpdate(float dt) { }

            /// <summary>
            /// UIManager控制調用Display
            /// </summary>
            public sealed override void Display(object obj)
            {
                this.gameObject.SetActive(true);

                // 進行顯示初始動作【子類OnShow】
                this.OnShow(obj);

                this._AddMask(); // only for Popup

                this.Freeze();
                this.ShowAnim(() =>
                {
                    this.UnFreeze();
                });
            }

            /// <summary>
            ///  UIManager控制調用Hide
            /// </summary>
            public sealed override void Hide(bool disableDoSub = false)
            {
                this.Freeze();
                this.HideAnim(() =>
                {
                    this.UnFreeze();
                    this._RemoveMask(); // only for Popup

                    if (!disableDoSub) this.CloseSub();
                    this.OnClose();
                    this.gameObject.SetActive(false);
                });
            }

            /// <summary>
            /// 專屬Popup才有的Mask (增加)
            /// </summary>
            private void _AddMask()
            {
                if (this.uiType.uiFormType == UIFormType.Popup || this.uiType.uiFormType == UIFormType.IndiePopup || this.uiType.uiFormType == UIFormType.SysPopup)
                {
                    UIManager.GetInstance().uiMaskManager.AddMask(this.transform, this.MaskEvent);
                    UIManager.GetInstance().uiMaskManager.SetMaskAlpha(this.maskType.opacity);
                }
            }

            /// <summary>
            /// 專屬Popup才有的Mask (移除)
            /// </summary>
            private void _RemoveMask()
            {
                if (this.uiType.uiFormType == UIFormType.Popup || this.uiType.uiFormType == UIFormType.IndiePopup || this.uiType.uiFormType == UIFormType.SysPopup)
                {
                    UIManager.GetInstance().uiMaskManager.RemoveMask(this.transform);
                }
            }

            /// <summary>
            /// Popup系列會由UIManager控制調用Freeze, 其餘的會由Display() or Hide()
            /// </summary>
            public void Freeze()
            {
                UIManager.GetInstance().uiFreezeManager.AddFreeze(this.transform);
            }

            /// <summary>
            /// Popup系列會由UIManager控制調用, 其餘的會由Display() or Hide()
            /// </summary>
            public void UnFreeze()
            {
                UIManager.GetInstance().uiFreezeManager.RemoveFreeze(this.transform);
            }

            /// <summary>
            /// 子類調用關閉自己
            /// </summary>
            protected sealed override void CloseSelf()
            {
                UIManager.GetInstance().Close(this.uiName);
            }

            protected virtual void MaskEvent()
            {
                if (this.maskType.isClickMaskToClose) UIManager.GetInstance().Close(this.uiName);
            }

            #region UI動畫過度
            protected virtual void ShowAnim(AnimEndCb animEndCb)
            {
                animEndCb();
            }

            protected virtual void HideAnim(AnimEndCb animEndCb)
            {
                animEndCb();
            }
            #endregion
        }
    }
}
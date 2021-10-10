using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using static UnityEngine.UI.GraphicRaycaster;
using CoreFrame.Exts;

namespace CoreFrame
{
    namespace UIFrame
    {
        public class UIManager : FrameManager<UIBase>
        {
            public UIMaskManager uiMaskManager = null;      // UIMaskMgr, 由UIManager進行單例管控
            public UIFreezeManager uiFreezeManager = null;  // UIFreezeMgr, 由UIManager進行單例管控

            private GameObject _goCanvas = null;          // Canvas物件節點
            private GameObject _goUIRoot = null;          // UIRoot物件節點
            private GameObject _goNormal = null;          // Normal物件節點, 會將對應的UIFormType加入此節點
            private GameObject _goFixed = null;           // Fixed物件節點, 會將對應的UIFormType加入此節點
            private GameObject _goPopup = null;           // Popup物件節點, 會將對應的UIFormType加入此節點
            private GameObject _goIndiePopup = null;      // IndiePopup物件節點, 會將對應的UIFormType加入此節點
            private GameObject _goIndependent = null;     // Independent物件節點, 會將對應的UIFormType加入此節點
            private GameObject _goSysPopup = null;        // SysPopup物件節點, 會將對應的UIFormType加入此節點
            private GameObject _goSysMsg = null;          // SysMsg物件節點, 會將對應的UIFormType加入此節點

            private Dictionary<string, UIBase> _dictNormalForms = new Dictionary<string, UIBase>();          // 一般的窗體【載入此Cache的類型: UIFormType.Normal】
            private Dictionary<string, UIBase> _dictFixedForms = new Dictionary<string, UIBase>();           // 固定的窗體【載入此Cache的類型: UIFormType.Fixed】
            private List<UIBase> _listPopupForms = new List<UIBase>();                                       // Popup的窗體【載入此Cache的類型: UIFormType.Popup】
            private List<UIBase> _listIndiePopupForms = new List<UIBase>();                                  // 獨立Popup的窗體【載入此Cache的類型: UIFormType.IndiePopup】
            private Dictionary<string, UIBase> _dictIndependentForms = new Dictionary<string, UIBase>();     // 獨立窗體 (通常用於LoadingUI)【載入此Cache的類型: UIFormType.Independent】
            private List<UIBase> _listSysPopupForms = new List<UIBase>();                                    // 【常駐】系統專用Popup的窗體【載入此Cache的類型: UIFormType.SysPopUp】
            private Dictionary<string, UIBase> _dictSysMsgForms = new Dictionary<string, UIBase>();          // 【常駐】系統專用訊息的窗體【載入此Cache的類型: UIFormType.SysMsg】

            private static UIManager _instance = null;
            public static UIManager GetInstance()
            {
                if (GameObject.Find(UISysDefine.UI_CANVAS) == null)
                {
                    Debug.Log(string.Format("<color=#FF0000>ERROR: 【Canvas】 not found, please to check your scene.</color>"));
                    return null;
                }

                if (_instance == null) _instance = GameObject.Find(UISysDefine.UI_CANVAS).AddComponent<UIManager>();
                return _instance;
            }

            private void Awake()
            {
                this._goCanvas = GameObject.Find(UISysDefine.UI_CANVAS);

                // 設置UIRoot
                this._goUIRoot = this._CreateUIRoot(UISysDefine.UI_ROOT_NAME);
                if (this._goUIRoot == null) return;

                // 設置UINode
                this._goNormal = this._CreateUINode(UISysDefine.UI_NORMAL_NODE);
                this._goFixed = this._CreateUINode(UISysDefine.UI_FIXED_NODE);
                this._goPopup = this._CreateUINode(UISysDefine.UI_POPUP_NODE);
                this._goIndiePopup = this._CreateUINode(UISysDefine.UI_INDIE_POPUP_NODE);
                this._goIndependent = this._CreateUINode(UISysDefine.UI_INDEPENDENT_NODE);
                this._goSysPopup = this._CreateUINode(UISysDefine.UI_SYS_POPUP_NODE);
                this._goSysMsg = this._CreateUINode(UISysDefine.UI_SYS_MSG_NODE);

                // 建立UIMask & UIFreeze容器
                this.uiMaskManager = new UIMaskManager();
                this.uiFreezeManager = new UIFreezeManager();
                GameObject uiMaskGo = this._CreateUIContainer(UISysDefine.UI_MASK_NAME);
                GameObject uiFreezeGo = this._CreateUIContainer(UISysDefine.UI_FREEZE_NAME);
                this.uiMaskManager.Init(uiMaskGo.transform);
                this.uiFreezeManager.Init(uiFreezeGo.transform);
            }

            #region 初始建立Node相關方法
            private GameObject _CreateUIRoot(string uid)
            {
                // 檢查是否已經有先被創立了
                GameObject uiRootChecker = this._goCanvas.transform.Find(uid)?.gameObject;
                if (uiRootChecker != null) return uiRootChecker;

                GameObject uiRootGo = new GameObject(UISysDefine.UI_ROOT_NAME, typeof(RectTransform));
                uiRootGo.layer = LayerMask.NameToLayer("UI");
                // 設置uiRoot為Canvas的子節點
                uiRootGo.transform.SetParent(this._goCanvas.transform);

                // 校正RectTransform
                RectTransform uiRootRect = uiRootGo.GetComponent<RectTransform>();
                uiRootRect.anchorMin = Vector2.zero;
                uiRootRect.anchorMax = Vector2.one;
                uiRootRect.sizeDelta = Vector2.zero;
                uiRootRect.localScale = Vector3.one;
                uiRootRect.localPosition = Vector3.zero;

                return uiRootGo;
            }

            private GameObject _CreateUINode(UISysDefine.UINode uiNode)
            {
                // 檢查是否已經有先被創立了
                GameObject uiNodeChecker = this._goUIRoot.transform.Find(uiNode.uid)?.gameObject;
                if (uiNodeChecker != null) return uiNodeChecker;

                GameObject uiNodeGo = new GameObject(uiNode.uid, typeof(RectTransform), typeof(Canvas));
                uiNodeGo.layer = LayerMask.NameToLayer("UI");
                // 設置uiNode為uiRoot的子節點
                uiNodeGo.transform.SetParent(this._goUIRoot.transform);

                // 校正RectTransform
                RectTransform uiNodeRect = uiNodeGo.GetComponent<RectTransform>();
                uiNodeRect.anchorMin = Vector2.zero;
                uiNodeRect.anchorMax = Vector2.one;
                uiNodeRect.sizeDelta = Vector2.zero;
                uiNodeRect.localScale = Vector3.one;
                uiNodeRect.localPosition = Vector3.zero;

                // 設置Canvas參數
                Canvas uiNodeCanvas = uiNodeGo.GetComponent<Canvas>();
                uiNodeCanvas.overridePixelPerfect = true;
                uiNodeCanvas.pixelPerfect = true;
                uiNodeCanvas.overrideSorting = true;
                uiNodeCanvas.sortingOrder = uiNode.order;
                uiNodeCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

                return uiNodeGo;
            }

            private GameObject _CreateUIContainer(string uid)
            {
                // 檢查是否已經有先被創立了
                GameObject uiContainerChecker = this._goUIRoot.transform.Find(uid)?.gameObject;
                if (uiContainerChecker != null) return uiContainerChecker;

                GameObject uiContainerGo = new GameObject(uid, typeof(RectTransform));
                uiContainerGo.layer = LayerMask.NameToLayer("UI");
                // 設置uiContainer為uiRoot的子節點
                uiContainerGo.transform.SetParent(this._goUIRoot.transform);

                // 校正Transform
                uiContainerGo.transform.localScale = Vector3.one;
                uiContainerGo.transform.localPosition = Vector3.zero;

                return uiContainerGo;
            }
            #endregion

            #region 實作Loading
            protected override async UniTask<UIBase> _LoadingFrameBase(string pathName)
            {
                GameObject go = await this._LoadGameObject(pathName);
                if (go == null) return null;

                GameObject instPref = Instantiate(go, this._goUIRoot.transform);  // instantiate 【UI Prefab】 (需先指定Instantiate Parent為UIRoot不然Canvas初始會跑掉)
                instPref.name = instPref.name.Replace("(Clone)", "");             // Replace Name
                UIBase uiBase = instPref.GetComponent<UIBase>();                  // 取得UIBase組件
                if (uiBase == null) return null;

                this._CalibrateCanvas(instPref);           // 校正Canvas相關組件參數

                uiBase.uiName = pathName;                  // Clone取得UIBase組件後, 就開始指定pathName作為uiName
                uiBase.InitThis();                         // Clone取得UIBase組件後, 也需要初始UI相關配置, 不然後面無法正常運作
                uiBase.InitFirst();                        // Clone取得UIBase組件後, 也需要初始UI相關綁定組件設定

                // >>> 需在InitThis之後, 以下設定開始生效 <<<

                this._SetParent(uiBase);                   // 透過UIFormType類型, 設置Parent
                this._SetOrder(uiBase);                    // SortingOrder設置需在SetActive(false)之前就設置, 基於UIFormType的階層去設置排序

                uiBase.gameObject.SetActive(false);        // 最後設置完畢後, 關閉GameObject的Active為false

                this._dictAllCached.Add(pathName, uiBase); // 最後加入所有快取

                return uiBase;
            }
            #endregion

            #region 相關校正與設置
            /// <summary>
            /// 校正Canvas相關組件參數
            /// </summary>
            /// <param name="instGo"></param>
            private void _CalibrateCanvas(GameObject instGo)
            {
                // 校正uiBase Canvas
                Canvas uiBaseCanvas = instGo.GetComponent<Canvas>();
                if (uiBaseCanvas != null)
                {
                    uiBaseCanvas.overridePixelPerfect = true;
                    uiBaseCanvas.pixelPerfect = true;
                    uiBaseCanvas.overrideSorting = false;
                    uiBaseCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
                }
                // 校正uiBase Graphic Raycaster
                GraphicRaycaster uiBaseGraphicRaycaster = instGo.GetComponent<GraphicRaycaster>();
                if (uiBaseGraphicRaycaster != null)
                {
                    uiBaseGraphicRaycaster.ignoreReversedGraphics = true;
                    uiBaseGraphicRaycaster.blockingObjects = BlockingObjects.None;
                    uiBaseGraphicRaycaster.blockingMask = ~0;                               // ~0 or -1 = Nothing
                    uiBaseGraphicRaycaster.blockingMask = 1 << LayerMask.NameToLayer("UI"); // 1 << 位元左移至UI的編號位子
                }
            }

            /// <summary>
            /// 依照對應的Node類型設置母節點
            /// </summary>
            /// <param name="uiBase"></param>
            protected override void _SetParent(UIBase uiBase)
            {
                switch (uiBase.uiType.uiFormType)
                {
                    case UIFormType.Normal:
                        uiBase.gameObject.transform.SetParent(this._goNormal.transform);
                        break;

                    case UIFormType.Fixed:
                        uiBase.gameObject.transform.SetParent(this._goFixed.transform);
                        break;

                    case UIFormType.Popup:
                        uiBase.gameObject.transform.SetParent(this._goPopup.transform);
                        break;

                    case UIFormType.IndiePopup:
                        uiBase.gameObject.transform.SetParent(this._goIndiePopup.transform);
                        break;

                    case UIFormType.Independent:
                        uiBase.gameObject.transform.SetParent(this._goIndependent.transform);
                        break;

                    case UIFormType.SysPopup:
                        uiBase.gameObject.transform.SetParent(this._goSysPopup.transform);
                        break;

                    case UIFormType.SysMsg:
                        uiBase.gameObject.transform.SetParent(this._goSysMsg.transform);
                        break;
                }
            }

            /// <summary>
            /// 設置SortingOrder渲染排序
            /// </summary>
            /// <param name="uiBase"></param>
            private void _SetOrder(UIBase uiBase)
            {
                Canvas uiBaseCanvas = uiBase.gameObject?.GetComponent<Canvas>();
                if (uiBaseCanvas == null) return;

                if (uiBase.uiType.order <= 0 ||
                    // Popup系列本身是屬於Stack, 會透過物件層級將階層自動移至Top, 所以不用固定排序
                    uiBase.uiType.uiFormType == UIFormType.Popup ||
                    uiBase.uiType.uiFormType == UIFormType.IndiePopup ||
                    uiBase.uiType.uiFormType == UIFormType.SysPopup) return;

                uiBaseCanvas.overrideSorting = true;

                int uiOrder = (uiBase.uiType.order >= UISysDefine.ORDER_DIFFERENCE) ? UISysDefine.ORDER_DIFFERENCE : uiBase.uiType.order;
                switch (uiBase.uiType.uiFormType)
                {
                    case UIFormType.Normal:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_NORMAL_NODE.order + uiOrder;
                        break;

                    case UIFormType.Fixed:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_FIXED_NODE.order + uiOrder;
                        break;

                    case UIFormType.Popup:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_POPUP_NODE.order + uiOrder;
                        break;

                    case UIFormType.IndiePopup:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_INDIE_POPUP_NODE.order + uiOrder;
                        break;

                    case UIFormType.Independent:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_INDEPENDENT_NODE.order + uiOrder;
                        break;

                    case UIFormType.SysPopup:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_SYS_POPUP_NODE.order + uiOrder;
                        break;

                    case UIFormType.SysMsg:
                        uiBaseCanvas.sortingOrder = UISysDefine.UI_SYS_MSG_NODE.order + uiOrder;
                        break;
                }
            }
            #endregion

            #region Show
            /// <summary>
            /// 顯示UI
            /// </summary>
            /// <param name="uiName"></param>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override async UniTask<UIBase> Show(string uiName, object obj = null)
            {
                if (uiName.IsNullOrZeroEmpty()) return null;

                if (this.CheckIsShowing(uiName))
                {
                    Debug.LogWarning(string.Format("【UI】{0}已經存在了!!!", uiName));
                    return null;
                }

                UIBase uiBase = await this._LoadIntoAllCahce(uiName);
                if (uiBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【UI】: {0}", uiName));
                    return null;
                }

                switch (uiBase.uiType.uiFormType)
                {
                    case UIFormType.Normal:
                        await this._LoadUIFormToNormCache(uiName, obj);
                        break;

                    case UIFormType.Fixed:
                        await this._LoadUIFormToFixedCache(uiName, obj);
                        break;

                    case UIFormType.Popup:
                        await this._PushUIFormToPopupStack(uiName, obj);
                        break;

                    case UIFormType.IndiePopup:
                        await this._PushUIFormToIndiePopupStack(uiName, obj);
                        break;

                    case UIFormType.Independent:
                        await this._LoadUIFormToIndieCache(uiName, obj);
                        break;

                    case UIFormType.SysPopup:
                        await this._PushUIFormToSysPopupStack(uiName, obj);
                        break;

                    case UIFormType.SysMsg:
                        await this._LoadUIFormToSysMsgCache(uiName, obj);
                        break;
                }

                Debug.Log(string.Format("顯示UI: 【{0}】", uiName));

                return uiBase;
            }
            #endregion

            #region Close
            /// <summary>
            /// 關閉UI
            /// </summary>
            /// <param name="uiName"></param>
            /// <param name="withDestroy"></param>
            public override void Close(string uiName, bool disableDoSub = false, bool withDestroy = false)
            {
                if (uiName.IsNullOrZeroEmpty() || !this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;

                switch (uiBase.uiType.uiFormType)
                {
                    case UIFormType.Normal:
                        this._ExitNormalForm(uiName, disableDoSub);
                        break;

                    case UIFormType.Fixed:
                        this._ExitFixedForm(uiName, disableDoSub);
                        break;

                    case UIFormType.Popup:
                        this._RemovePopupForm(disableDoSub);
                        break;

                    case UIFormType.IndiePopup:
                        this._RemoveIndiePopupForm(disableDoSub);
                        break;

                    case UIFormType.Independent:
                        this._ExitIndependentForm(uiName, disableDoSub);
                        break;

                    case UIFormType.SysPopup:
                        this._RemoveSysPopupForm(disableDoSub);
                        break;

                    case UIFormType.SysMsg:
                        this._ExitSysMsgForm(uiName, disableDoSub);
                        break;
                }

                if (withDestroy) this._Destroy(uiBase, uiName);
                else if (uiBase.isCloseAndDestroy) this._Destroy(uiBase, uiName);

                Debug.Log(string.Format("關閉Scene: 【{0}】", uiName));
            }

            /// <summary>
            /// 關閉所有UI (自動排除Permanent類型)
            /// </summary>
            /// <param name="withDestroy">是否執行Destroy</param>
            /// <param name="withoutUINames">欲排除執行的UINames</param>
            /// <returns></returns>
            public override async UniTask CloseAll(bool disableDoSub = false, bool withDestroy = false, params string[] withoutUINames)
            {
                // 需要注意快取需要temp出來, 因為如果for迴圈裡有功能直接對快取進行操作會出錯
                foreach (UIBase uiBase in this._dictAllCached.Values.ToList())
                {
                    string uiName = uiBase.uiName;
                    if (uiBase == null) continue;

                    // 檢查排除執行的UI
                    bool checkWithout = false;
                    if (withoutUINames.Length > 0)
                    {
                        for (int i = 0; i < withoutUINames.Length; i++)
                        {
                            if (uiName == withoutUINames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的UI直接掠過處理
                    if (checkWithout) continue;

                    // 如果沒有強制Destroy就判斷有開啟的UI才列入刪除
                    if (!withDestroy && !this.CheckIsShowing(uiBase)) continue;

                    // Permanent類型不列入刪除執行 (SysPopup & SysMsg)
                    switch (uiBase.uiType.uiFormType)
                    {
                        case UIFormType.Normal:
                            this._ExitNormalForm(uiName, disableDoSub);
                            break;

                        case UIFormType.Fixed:
                            this._ExitFixedForm(uiName, disableDoSub);
                            break;

                        case UIFormType.Popup:
                            this._RemovePopupForm(disableDoSub);
                            break;

                        case UIFormType.IndiePopup:
                            this._RemoveIndiePopupForm(disableDoSub);
                            break;

                        case UIFormType.Independent:
                            this._ExitIndependentForm(uiName, disableDoSub);
                            break;
                    }

                    // Permanent類型不列入刪除執行 (SysPopup & SysMsg)
                    switch (uiBase.uiType.uiFormType)
                    {
                        case UIFormType.Normal:
                        case UIFormType.Fixed:
                        case UIFormType.Popup:
                        case UIFormType.IndiePopup:
                        case UIFormType.Independent:
                            if (withDestroy) this._Destroy(uiBase, uiName);
                            else if (uiBase.isCloseAndDestroy) this._Destroy(uiBase, uiName);
                            break;
                    }
                }

                // 等待執行完畢
                await UniTask.Yield();
            }
            #endregion

            #region 開啟窗體, 並載入窗體至對應的Cache
            private async UniTask _LoadUIFormToNormCache(string uiName, object obj)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._dictNormalForms.Add(uiName, uiBase);
                uiBase.Display(obj);
            }

            private async UniTask _LoadUIFormToFixedCache(string uiName, object obj)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._dictFixedForms.Add(uiName, uiBase);
                uiBase.Display(obj);
            }

            private async UniTask _PushUIFormToPopupStack(string uiName, object obj)
            {
                if (this._listPopupForms.Count >= 2)
                {
                    UIBase topUIBase = this._listPopupForms[this._listPopupForms.Count - 1];
                    topUIBase.Freeze();
                }

                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._listPopupForms.Add(uiBase);
                uiBase.gameObject.transform.SetAsLastSibling();
                uiBase.Display(obj);
            }

            private async UniTask _PushUIFormToIndiePopupStack(string uiName, object obj)
            {
                if (this._listIndiePopupForms.Count >= 2)
                {
                    UIBase topUIBase = this._listIndiePopupForms[this._listIndiePopupForms.Count - 1];
                    topUIBase.Freeze();
                }

                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._listIndiePopupForms.Add(uiBase);
                uiBase.gameObject.transform.SetAsLastSibling();
                uiBase.Display(obj);
            }

            private async UniTask _LoadUIFormToIndieCache(string uiName, object obj)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._dictIndependentForms.Add(uiName, uiBase);
                uiBase.Display(obj);
            }

            private async UniTask _PushUIFormToSysPopupStack(string uiName, object obj)
            {
                if (this._listSysPopupForms.Count >= 2)
                {
                    UIBase topUIBase = this._listSysPopupForms[this._listSysPopupForms.Count - 1];
                    topUIBase.Freeze();
                }

                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._listSysPopupForms.Add(uiBase);
                uiBase.gameObject.transform.SetAsLastSibling();
                uiBase.Display(obj);
            }

            private async UniTask _LoadUIFormToSysMsgCache(string uiName, object obj)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                await uiBase.PreInit();
                this._dictSysMsgForms.Add(uiName, uiBase);
                uiBase.Display(obj);
            }
            #endregion

            #region 關閉窗體, 並從對應中的Cache移除
            private void _ExitNormalForm(string uiName, bool disableDoSub)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                uiBase.Hide(disableDoSub);
                this._dictNormalForms.Remove(uiName);
            }

            private void _ExitFixedForm(string uiName, bool disableDoSub)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                uiBase.Hide(disableDoSub);
                this._dictFixedForms.Remove(uiName);
            }

            private void _RemovePopupForm(bool disableDoSub)
            {
                if (this._listPopupForms.Count >= 2)
                {
                    // 先取出最後一個執行Hide(), 並且移除快取
                    UIBase topUIBase = this._listPopupForms.Pop();
                    topUIBase.Hide(disableDoSub);

                    // 再一次取出最後一個(相對是倒數第二個), 取出後解凍UI
                    if (this._listPopupForms.Count <= 1) return;
                    topUIBase = this._listPopupForms[this._listPopupForms.Count - 1];
                    topUIBase.UnFreeze();
                }
                else if (this._listPopupForms.Count == 1)
                {
                    UIBase topUIBase = this._listPopupForms.Pop();
                    topUIBase.Hide(disableDoSub);
                }
            }

            private void _RemoveIndiePopupForm(bool disableDoSub)
            {
                if (this._listIndiePopupForms.Count >= 2)
                {
                    // 先取出最後一個執行Hide(), 並且移除快取
                    UIBase topUIBase = this._listIndiePopupForms.Pop();
                    topUIBase.Hide(disableDoSub);

                    // 再一次取出最後一個(相對是倒數第二個), 取出後解凍UI
                    topUIBase = this._listIndiePopupForms[this._listIndiePopupForms.Count - 1];
                    topUIBase.UnFreeze();
                }
                else if (this._listIndiePopupForms.Count == 1)
                {
                    UIBase topUIBase = this._listIndiePopupForms.Pop();
                    topUIBase.Hide(disableDoSub);
                }
            }

            private void _ExitIndependentForm(string uiName, bool disableDoSub)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                uiBase.Hide(disableDoSub);
                this._dictIndependentForms.Remove(uiName);
            }

            private void _RemoveSysPopupForm(bool disableDoSub)
            {
                if (this._listSysPopupForms.Count >= 2)
                {
                    // 先取出最後一個執行Hide(), 並且移除快取
                    UIBase topUIBase = this._listSysPopupForms.Pop();
                    topUIBase.Hide(disableDoSub);

                    // 再一次取出最後一個(相對是倒數第二個), 取出後解凍UI
                    topUIBase = this._listSysPopupForms[this._listSysPopupForms.Count - 1];
                    topUIBase.UnFreeze();
                }
                else if (this._listSysPopupForms.Count == 1)
                {
                    UIBase topUIBase = this._listSysPopupForms.Pop();
                    topUIBase.Hide(disableDoSub);
                }
            }

            private void _ExitSysMsgForm(string uiName, bool disableDoSub)
            {
                if (!this.HasInAllCached(uiName)) return;

                UIBase uiBase = this._GetFromAllCached(uiName);
                if (uiBase == null) return;
                uiBase.Hide(disableDoSub);
                this._dictSysMsgForms.Remove(uiName);
            }
            #endregion
        }
    }
}
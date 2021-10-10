using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CoreFrame.Exts;

namespace CoreFrame
{
    namespace ResFrame
    {
        public class ResManager : FrameManager<ResBase>
        {
            private GameObject _goResRoot = null;    // ResRoot節點       
            private GameObject _goNormal = null;     // Normal物件節點, 會將對應的ResNodeType加入此節點
            private GameObject _goPermanent = null;  // Permanent物件節點, 會將對應的ResNodeType加入此節點

            private Dictionary<string, ResBase> _dictNormalTypes = new Dictionary<string, ResBase>();       // 一般的資源【載入此Cache的類型: ResNodeType.Normal】
            private Dictionary<string, ResBase> _dictPermanentTypes = new Dictionary<string, ResBase>();    // 常駐的資源【載入此Cache的類型: ResNodeType.Permanent】

            private static ResManager _instance = null;
            public static ResManager GetInstance()
            {
                if (_instance == null)
                {
                    if (GameObject.Find(ResSysDefine.RES_ROOT_NAME) != null) _instance = GameObject.Find(ResSysDefine.RES_ROOT_NAME).AddComponent<ResManager>();
                    else _instance = new GameObject(ResSysDefine.RES_ROOT_NAME).AddComponent<ResManager>();
                }
                return _instance;
            }

            private void Awake()
            {
                this._goResRoot = GameObject.Find(ResSysDefine.RES_ROOT_NAME);
                if (this._goResRoot == null) return;

                // 設置ResNode
                this._goNormal = this._CreateResNode(ResSysDefine.RES_NORMAL_NODE);
                this._goPermanent = this._CreateResNode(ResSysDefine.RES_PERMANENT_NODE);
            }

            #region 初始建立Node相關方法
            private GameObject _CreateResNode(string resNodeName)
            {
                // 檢查是否已經有先被創立了
                GameObject resNodeChecker = this._goResRoot.transform.Find(resNodeName)?.gameObject;
                if (resNodeChecker != null) return resNodeChecker;

                GameObject resNodeGo = new GameObject(resNodeName);
                // 設置resNode為resRoot的子節點
                resNodeGo.transform.SetParent(this._goResRoot.transform);

                // 校正Transform
                resNodeGo.transform.localScale = Vector3.one;
                resNodeGo.transform.localPosition = Vector3.zero;

                return resNodeGo;
            }
            #endregion

            #region 實作Loading
            protected override async UniTask<ResBase> _LoadingFrameBase(string pathName)
            {
                GameObject go = await this._LoadGameObject(pathName);
                if (go == null) return null;

                GameObject instPref = Instantiate(go, this._goResRoot.transform);  // instantiate 【Res Prefab】 (先指定Instantiate Parent為ResRoot)
                instPref.name = instPref.name.Replace("(Clone)", "");              // Replace Name
                ResBase resBase = instPref.GetComponent<ResBase>();                // 取得ResBase組件
                if (resBase == null) return null;

                resBase.resName = pathName;                 // Clone取得ResBase組件後, 就開始指定resPath作為resName
                resBase.InitThis();                         // Clone取得ResBase組件後, 也初始ResBase相關設定
                resBase.InitFirst();                        // Clone取得ResBase組件後, 也初始ResBase相關綁定組件設定   

                // >>> 需在InitThis之後, 以下設定開始生效 <<<

                this._SetParent(resBase);                   // 透過ResNodeType類型, 設置Parent

                resBase.gameObject.SetActive(false);        // 最後設置完畢後, 關閉GameObject的Active為false

                this._dictAllCached.Add(pathName, resBase); // 最後加入所有快取

                return resBase;
            }
            #endregion

            #region 相關校正與設置
            /// <summary>
            /// 依照對應的Node類型設置母節點
            /// </summary>
            /// <param name="resBase"></param>
            protected override void _SetParent(ResBase resBase)
            {
                switch (resBase.resType.resNodeType)
                {
                    case ResNodeType.Normal:
                        resBase.gameObject.transform.SetParent(this._goNormal.transform);
                        break;

                    case ResNodeType.Permanent:
                        resBase.gameObject.transform.SetParent(this._goPermanent.transform);
                        break;
                }
            }
            #endregion

            #region Show
            /// <summary>
            /// 開啟Res
            /// </summary>
            /// <param name="resName"></param>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override async UniTask<ResBase> Show(string resName, object obj = null)
            {
                if (resName.IsNullOrZeroEmpty()) return null;

                if (this.CheckIsShowing(resName))
                {
                    Debug.LogWarning(string.Format("【Res】{0}已經存在了!!!", resName));
                    return null;
                }

                ResBase resBase = await this._LoadIntoAllCahce(resName);
                if (resBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【Res】: {0}", resName));
                    return null;
                }

                switch (resBase.resType.resNodeType)
                {
                    case ResNodeType.Normal:
                        await this._LoadResToNormalCache(resName, obj);
                        break;

                    case ResNodeType.Permanent:
                        await this._LoadResToPermanentCache(resName, obj);
                        break;
                }

                Debug.Log(string.Format("加入UI: 【{0}】", resName));

                return resBase;
            }
            #endregion

            #region Close
            /// <summary>
            /// 關閉Res
            /// </summary>
            /// <param name="resName"></param>
            /// <param name="withDestroy"></param>
            public override void Close(string resName, bool disableDoSub = false, bool withDestroy = false)
            {
                if (resName.IsNullOrZeroEmpty() || !this.HasInAllCached(resName)) return;

                ResBase resBase = this._GetFromAllCached(resName);
                if (resBase == null) return;

                switch (resBase.resType.resNodeType)
                {
                    case ResNodeType.Normal:
                        this._ExitNormalRes(resName, disableDoSub);
                        break;

                    case ResNodeType.Permanent:
                        this._ExitPermanentRes(resName, disableDoSub);
                        break;
                }

                if (withDestroy) this._Destroy(resBase, resName);
                else if (resBase.isCloseAndDestroy) this._Destroy(resBase, resName);

                Debug.Log(string.Format("關閉Res: 【{0}】", resName));
            }

            /// <summary>
            /// 關閉所有Res (自動排除Permanent類型)
            /// </summary>
            /// <param name="withDestroy">是否Destroy</param>
            /// <param name="withoutResNames">欲排除執行的ResNames</param>
            /// <returns></returns>
            public override async UniTask CloseAll(bool disableDoSub = false, bool withDestroy = false, params string[] withoutResNames)
            {
                // 需要注意快取需要temp出來, 因為如果for迴圈裡有功能直接對快取進行操作會出錯
                foreach (ResBase resBase in this._dictAllCached.Values.ToList())
                {
                    string resName = resBase.resName;
                    if (resBase == null) continue;

                    // 檢查排除執行的Res
                    bool checkWithout = false;
                    if (withoutResNames.Length > 0)
                    {
                        for (int i = 0; i < withoutResNames.Length; i++)
                        {
                            if (resName == withoutResNames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的Res直接掠過處理
                    if (checkWithout) continue;

                    // 如果沒有強制Destroy就判斷有開啟的Res才列入刪除
                    if (!withDestroy && !this.CheckIsShowing(resBase)) continue;

                    // Permanent類型不列入刪除執行
                    switch (resBase.resType.resNodeType)
                    {
                        case ResNodeType.Normal:
                            this._ExitNormalRes(resName, disableDoSub);
                            break;
                    }

                    // Permanent類型不列入刪除執行
                    switch (resBase.resType.resNodeType)
                    {
                        case ResNodeType.Normal:
                            if (withDestroy) this._Destroy(resBase, resName);
                            else if (resBase.isCloseAndDestroy) this._Destroy(resBase, resName);
                            break;
                    }
                }

                // 等待執行完畢
                await UniTask.Yield();
            }
            #endregion

            #region 顯示資源, 並載入資源至對應的Cache
            private async UniTask _LoadResToNormalCache(string resName, object obj)
            {
                if (!this.HasInAllCached(resName)) return;

                ResBase resBase = this._GetFromAllCached(resName);
                if (resBase == null) return;
                await resBase.PreInit();
                this._dictNormalTypes.Add(resName, resBase);
                resBase.Display(obj);
            }

            private async UniTask _LoadResToPermanentCache(string resName, object obj)
            {
                if (!this.HasInAllCached(resName)) return;

                ResBase resBase = this._GetFromAllCached(resName);
                if (resBase == null) return;
                await resBase.PreInit();
                this._dictPermanentTypes.Add(resName, resBase);
                resBase.Display(obj);
            }
            #endregion

            #region 關閉資源, 並從暫存Cache中移除
            private void _ExitNormalRes(string resName, bool disableDoSub)
            {
                if (!this.HasInAllCached(resName)) return;

                ResBase resBase = this._GetFromAllCached(resName);
                if (resBase == null) return;
                resBase.Hide(disableDoSub);
                this._dictNormalTypes.Remove(resName);
            }

            private void _ExitPermanentRes(string resName, bool disableDoSub)
            {
                if (!this.HasInAllCached(resName)) return;

                ResBase resBase = this._GetFromAllCached(resName);
                if (resBase == null) return;
                resBase.Hide(disableDoSub);
                this._dictPermanentTypes.Remove(resName);
            }
            #endregion
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CoreFrame.Exts;

namespace CoreFrame
{
    namespace SceneFrame
    {
        public class SceneManager : FrameManager<SceneBase>
        {
            private GameObject _goSceneRoot = null;  // SceneRoot節點      
            private GameObject _goNormal = null;     // Normal物件節點, 會將對應的SceneNodeType加入此節點
            private GameObject _goPermanent = null;  // Permanent物件節點, 會將對應的SceneNodeType加入此節點

            private Dictionary<string, SceneBase> _dictNormalTypes = new Dictionary<string, SceneBase>();       // 一般的資源【載入此Cache的類型: SceneNodeType.Normal】
            private Dictionary<string, SceneBase> _dictPermanentTypes = new Dictionary<string, SceneBase>();    // 常駐的資源【載入此Cache的類型: SceneNodeType.Permanent】

            private static SceneManager _instance = null;
            public static SceneManager GetInstance()
            {
                if (_instance == null)
                {
                    if (GameObject.Find(SceneSysDefine.SCENE_ROOT_NAME) != null) _instance = GameObject.Find(SceneSysDefine.SCENE_ROOT_NAME).AddComponent<SceneManager>();
                    else _instance = new GameObject(SceneSysDefine.SCENE_ROOT_NAME).AddComponent<SceneManager>();
                }
                return _instance;
            }

            private void Awake()
            {
                this._goSceneRoot = GameObject.Find(SceneSysDefine.SCENE_ROOT_NAME);
                if (this._goSceneRoot == null) return;

                // 設置SceneNode
                this._goNormal = this._CreateSceneNode(SceneSysDefine.SCENE_NORMAL_NODE);
                this._goPermanent = this._CreateSceneNode(SceneSysDefine.SCENE_PERMANENT_NODE);
            }

            #region 初始建立Node相關方法
            private GameObject _CreateSceneNode(string scNodeName)
            {
                // 檢查是否已經有先被創立了
                GameObject scNodeChecker = this._goSceneRoot.transform.Find(scNodeName)?.gameObject;
                if (scNodeChecker != null) return scNodeChecker;

                GameObject scNodeGo = new GameObject(scNodeName);
                // 設置sceneNode為sceneRoot的子節點
                scNodeGo.transform.SetParent(this._goSceneRoot.transform);

                // 校正Transform
                scNodeGo.transform.localScale = Vector3.one;
                scNodeGo.transform.localPosition = Vector3.zero;

                return scNodeGo;
            }
            #endregion

            #region 實作Loading
            protected override async UniTask<SceneBase> _LoadingFrameBase(string pathName)
            {
                GameObject go = await this._LoadGameObject(pathName);
                if (go == null) return null;

                GameObject instPref = Instantiate(go, this._goSceneRoot.transform);  // instantiate 【Scene Prefab】 (先指定Instantiate Parent為SceneRoot)
                instPref.name = instPref.name.Replace("(Clone)", "");                // Replace Name
                SceneBase scBase = instPref.GetComponent<SceneBase>();               // 取得SceneBase組件
                if (scBase == null) return null;

                scBase.scName = pathName;                  // Clone取得SceneBase組件後, 就開始指定pathName作為scName
                scBase.InitThis();                         // Clone取得SceneBase組件後, 也初始SceneBase相關設定
                scBase.InitFirst();                        // Clone取得SceneBase組件後, 也初始SceneBase相關綁定組件設定

                // >>> 需在InitThis之後, 以下設定開始生效 <<<

                this._SetParent(scBase);                   // 透過SceneNodeType類型, 設置Parent

                scBase.gameObject.SetActive(false);        // 最後設置完畢後, 關閉GameObject的Active為false

                this._dictAllCached.Add(pathName, scBase); // 最後加入所有快取

                return scBase;
            }
            #endregion

            #region 相關校正與設置
            /// <summary>
            /// 依照對應的Node類型設置母節點
            /// </summary>
            /// <param name="scBase"></param>
            protected override void _SetParent(SceneBase scBase)
            {
                switch (scBase.sceneType.sceneNodeType)
                {
                    case SceneNodeType.Normal:
                        scBase.gameObject.transform.SetParent(this._goNormal.transform);
                        break;

                    case SceneNodeType.Permanent:
                        scBase.gameObject.transform.SetParent(this._goPermanent.transform);
                        break;
                }
            }
            #endregion

            #region Show
            /// <summary>
            /// 顯示Scene
            /// </summary>
            /// <param name="scName"></param>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override async UniTask<SceneBase> Show(string scName, object obj = null)
            {
                if (scName.IsNullOrZeroEmpty()) return null;

                if (this.CheckIsShowing(scName))
                {
                    Debug.LogWarning(string.Format("【Scene】{0}已經存在了!!!", scName));
                    return null;
                }

                SceneBase scBase = await this._LoadIntoAllCahce(scName);
                if (scBase == null)
                {
                    Debug.LogWarning(string.Format("找不到相對路徑資源【Scene】: {0}", scName));
                    return null;
                }

                switch (scBase.sceneType.sceneNodeType)
                {
                    case SceneNodeType.Normal:
                        await this._LoadSceneToNormalCache(scName, obj);
                        break;

                    case SceneNodeType.Permanent:
                        await this._LoadSceneToPermanentCache(scName, obj);
                        break;
                }

                Debug.Log(string.Format("顯示Scene: 【{0}】", scName));

                return scBase;
            }
            #endregion

            #region Close
            /// <summary>
            /// 關閉Scene
            /// </summary>
            /// <param name="scName"></param>
            /// <param name="withDestroy"></param>
            public override void Close(string scName, bool disableDoSub = false, bool withDestroy = false)
            {
                if (scName.IsNullOrZeroEmpty() || !this.HasInAllCached(scName)) return;

                SceneBase scBase = this._GetFromAllCached(scName);
                if (scBase == null) return;

                switch (scBase.sceneType.sceneNodeType)
                {
                    case SceneNodeType.Normal:
                        this._ExitNormalScene(scName, disableDoSub);
                        break;

                    case SceneNodeType.Permanent:
                        this._ExitPermanentScene(scName, disableDoSub);
                        break;
                }

                if (withDestroy) this._Destroy(scBase, scName);
                else if (scBase.isCloseAndDestroy) this._Destroy(scBase, scName);

                Debug.Log(string.Format("關閉Scene: 【{0}】", scName));
            }

            /// <summary>
            ///  關閉所有Scene (自動排除Permanent類型)
            /// </summary>
            /// <param name="withDestroy">是否Destroy</param>
            /// <param name="withoutScNames">欲排除執行的ScNames</param>
            /// <returns></returns>
            public override async UniTask CloseAll(bool disableDoSub = false, bool withDestroy = false, params string[] withoutScNames)
            {
                // 需要注意快取需要temp出來, 因為如果for迴圈裡有功能直接對快取進行操作會出錯
                foreach (SceneBase scBase in this._dictAllCached.Values.ToList())
                {
                    string scName = scBase.scName;
                    if (scBase == null) continue;

                    // 檢查排除執行的Scene
                    bool checkWithout = false;
                    if (withoutScNames.Length > 0)
                    {
                        for (int i = 0; i < withoutScNames.Length; i++)
                        {
                            if (scName == withoutScNames[i]) checkWithout = true;
                        }
                    }

                    // 排除在外的Scene直接掠過處理
                    if (checkWithout) continue;

                    // 如果沒有強制Destroy就判斷有開啟的Scene才列入刪除
                    if (!withDestroy) if (!this.CheckIsShowing(scBase)) continue;

                    // Permanent類型不列入刪除執行
                    switch (scBase.sceneType.sceneNodeType)
                    {
                        case SceneNodeType.Normal:
                            this._ExitNormalScene(scName, disableDoSub);
                            break;
                    }

                    // Permanent類型不列入刪除執行
                    switch (scBase.sceneType.sceneNodeType)
                    {
                        case SceneNodeType.Normal:
                            if (withDestroy) this._Destroy(scBase, scName);
                            else if (scBase.isCloseAndDestroy) this._Destroy(scBase, scName);
                            break;
                    }
                }

                // 等待執行完畢
                await UniTask.Yield();
            }
            #endregion

            #region 顯示場景, 並載入場景至暫存Cache
            private async UniTask _LoadSceneToNormalCache(string scName, object obj)
            {
                if (!this.HasInAllCached(scName)) return;

                SceneBase scBase = this._GetFromAllCached(scName);
                if (scBase == null) return;
                await scBase.PreInit();
                this._dictNormalTypes.Add(scName, scBase);
                scBase.Display(obj);
            }

            private async UniTask _LoadSceneToPermanentCache(string scName, object obj)
            {
                if (!this.HasInAllCached(scName)) return;

                SceneBase scBase = this._GetFromAllCached(scName);
                if (scBase == null) return;
                await scBase.PreInit();
                this._dictPermanentTypes.Add(scName, scBase);
                scBase.Display(obj);
            }
            #endregion

            #region 關閉場景, 並從暫存Cache中移除
            private void _ExitNormalScene(string scName, bool disableDoSub)
            {
                if (!this.HasInAllCached(scName)) return;

                SceneBase scBase = this._GetFromAllCached(scName);
                if (scBase == null) return;
                scBase.Hide(disableDoSub);
                this._dictNormalTypes.Remove(scName);
            }

            private void _ExitPermanentScene(string scName, bool disableDoSub)
            {
                if (!this.HasInAllCached(scName)) return;

                SceneBase scBase = this._GetFromAllCached(scName);
                if (scBase == null) return;
                scBase.Hide(disableDoSub);
                this._dictPermanentTypes.Remove(scName);
            }
            #endregion
        }
    }
}
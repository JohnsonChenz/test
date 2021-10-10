using CoreFrame.Exts;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoreFrame
{
    public abstract class FrameManager<T> : MonoBehaviour where T : FrameBase
    {
        protected Dictionary<string, T> _dictAllCached = new Dictionary<string, T>();           // 【常駐】所有快取 (只會在Destroy時, Remove對應的快取)
        protected Dictionary<string, bool> _dictLoadingFlags = new Dictionary<string, bool>();  // 用來標記正在加載中的資源 (暫存快取)

        /// <summary>
        /// 判斷是否有在AllCached中
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public bool HasInAllCached(string pathName)
        {
            if (pathName.IsNullOrZeroEmpty()) return false;
            return this._dictAllCached.ContainsKey(pathName);
        }

        /// <summary>
        /// 判斷是否有在LoadingFlags中
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        protected bool _HasInLoadingFlags(string pathName)
        {
            if (pathName.IsNullOrZeroEmpty()) return false;
            return this._dictLoadingFlags.ContainsKey(pathName);
        }

        /// <summary>
        /// 從AllCached中取得FrameBase as T
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        protected T _GetFromAllCached(string pathName)
        {
            if (pathName.IsNullOrZeroEmpty()) return null;

            T fBase = null;
            if (this.HasInAllCached(pathName)) this._dictAllCached.TryGetValue(pathName, out fBase);
            return fBase;
        }

        /// <summary>
        /// 取得轉型的FrameBase
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public U GetFrameComponent<U>(string pathName) where U : T
        {
            return (U)this._GetFromAllCached(pathName);
        }

        /// <summary>
        /// 檢查是否開啟
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public bool CheckIsShowing(string pathName)
        {
            T fBase = this._GetFromAllCached(pathName);
            if (fBase == null) return false;
            return fBase.gameObject.activeSelf;
        }

        /// <summary>
        /// 檢查是否開啟
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public bool CheckIsShowing(T fBase)
        {
            if (fBase == null) return false;
            return fBase.gameObject.activeSelf;
        }

        /// <summary>
        /// 載入GameObject
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> _LoadGameObject(string pathName)
        {
            if (pathName.IsNullOrZeroEmpty()) return null;

            GameObject obj = await CachedResource.Load<GameObject>(pathName, false);
            if (obj == null)
            {
                Debug.LogWarning(string.Format("【 {0} 】此路徑找不到所屬資源!!!"));
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 取得載入的GameObject後, 並且取得FrameBase組件開始處理
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        protected abstract UniTask<T> _LoadingFrameBase(string pathName);

        /// <summary>
        /// 設置FrameBase至AllCached中 (執行相關一系列的加載後, 最後是由LoadingFrameBase設置快取)
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        protected async UniTask<T> _LoadIntoAllCahce(string pathName)
        {
            T fBase;
            // 判斷不在AllCached中, 也不在LoadingFlags中, 才進行加載程序
            if (!this.HasInAllCached(pathName) && !this._HasInLoadingFlags(pathName))
            {
                this._dictLoadingFlags.Add(pathName, true);        // 標記LoadingFlag = true
                fBase = await this._LoadingFrameBase(pathName);    // 開始加載
                this._dictLoadingFlags[pathName] = false;          // 標記LoadingFlag = false
                this._dictLoadingFlags.Remove(pathName);           // 移除LoadingFlag
            }
            else fBase = this._GetFromAllCached(pathName);         // 如果判斷沒有要執行加載程序, 就直接從AllCached中取得

            return fBase;
        }

        /// <summary>
        /// 預加載
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public async UniTask Preload(string pathName)
        {
            if (!pathName.IsNullOrZeroEmpty()) await this._LoadIntoAllCahce(pathName);

            // 等待執行完畢
            await UniTask.Yield();
        }

        /// <summary>
        /// 預加載
        /// </summary>
        /// <param name="pathNames"></param>
        /// <returns></returns>
        public async UniTask Preload(string[] pathNames)
        {
            if (pathNames.Length > 0)
            {
                for (int i = 0; i < pathNames.Length; i++)
                {
                    if (pathNames[i].IsNullOrZeroEmpty()) continue;
                    await this._LoadIntoAllCahce(pathNames[i]);
                }
            }

            // 等待執行完畢
            await UniTask.Yield();
        }

        /// <summary>
        /// 依照對應的Node類型設置母節點 (子類實作)
        /// </summary>
        /// <param name="fBase"></param>
        protected virtual void _SetParent(T fBase) { }

        #region Show
        public abstract UniTask<T> Show(string pathName, object obj = null);
        #endregion

        #region Close
        public abstract void Close(string pathName, bool disableDoSub = false, bool withDestroy = false);
        public abstract UniTask CloseAll(bool disableDoSub = false, bool withDestroy = false, params string[] withoutNames);
        #endregion

        #region Destroy
        /// <summary>
        /// 刪除FrameBase
        /// </summary>
        /// <param name="fBase"></param>
        /// <param name="pathName"></param>
        protected virtual void _Destroy(T fBase, string pathName)
        {
            fBase.OnRelease();                      // 執行FrameBase相關釋放程序
            Destroy(fBase.gameObject);              // 刪除FrameBase資源
            this._dictAllCached.Remove(pathName);   // 移除FrameBase快取

            Debug.Log(string.Format("Destroy Frame: {0}", pathName));
        }
        #endregion

        /// <summary>
        /// 【特殊方法】交由Protocol委託Handle (主要用於Server傳送封包給Client後, 進行一次性刷新)
        /// </summary>
        /// <param name="funcId"></param>
        public void UpdateByProtocol(int funcId)
        {
            foreach (T fBase in this._dictAllCached.Values.ToList())
            {
                if (fBase == null) continue;

                if (this.CheckIsShowing(fBase))
                {
                    fBase.OnUpdateOnceAfterProtocol(funcId);
                }
            }
        }
    }
}

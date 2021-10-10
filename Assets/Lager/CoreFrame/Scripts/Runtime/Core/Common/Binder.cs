using UnityEngine;

namespace CoreFrame
{

    public class Binder
    {

        private static Binder _instance = null;
        public static Binder GetIncetance()
        {
            if (_instance == null) _instance = new Binder();
            return _instance;
        }

        /// <summary>
        /// 由FrameBase調用綁定
        /// </summary>
        /// <param name="fBase"></param>
        public void BindComponent(FrameBase fBase)
        {
            this._BindNode(fBase.gameObject, fBase);
        }

        /// <summary>
        /// 開始進行步驟流程綁定
        /// </summary>
        /// <param name="go"></param>
        /// <param name="fBase"></param>
        private void _BindNode(GameObject go, FrameBase fBase)
        {
            if (fBase.collector.name == go.name)
            {
                Debug.LogWarning("重複綁定: " + go.name);
                return;
            }

            fBase.collector.name = go.name;
            this._BindSubNode(go, fBase);
        }

        /// <summary>
        /// 將會交由判斷後, 重複進行綁定子節點
        /// </summary>
        /// <param name="go"></param>
        /// <param name="fBase"></param>
        private void _BindSubNode(GameObject go, FrameBase fBase)
        {
            string name = go.name;
            // 檢查是否要結束綁定, 有檢查到【BIND_END】時, 則停止繼續搜尋綁定物件
            if (this._CheckIsToBindChildren(name))
            {
                // 這邊檢查有【BIND_PREFIX】時, 則進入判斷
                if (this._CheckNodeHasPrefix(name))
                {
                    // 之後再透過【BIND_SEPARATOR】去切開取得字串陣列
                    string[] names = this._GetPrefixSplitNameBySeparator(name);

                    string bindType = names[0]; // 綁定類型(會去查找dictComponentFinder裡面有沒有符合的類型)
                    string bindKey = names[1];  // 要成為取得綁定物件後的Key

                    // 再去判斷取得後的字串陣列是否綁定格式資格
                    if (names == null || names.Length < 2 || !FrameSysDefine.dictComponentFinder.ContainsKey(bindType))
                    {
                        Debug.Log(name + "命令不規範, 請使用_xxx@xxx的格式!");
                        return;
                    }

                    // 找到對應的綁定類型後, 進行綁定
                    if (FrameSysDefine.dictComponentFinder[bindType] == "GameObject")
                    {
                        // 綁定至FrameBase中對應的容器, 此時進行完成綁定
                        fBase.collector.AddNode(bindKey, go);
                    }
                }

                // 依序綁定下一個子物件(無限循環找到符合資格與結束為主)
                foreach (Transform cTs in go.GetComponentInChildren<Transform>())
                {
                    this._BindSubNode(cTs.gameObject, fBase);
                }
            }
        }

        /// <summary>
        /// 步驟1. 會先執行檢查是否要進行子節點綁定(如果不進行則在最後面+上【BIND_END】後綴字, 就會停止執行以下步驟)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool _CheckIsToBindChildren(string name)
        {
            if (name.Substring(name.Length - 1) != FrameSysDefine.BIND_END.ToString()) return true;
            return false;
        }

        /// <summary>
        /// 步驟2. 檢查是否有+【BIND_PREFIX】前綴字(表示想要進行綁定)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool _CheckNodeHasPrefix(string name)
        {
            if (name.Substring(0, 1) == FrameSysDefine.BIND_PREFIX.ToString()) return true;
            return false;
        }

        /// <summary>
        /// 步驟3. 透過【BIND_SEPARATOR】去Split字串, 返回取得字串陣列. 
        /// ※備註: (Example) _Node@MyObj => ["_Node", "MyObj"], 之後可以透過取得的陣列去查找表看是否有對應的組件需要綁定
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string[] _GetPrefixSplitNameBySeparator(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return name.Split(FrameSysDefine.BIND_SEPARATOR);
        }
    }

}
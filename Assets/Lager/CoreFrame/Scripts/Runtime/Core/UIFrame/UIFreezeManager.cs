using CoreFrame.Exts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    namespace UIFrame
    {
        public class UIFreezeManager
        {
            #region FreezeNodePool, FreezeUI物件池
            class FreezeNodePool
            {
                private static FreezeNodePool _instance = null;
                public static FreezeNodePool GetInstance()
                {
                    if (_instance == null) _instance = new FreezeNodePool();
                    return _instance;
                }

                private List<UIFreeze> _uiFreezePool = new List<UIFreeze>();    // uiFreeze物件池
                private int _initNum = 5;                                       // 物件池初始數量

                private void _Init()
                {
                    for (int i = 0; i < this._initNum; i++)
                    {
                        UIFreeze uiFreeze = new GameObject(UIManager.GetInstance().uiFreezeManager.nodeName).AddComponent<UIFreeze>();
                        uiFreeze.gameObject.SetActive(false);
                        uiFreeze.gameObject.layer = LayerMask.NameToLayer("UI");
                        uiFreeze.gameObject.transform.SetParent(UIManager.GetInstance().uiFreezeManager.uiFreezeTrans);
                        uiFreeze.gameObject.transform.localPosition = Vector3.zero;
                        uiFreeze.gameObject.transform.localScale = Vector3.one;
                        uiFreeze.InitFreeze();
                        this._uiFreezePool.Add(uiFreeze);
                    }
                }

                public UIFreeze GetUIFreeze(Transform parent)
                {
                    if (this._uiFreezePool.Count <= 0) this._Init();

                    UIFreeze uiFreeze = this._uiFreezePool.Pop();

                    uiFreeze.ReUse();
                    uiFreeze.gameObject.transform.SetParent(parent);
                    uiFreeze.gameObject.transform.SetAsLastSibling();
                    uiFreeze.SetLocalScale(Vector3.one);

                    return uiFreeze;
                }

                public bool RecycleUIFreeze(Transform parent)
                {
                    if (parent.Find(UIManager.GetInstance().uiFreezeManager.nodeName) == null) return false;

                    GameObject uiFreezeNode = parent.Find(UIManager.GetInstance().uiFreezeManager.nodeName).gameObject;
                    if (uiFreezeNode == null || !uiFreezeNode.GetComponent<UIFreeze>())
                    {
                        Debug.Log("未找到對應的UIFreezeNode, 不需回收!");
                        return false;
                    }

                    uiFreezeNode.transform.SetParent(UIManager.GetInstance().uiFreezeManager.uiFreezeTrans);
                    UIFreeze uiFreeze = uiFreezeNode.GetComponent<UIFreeze>();
                    uiFreeze.UnUse();
                    this._uiFreezePool.Add(uiFreeze);
                    return true;
                }

                public void ClearFreezePool()
                {
                    foreach (UIFreeze uiFreeze in this._uiFreezePool)
                    {
                        uiFreeze.UnUse();
                    }
                    this._uiFreezePool.Clear();
                }
            }
            #endregion

            #region UIFreezeManager, UIFreeze相關控制
            [HideInInspector] public string nodeName = "UIFreezeNode";
            [HideInInspector] public Transform uiFreezeTrans = null;

            public void Init(Transform uiFreezeTrans)
            {
                this.uiFreezeTrans = uiFreezeTrans;
            }

            public void AddFreeze(Transform parent)
            {
                if (parent.Find(this.nodeName) || !parent.GetComponent<UIBase>()) return;

                FreezeNodePool.GetInstance().GetUIFreeze(parent);
            }

            public void RemoveFreeze(Transform parent)
            {
                FreezeNodePool.GetInstance().RecycleUIFreeze(parent);
            }
            #endregion
        }
    }
}
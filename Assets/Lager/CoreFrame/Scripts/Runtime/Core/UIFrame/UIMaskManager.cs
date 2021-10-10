using CoreFrame.Exts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    namespace UIFrame
    {
        public class UIMaskManager
        {
            #region MaskNodePool, MaskUI物件池
            class MaskNodePool
            {
                private static MaskNodePool _instance = null;
                public static MaskNodePool GetInstance()
                {
                    if (_instance == null) _instance = new MaskNodePool();
                    return _instance;
                }

                private List<UIMask> _uiMaskPool = new List<UIMask>();  // uiMask物件池
                private int _initNum = 5;                               // 物件池初始數量

                private void _Init(Sprite maskSprite)
                {
                    for (int i = 0; i < this._initNum; i++)
                    {
                        UIMask uiMask = new GameObject(UIManager.GetInstance().uiMaskManager.nodeName).AddComponent<UIMask>();
                        uiMask.gameObject.SetActive(false);
                        uiMask.gameObject.layer = LayerMask.NameToLayer("UI");
                        uiMask.gameObject.transform.SetParent(UIManager.GetInstance().uiMaskManager.uiMaskTrans);
                        uiMask.gameObject.transform.localPosition = Vector3.zero;
                        uiMask.gameObject.transform.localScale = Vector3.one;
                        uiMask.InitMask(maskSprite);
                        this._uiMaskPool.Add(uiMask);
                    }
                }

                public UIMask GetUIMask(Transform parent, Sprite maskSprite)
                {
                    if (this._uiMaskPool.Count <= 0) this._Init(maskSprite);

                    UIMask uiMask = this._uiMaskPool.Pop();

                    uiMask.ReUse(parent.GetComponent<UIBase>().uiName);
                    uiMask.gameObject.transform.SetParent(parent);
                    uiMask.gameObject.transform.SetAsFirstSibling();
                    uiMask.SetLocalScale(Vector3.one);

                    return uiMask;
                }

                public bool RecycleUIMask(Transform parent)
                {

                    if (parent.Find(UIManager.GetInstance().uiMaskManager.nodeName) == null) return false;

                    GameObject uiMaskNode = parent.Find("UIMaskNode").gameObject;
                    if (uiMaskNode == null || !uiMaskNode.GetComponent<UIMask>())
                    {
                        Debug.Log("未找到對應的UIMaskNode, 不需回收!");
                        return false;
                    }

                    uiMaskNode.transform.SetParent(UIManager.GetInstance().uiMaskManager.uiMaskTrans);
                    UIMask uiMask = uiMaskNode.GetComponent<UIMask>();
                    uiMask.UnUse();
                    this._uiMaskPool.Add(uiMask);
                    return true;
                }

                public void ClearMaskPool()
                {
                    foreach (UIMask uiMask in this._uiMaskPool)
                    {
                        uiMask.UnUse();
                    }
                    this._uiMaskPool.Clear();
                }
            }
            #endregion

            #region UIMaskManager, UIMask相關控制
            [HideInInspector] public string nodeName = "UIMaskNode";
            [HideInInspector] public Transform uiMaskTrans = null;
            private UIMask _uiMask = null;
            private Sprite _maskSprite = null;

            public void Init(Transform uiMaskTrans)
            {
                this.uiMaskTrans = uiMaskTrans;
            }

            public void AddMask(Transform parent, MaskEventFunc maskClickEvent = null)
            {
                this._maskSprite = this._MakeTexture2dSprite();
                if (parent.Find(this.nodeName) || !parent.GetComponent<UIBase>()) return;

                this._uiMask = MaskNodePool.GetInstance().GetUIMask(parent, this._maskSprite);
                if (maskClickEvent != null) this._uiMask.SetMaskClickEvent(maskClickEvent);
            }

            public void SetMaskAlpha(UIMaskOpacity opacityType)
            {
                this._uiMask.SetMaskAlpha(opacityType);
            }

            public void RemoveMask(Transform parent)
            {
                MaskNodePool.GetInstance().RecycleUIMask(parent);
            }

            private Sprite _MakeTexture2dSprite()
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, true);
                texture.SetPixel(0, 0, Color.black);
                texture.Apply();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                return sprite;
            }
            #endregion
        }
    }
}
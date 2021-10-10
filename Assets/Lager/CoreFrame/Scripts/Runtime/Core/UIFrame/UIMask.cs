using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreFrame
{
    namespace UIFrame
    {
        public class UIMask : MonoBehaviour
        {
            [HideInInspector] public string uiName = "";
            private MaskEventFunc _maskClickEvent = null;
            private Image _maskImage = null;
            public void InitMask(Sprite maskSprite)
            {
                // 建立一個Image當作遮罩 & MaskButton Raycast
                this._maskImage = this.gameObject.AddComponent<Image>();
                this._maskImage.sprite = maskSprite;
                this._maskImage.rectTransform.sizeDelta = new Vector2(UISysDefine.RECT_SIZE, UISysDefine.RECT_SIZE);

                // 建立Mask按鈕事件
                Button maskBtn = this.gameObject.AddComponent<Button>();
                maskBtn.transition = Selectable.Transition.None;
                maskBtn.onClick.AddListener(() =>
                {
                    this._maskClickEvent?.Invoke();
                });
            }

            /// <summary>
            /// 重新使用Mask時, 再指定一次uiName
            /// </summary>
            /// <param name="uiName"></param>
            public void ReUse(string uiName)
            {
                this.uiName = uiName;
                this.gameObject.SetActive(true);
            }

            /// <summary>
            /// 回收至MaskPool時, 會需要釋放相關參數
            /// </summary>
            public void UnUse()
            {
                this.uiName = "";
                this._maskClickEvent = null;
                this._maskImage.color = new Color(0, 0, 0, 0);
                this.gameObject.SetActive(false);
            }

            /// <summary>
            /// 交由UIMaskManager調用顯示Mask Alpha
            /// </summary>
            /// <param name="opacityType"></param>
            public void SetMaskAlpha(UIMaskOpacity opacityType)
            {

                byte o = 0;
                switch (opacityType)
                {
                    case UIMaskOpacity.None:
                        this.gameObject.SetActive(false);
                        break;

                    case UIMaskOpacity.OpacityZero:
                        o = 0;
                        break;

                    case UIMaskOpacity.OpacityLow:
                        o = 64;
                        break;

                    case UIMaskOpacity.OpacityHalf:
                        o = 128;
                        break;

                    case UIMaskOpacity.OpacityHigh:
                        o = 192;
                        break;

                    case UIMaskOpacity.OpacityFull:
                        o = 255;
                        break;
                }
                this._maskImage.color = new Color32(0, 0, 0, o);
            }

            public void SetLocalScale(Vector3 scale)
            {
                this._maskImage.rectTransform.localScale = scale;
            }

            public void SetMaskClickEvent(MaskEventFunc maskEventFunc)
            {
                this._maskClickEvent = maskEventFunc;
            }
        }
    }
}
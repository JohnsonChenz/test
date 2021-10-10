using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CoreFrame
{
    namespace UIFrame
    {
        public class UIFreeze : MonoBehaviour
        {
            private Image _freezeImage = null;

            public void InitFreeze()
            {
                // 建立一個Image當作FreezeButton Raycast用 (BlockEvent)
                this._freezeImage = this.gameObject.AddComponent<Image>();
                this._freezeImage.rectTransform.sizeDelta = new Vector2(UISysDefine.RECT_SIZE, UISysDefine.RECT_SIZE);
                this._freezeImage.color = new Color(0, 0, 0, 0);

                // 建立Mask按鈕事件
                Button freezeBtn = this.gameObject.AddComponent<Button>();
                freezeBtn.transition = Selectable.Transition.None;
                freezeBtn.onClick.AddListener(() =>
                {
                    Debug.LogWarning("UI被凍結了");
                });
            }

            /**
            <summary>
            重新使用UIFreeze
            </summary>
             */
            public void ReUse()
            {
                this.gameObject.SetActive(true);
            }

            /**
            <summary>
            回收至FreezePool的相關釋放
            </summary>
             */
            public void UnUse()
            {
                this.gameObject.SetActive(false);
            }

            public void SetLocalScale(Vector3 scale)
            {
                this._freezeImage.rectTransform.localScale = scale;
            }
        }
    }
}
using CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using DialogueSys;
using KoganeUnityLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DialogueSys
{
    public abstract partial class DialogueBaseUI : UIBase
    {
        /// <summary>
        /// 講者顯示狀態
        /// </summary>
        public enum SpeakerImageStatus
        {
            None,
            Bright,
            Black
        }

        /// <summary>
        /// 講者顯示組件類別
        /// </summary>
        public class Speaker
        {
            public GameObject speakerRoot;                // 講者主物件
            public SpeakerImageStatus speakerImageStatus; // 講者顯示狀態
            public RawImage rawImage;                     // 講者顯示圖片
        }


        public override void InitThis()
        {
            this.uiType = new UIType(UIFormType.IndiePopup);
            this.maskType = new MaskType(UIMaskOpacity.OpacityHalf);
            this.isCloseAndDestroy = true;
        }

        protected override void InitOnceComponents()
        {
            this._InitComponents();
        }

        protected override void InitOnceEvents()
        {
            this._InitEvents();
        }

        protected override void ShowAnim(AnimEndCb animEndCb)
        {
            animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
        }

        protected override void HideAnim(AnimEndCb animEndCb)
        {
            animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
        }

        protected override void OnClose()
        {
            this.ClearUI();

            // 銷毀預製出來的物件池根節點物件
            Destroy(this.camPool);
            Destroy(this.camRoot);
        }

        protected override void MaskEvent()
        {
            // 此處只需Invoke，原因為此處調用的事件都是"執行下一對話"跟"結束對話"，執行下一對話時，Mask Event會自動被驅動器重新指定，而結束對話關閉UI時，會在ClearUI方法內釋放掉記憶體。
            this.maskEvent?.Invoke();
        }

        /// <summary>
        /// 對話選項按鈕 (自行拖拉)
        /// </summary>
        public GameObject dialogueEventBtn;

        // --- 對話UI基本顯示組件
        private Button typewriterMask;                // 打字機效果專用遮罩
        private TMP_Text text;                        // 對話文字內容
        private TMP_Text speakerName;                 // 當前講者名稱
        private ScrollRect eventOptionScrollView;     // 選擇按鈕滑動區域
        private Speaker[] speakers = new Speaker[3];  // 三位講者顯示

        // --- 打字機效果參數
        protected float typeWriterSpeed = 15;         // 打字機打字效果速度

        // --- Actions
        private Action maskEvent;                     // 對話Mask專用Action

        // --- 攝影棚相關組件
        private GameObject camPool;                   // 攝影棚物件池節點
        private GameObject camRoot;                   // 攝影棚物件生成節點

        /// <summary>
        /// 初始相關按鈕事件
        /// </summary>
        protected virtual void _InitEvents() { }

        /// <summary>
        /// 初始綁定UI組件
        /// </summary>
        protected virtual void _InitComponents()
        {
            // 初始設置攝影棚相關組件
            if (this.camPool == null) this.camPool = new GameObject("DialogueCamPool");
            if (this.camRoot == null) this.camRoot = new GameObject("DialogueCamRoot");
            this.camStudioPool.Init(this.camPool.transform);

            this.typewriterMask = this.collector.GetNode("TypewritterMask").GetComponent<Button>();
            this.text = this.collector.GetNode("Text").GetComponent<TMP_Text>();
            this.speakerName = this.collector.GetNode("SpeakerName").GetComponent<TMP_Text>();
            this.eventOptionScrollView = this.collector.GetNode("EventOptionScrollView").GetComponent<ScrollRect>();

            // 透過迴圈初始化並綁定講者資訊
            for (int i = 0; i < this.speakers.Length; i++)
            {
                this.speakers[i] = new Speaker();
                this.speakers[i].speakerRoot = this.collector.GetNode(string.Format("Root_{0}", i));
                this.speakers[i].rawImage = this.speakers[i].speakerRoot.GetComponentInChildren<RawImage>();
                this.speakers[i].speakerImageStatus = SpeakerImageStatus.None;
            }
        }

        #region 父類公開方法

        /// <summary>
        /// 設置講者顯示
        /// </summary>
        /// <param name="speakerDatas">所代入之講者資料</param>
        public async UniTaskVoid SetSpeakers(SpeakerData[] speakerDatas)
        {
            // 重置講者顯示
            this._ResetSpeakers();

            for (int i = 0; i < speakerDatas.Length; i++)
            {
                // 如果判斷沒有模型Id，就跳過此迴圈並不做顯示及加載
                if (speakerDatas[i].gid.Trim().IsNullOrZeroEmpty()) continue;

                // 代入模型Id，加載模型
                GameObject instModel = await this._LoadSpeakerModel(speakerDatas[i]);
                if (instModel == null) continue;

                // 如果模型資源載入成功，加載攝影棚
                this._SetSpeakerModel(instModel, this.speakers[i].rawImage);

                // 將場景上的講者物件打開
                this.speakers[i].speakerRoot.gameObject.SetActive(true);

                // 判斷講者顯示是否要褪黑
                if (speakerDatas[i].speakerImageTurnBlack)
                {
                    // 如果目前的圖片狀態不為褪黑(防止重複調用方法)
                    if (this.speakers[i].speakerImageStatus != SpeakerImageStatus.Black)
                    {
                        // 設置其狀態為褪黑，並調用子類實作狀態變更過渡效果
                        this.speakers[i].speakerImageStatus = SpeakerImageStatus.Black;
                        this.OnSpeakerImageStatusChanged(this.speakers[i], this.speakers[i].speakerImageStatus);
                    }
                }
                else
                {
                    // 如果目前的圖片狀態不為明亮(防止重複調用方法)
                    if (this.speakers[i].speakerImageStatus != SpeakerImageStatus.Bright)
                    {
                        // 設置其狀態為明亮，並調用子類實作狀態變更過渡效果
                        this.speakers[i].speakerImageStatus = SpeakerImageStatus.Bright;
                        this.OnSpeakerImageStatusChanged(this.speakers[i], this.speakers[i].speakerImageStatus);
                    }

                }

                // 最後，透過調整X Scale，設置講者顯示圖片的面向
                this.speakers[i].rawImage.transform.localScale = new Vector3((int)speakerDatas[i].speakerImageReverse, this.speakers[i].rawImage.transform.localScale.y, this.speakers[i].rawImage.transform.localScale.z);
            }
        }

        /// <summary>
        /// 設置對話內容文字顯示
        /// </summary>
        /// <param name="speakerName">講者名稱</param>
        /// <param name="content">說話內容</param>
        /// <param name="skip">跳過打字機打字效果，預設為false</param>
        public void SetTexts(string speakerName, string content, bool skip = false)
        {
            // 先重置對話顯示
            this._ResetTexts();

            // 設置講者名稱 (如果有特別處理額外講者名稱，就先代入額外講者名稱)
            this.speakerName.text = speakerName;

            // 取得打字機組件
            TMP_Typewriter tmpTypewriter = this.text.GetComponent<TMP_Typewriter>();

            // 如果尋找到打字機組件，設置打字機效果，如無，則直接設置文字
            if (tmpTypewriter != null) this.SetTypeWriter(tmpTypewriter, content, skip);
            else this.text.text = content;
        }

        /// <summary>
        /// 設置Mask按鈕事件
        /// </summary>
        /// <param name="action">按鈕事件Action</param>
        public void SetMaskEvent(Action action)
        {
            this.maskEvent = action;
        }

        /// <summary>
        /// 設置對話選擇事件按鈕
        /// </summary>
        /// <param name="listDialogueEventBtn">處理過後的按鈕物件List</param>
        public void SetEventOptions(List<Transform> listDialogueEventBtn)
        {
            this._ResetEventOption();

            // 將ViewPort打開
            this.eventOptionScrollView.viewport.gameObject.SetActive(true);

            // 將按鈕階層設置到選擇按鈕Content容器
            foreach (var dialogueEventBtn in listDialogueEventBtn)
            {
                dialogueEventBtn.SetParent(this.eventOptionScrollView.content);
            }
        }

        /// <summary>
        /// 清空所有UI顯示
        /// </summary>
        public void ClearUI()
        {
            this._ResetMaskEvent();
            this._ResetEventOption();
            this._ResetTexts();
            this._ResetSpeakers();
        }
        #endregion

        #region 開放子類覆寫實作方法
        /// <summary>
        /// 音效設置
        /// </summary>
        /// <param name="audioId">音效資源ID</param>
        public virtual void SetAudio(string audioId) { }

        /// <summary>
        /// 講者圖片顯示狀態變更時，由父類調用一次(防止動畫重複播放)，並由子類實作，用以設置相關動畫過渡效果。
        /// </summary>
        /// <param name="speaker">講者資料</param>
        /// <param name="speakerImageStatus">講者圖片顯示狀態</param>
        protected virtual void OnSpeakerImageStatusChanged(Speaker speaker, SpeakerImageStatus speakerImageStatus) { }

        /// <summary>
        /// 透過gid加載講者模型
        /// </summary>
        /// <param name="speakerData">代入的講者資料</param>
        /// <returns></returns>
        protected virtual async UniTask<GameObject> _LoadSpeakerModel(SpeakerData speakerData)
        {
            // 解析講者模型路徑
            string pathName = this._ParsingModelPathName(speakerData);

            GameObject instModel = null;
            try
            {
                instModel = Instantiate(Resources.Load<GameObject>(pathName));
            }
            catch
            {
                Debug.Log(string.Format("<color=#FF0000>模型載入出錯，請檢查該路徑資源是否存在!! Path : 【{0}】</color>", pathName));
            }

            if (instModel != null) return instModel;
            return null;
        }

        /// <summary>
        /// 透過講者模型gid解析出完整模型路徑，加載出講者模型
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        protected virtual string _ParsingModelPathName(SpeakerData speakerData)
        {
            return speakerData.gid;
        }

        /// <summary>
        /// 設置講者攝影棚
        /// </summary>
        /// <param name="model">模型</param>
        /// <param name="speakerImage">目標RawImage</param>
        protected virtual void _SetSpeakerModel(GameObject model, RawImage speakerImage)
        {
            CamStudio camStudio = this.CreateRtCamStudio(this.uiName, null, speakerImage, 9);
            model.transform.SetParent(camStudio.root);
            model.transform.localPosition = Vector3.zero;
            camStudio.SetCamFocus(model.transform);
        }
        #endregion

        #region 父類私有方法

        /// <summary>
        /// 重置講者顯示
        /// </summary>
        private void _ResetSpeakers()
        {
            // 釋放攝影棚資源
            this.LeaveRtCamStudio(this.uiName);

            // 跑迴圈，將三位講者顯示重置
            for (int i = 0; i < this.speakers.Length; i++)
            {
                this.speakers[i].speakerRoot.gameObject.SetActive(false);
                this.speakers[i].rawImage.color = new Color(1, 1, 1);
                this.speakers[i].rawImage.texture = null;
            }
        }

        /// <summary>
        /// 重置對話文字內容顯示
        /// </summary>
        private void _ResetTexts()
        {
            this.speakerName.text = null;
            this.text.text = null;
        }

        /// <summary>
        /// 重置選擇按鈕Content容器
        /// </summary>
        private void _ResetEventOption()
        {
            // 關閉ViewPort
            this.eventOptionScrollView.viewport.gameObject.SetActive(false);
            // 清空選擇按鈕Content容器
            this.eventOptionScrollView.content.DestroyAllChildren();
        }

        /// <summary>
        /// 重置基底Mask
        /// </summary>
        private void _ResetMaskEvent()
        {
            this.maskEvent = null;
        }

        /// <summary>
        /// 對話內容設置打字機效果
        /// </summary>
        /// <param name="tmpTypewriter">目標Tmp_Text身上之TMP_Typewrite組件</param>
        /// <param name="text">文字內容</param>
        /// <param name="skip">對話是否有被跳過</param>
        private void SetTypeWriter(TMP_Typewriter tmpTypewriter, string text, bool skip)
        {
            // 播放對話打字機效果，代入文字，速度，以及播放完成後的Callback
            tmpTypewriter.Play(text, this.typeWriterSpeed, () => { this.ResetTypewriterMask(); });

            // 如果對話目前處於被跳過狀態，打字機直接調用Skip結束表演
            if (skip)
            {
                tmpTypewriter.Skip();
            }
            // 設置打字機Mask (對話播放到一半，點擊一次畫面直接顯示所有對話之效果)
            else
            {
                this.SetTypewriterMask(() =>
                {
                    tmpTypewriter.Skip();
                }
                );
            }
        }

        /// <summary>
        /// 設置打字機Mask
        /// </summary>
        /// <param name="unityAction">代入的Action</param>
        private void SetTypewriterMask(UnityAction unityAction)
        {
            // 先重置打字機Mask
            this.ResetTypewriterMask();

            // 開啟Mask物件
            this.typewriterMask.gameObject.SetActive(true);

            // 設置打字機Mask點擊事件
            this.typewriterMask.onClick.AddListener(() =>
            {
                unityAction.Invoke();
            });
        }

        /// <summary>
        /// 重置打字機Mask
        /// </summary>
        private void ResetTypewriterMask()
        {
            this.typewriterMask.gameObject.SetActive(false);
            this.typewriterMask.onClick.RemoveAllListeners();
        }
        #endregion
    }
}
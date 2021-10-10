using CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSys
{
    public abstract class DialogueDriverBase
    {
        // --- 對話運行資訊參數
        protected int currentDialogueIndex; // 當前對話Index
        protected int nextDialogueIndex;    // 下一對話Index      

        /// <summary>
        /// 對話起始Index
        /// </summary>
        protected int dialogueStartIndex
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 當前對話群組最大可傳入Index
        /// </summary>
        protected int dialogueMaxIndex
        {
            get
            {
                return this._currentDialogueDatas.Count - 1;
            }
        }

        /// <summary>
        /// 當前對話群組總長度
        /// </summary>
        protected int dialogueCount
        {
            get
            {
                return this._currentDialogueDatas.Count;
            }
        }

        /// <summary>
        /// 當前對話對話選項數量
        /// </summary>
        protected int dialogueEventDataCount
        {
            get
            {
                return this._currentDialogueEventDatas.Count;
            }
        }

        // --- 對話狀態參數
        private bool isSkippingDialogue;                              // 是否正在跳過對話
        private bool isRunningDialogue;                               // 是否正在運行對話

        // --- 對話功能參數
        protected bool manualInvokeCb;                                // 是否開放手動調用Callback
        protected bool doNotForceChooseEvent;                         // 是否不強迫選擇對話事件

        // --- 對話資料參數
        protected string dialogueUIPath;                              // 對話UI路徑
        protected Action _endCb;                                      // 對話系統結束時Callback
        protected List<DialogueData> _currentDialogueDatas;           // 當前的對話資料List
        protected List<DialogueEventData> _currentDialogueEventDatas; // 對話事件選項資料List

        // --- 對話成員參數
        protected DialogueBaseUI _dialogueUI;                         // 對話UI
        protected DialogueSystemBase _dialogueSystem;                 // 對話系統組件

        public DialogueDriverBase()
        {
            // 初始對話相關狀態設置
            this.isRunningDialogue = false;
            this.isSkippingDialogue = false;
        }

        #region 父類公開方法
        /// <summary>
        /// 更新對話及相關顯示
        /// </summary>
        /// <param name="dialogueDataContainer">對話資料儲存器</param>
        /// <param name="endCb">對話EndCallback</param>
        /// <param name="endCb">對話結束所調用的Callback</param>
        /// <param name="manualInvokeCb">是否開啟手動調用Callback，預設為False</param>
        /// <param name="doNotForceChooseEvent">是否不強迫選擇對話事件，預設為False</param>
        /// <returns></returns>
        public async UniTaskVoid Init(object dialogueDataContainer, string dialogueUIPath, Action endCb, bool manualInvokeCb, bool doNotForceChooseEvent)
        {
            // 如果UI路徑為空就不執行
            if (dialogueUIPath.IsNullOrZeroEmpty()) return;

            // 先重置所有參數設置
            this._Reset();

            // 代入UI路徑
            this.dialogueUIPath = dialogueUIPath;

            // 開啟對話UI
            await this._CheckAndOpenDialogueUI();

            // 檢查並獲取在場景中的DialogueUI型別是否正確
            this._dialogueUI = this._CheckAndGetDialogueUIOnFrame();

            // 如果沒有對話UI或無法初始對話資料就不運行對話
            if (this._dialogueUI != null && this._ParsingContainerDataIntoThis(dialogueDataContainer))
            {
                // 指定代入Callback至成員Callback
                this._endCb = endCb;

                // 設定是否要手動調用Callback
                this.manualInvokeCb = manualInvokeCb;

                // 設定是否不強迫選擇對話事件
                this.doNotForceChooseEvent = doNotForceChooseEvent;

                // 設置對話運行狀態
                this.isRunningDialogue = true;

                // 調用子類各別實現開始運行對話方法
                this._OnStart();
            }
            else
            {
                // 結束對話，釋放資源，不調用Callback
                this.Stop(true);

                Debug.Log("<color=#FF0000>對話資料儲存器轉換錯誤，或是無法取得對話UI</color>");
            }
        }

        /// <summary>
        /// 停止當前對話，關閉對話UI，並調用EndCallback
        /// </summary>
        /// <param name="disableEndCb">是否停用EndCallback，預設為false</param>
        public void Stop(bool disableEndCb = false)
        {
            // 調用對話結束時Callback
            if (!disableEndCb && !this.manualInvokeCb) this._endCb?.Invoke();
            // 關閉對話UI
            this._CheckAndCloseDialogueUI();
            // 釋放資源
            this._Release();
            // 設置運行狀態為False
            this.isRunningDialogue = false;
        }

        /// <summary>
        /// 跳過當前對話
        /// </summary>
        public void Skip()
        {
            // 如果沒有對話也沒有對話選項就不執行
            if (this.dialogueCount == 0 && this.dialogueEventDataCount == 0) return;

            // 如果當前執行的對話Index < 最後一個對話的Index，代表當前對話非最後一個對話，這時做跳過的動作才有意義
            if (this.currentDialogueIndex < this.dialogueMaxIndex)
            {
                this.isSkippingDialogue = true;
                this._UpdateDialogue(this.dialogueMaxIndex);
                this.isSkippingDialogue = false;
            }
            else
            {
                Debug.Log("<color=#FF0000>已經為最後一個對話，無法跳過!!</color>");
            }
        }

        /// <summary>
        /// 手動調用End Callback，需在對話開始時開啟手動調用開關，僅能調用一次
        /// </summary>
        public void InvokeEndCbManual()
        {
            // 如果有開啟手動調用Callback
            if (this.manualInvokeCb)
            {
                // 調用Callback
                this._endCb?.Invoke();
                // 資源釋放
                this._endCb = null;
                // 關閉手動調用Callback
                this.manualInvokeCb = false;
                Debug.Log("<color=#FF0000>Callback調用完畢，將無法再次調用</color>");
            }
            else
            {
                Debug.Log("<color=#FF0000>並無開啟手動調用Callback!!</color>");
            }
        }

        /// <summary>
        /// 檢查是否正在運行對話
        /// </summary>
        /// <returns></returns>
        public bool IsRunningDialogue()
        {
            return this.isRunningDialogue;
        }
        #endregion

        #region 父類保護方法

        /// <summary>
        /// 從第一個對話資料開始更新對話UI顯示
        /// </summary>
        protected void _BeginDialogue()
        {
            if (!this.IsRunningDialogue()) return;
            this._UpdateDialogue(this.dialogueStartIndex);
        }

        #endregion

        #region 父類私有方法

        /// <summary>
        /// 更新對話選項及相關顯示
        /// </summary>
        private void _UpdateDialogue(int index)
        {
            // 先清除UI顯示
            this._dialogueUI.ClearUI();

            // 記錄此次的對話Index
            this.currentDialogueIndex = index;

            // 事件處理
            // 如果對話數量為0
            if (this.dialogueCount == 0)
            {
                // 有對話事件就設置按鈕
                if (this.dialogueEventDataCount > 0)
                {
                    if (this.doNotForceChooseEvent)
                    {
                        // 直接設置點擊Mask結束對話
                        this._dialogueUI.SetMaskEvent(() =>
                        {
                            // 調用子類各別實現對話結束方法 (強迫對話結束)
                            this._OnFinished(this.doNotForceChooseEvent);
                        }
                        );
                    }

                    this._UpdateDialogueEventOptions();
                }
                else
                {
                    // 調用子類各別實現對話結束方法
                    this._OnFinished();
                }
                return;
            }
            // 如果當前Index等於當前數量 - 1 (數量為1，取的Index就是0，依此類推)，代表當前傳入對話Index為最後一個對話，並依照對話事件數量來判斷後續動作
            else if (index == this.dialogueMaxIndex)
            {
                // 有對話事件就設置按鈕
                if (this.dialogueEventDataCount > 0)
                {
                    if (this.doNotForceChooseEvent)
                    {
                        // 直接設置點擊Mask結束對話
                        this._dialogueUI.SetMaskEvent(() =>
                        {
                            // 調用子類各別實現對話結束方法 (強迫對話結束)
                            this._OnFinished(this.doNotForceChooseEvent);
                        }
                        );
                    }

                    this._UpdateDialogueEventOptions();
                }
                // 沒有就依照是否有Skip對話來設置相關結束對話之作法
                else
                {
                    // 如果狀態為Skip對話
                    if (this.isSkippingDialogue)
                    {
                        // 調用子類各別實現對話結束方法
                        this._OnFinished();
                        return;
                    }
                    // 如果為正常運行對話
                    else
                    {
                        // 直接設置點擊Mask結束對話
                        this._dialogueUI.SetMaskEvent(() =>
                        {
                            // 調用子類各別實現對話結束方法
                            this._OnFinished();
                        }
                        );
                    }
                }
            }
            // 如果當前Index小於當前數量 - 1，代表後面還有對話可以講
            else if (index < this.dialogueMaxIndex)
            {
                // 設置點擊Mask進入下一對話
                this._dialogueUI.SetMaskEvent(() =>
                {
                    this._UpdateDialogue(index);
                }
                );
            }

            // 以下設置UI顯示
            // 利用當前Index取得對話資料
            DialogueData currentDialogue = this._GetCurrentDialogue(index);

            if (currentDialogue == null)
            {
                Debug.Log(string.Format("<color=#FF0000>無法取得對話資料，將不顯示對話資訊，當前對話Index:{0} || 下一對話Index:{1} || 對話List最大Index:{2} || 對話總數量:{3} || 對話選項數量:{4}</color>", this.currentDialogueIndex, this.nextDialogueIndex > this.dialogueMaxIndex ? "<color=#FF0000>已無下一對話</color>" : this.nextDialogueIndex.ToString(), this.dialogueMaxIndex, this.dialogueCount, this.dialogueEventDataCount));
                return;
            }

            // 設置講者顯示
            this._dialogueUI.SetSpeakers(currentDialogue.speakerDatas).Forget();

            // 設置對話文字內容
            // 如果狀態為Skip對話
            if (this.isSkippingDialogue)
            {
                // 設置文字，並關閉打字機效果
                this._dialogueUI.SetTexts(currentDialogue.speakerName, currentDialogue.content, true);
            }
            // 如果為正常運行對話
            else
            {
                // 設置文字
                this._dialogueUI.SetTexts(currentDialogue.speakerName, currentDialogue.content);
            }

            // 設置音效
            this._dialogueUI.SetAudio(currentDialogue.audioId);

            // 增加Index
            index++;

            // 記錄增加後的Index下一對話Index
            this.nextDialogueIndex = index;

            Debug.Log(string.Format("<color=#FFC702>對話資料運行中:當前對話Index:{0} || 下一對話Index:{1} || 對話List最大Index:{2} || 對話總數量:{3} || 對話選項數量:{4} </color>", this.currentDialogueIndex, this.nextDialogueIndex > this.dialogueMaxIndex ? "<color=#FF0000>已無下一對話</color>" : this.nextDialogueIndex.ToString(), this.dialogueMaxIndex, this.dialogueCount, this.dialogueEventDataCount));
        }

        /// <summary>
        /// 更新事件選項按鈕至UI顯示
        /// </summary>
        private void _UpdateDialogueEventOptions()
        {
            // 儲存處理好的按鈕物件
            List<Transform> listDialogueEventBtn = new List<Transform>();

            // 以當前有效的對話端口數量來跑迴圈
            for (int i = 0; i < this._currentDialogueEventDatas.Count; i++)
            {
                // 生成按鈕並將其加入到對話UI的ScrollView中
                Transform dialogueEventBtn = GameObject.Instantiate(this._dialogueUI.dialogueEventBtn, this._dialogueUI.transform).transform;

                // 設置按鈕文字
                TMP_Text text = dialogueEventBtn.GetComponentInChildren<TMP_Text>();
                text.text = this._currentDialogueEventDatas[i].desc;

                // 此處由子類各別設置按鈕事件
                Button button = dialogueEventBtn.GetComponentInChildren<Button>();
                this._OnSetDialogueEventBtn(button, this._currentDialogueEventDatas[i]);

                // 將處理好的按鈕新增到List
                listDialogueEventBtn.Add(dialogueEventBtn);
            }

            // 將處理好的按鈕物件傳入UI做顯示
            this._dialogueUI.SetEventOptions(listDialogueEventBtn);
        }

        /// <summary>
        /// 依據對話UI顯示狀況來開啟對話UI
        /// </summary>
        /// <returns></returns>
        private async UniTask _CheckAndOpenDialogueUI()
        {
            if (!UIManager.GetInstance().CheckIsShowing(this.dialogueUIPath))
            {
                await UIManager.GetInstance().Show(this.dialogueUIPath);
            }
        }

        /// <summary>
        /// 依據對話UI顯示狀況來關閉對話UI
        /// </summary>
        private void _CheckAndCloseDialogueUI()
        {
            if (UIManager.GetInstance().CheckIsShowing(this.dialogueUIPath))
            {
                UIManager.GetInstance().Close(this.dialogueUIPath);
            }
        }

        /// <summary>
        /// 檢查並獲取在場景中的DialogueUI型別是否正確
        /// </summary>
        /// <returns></returns>
        private DialogueBaseUI _CheckAndGetDialogueUIOnFrame()
        {
            DialogueBaseUI dialogueBaseUI = null;

            try
            {
                dialogueBaseUI = UIManager.GetInstance().GetFrameComponent<UIBase>(this.dialogueUIPath) as DialogueBaseUI;
            }
            catch
            {
                Debug.Log("<color=#FF0000>場景UI組件獲取或轉型失敗</color>");
            }

            return dialogueBaseUI;
        }

        /// <summary>
        /// 取得對話資料
        /// </summary>
        /// <param name="targetIndex">欲從List中取得的對話Index</param>
        /// <returns>對話資料</returns>
        private DialogueData _GetCurrentDialogue(int targetIndex)
        {
            if (this.dialogueCount == 0) return null;

            return this._currentDialogueDatas[targetIndex];
        }

        /// <summary>
        /// 資源釋放
        /// </summary>
        private void _Release()
        {
            this._dialogueUI = null;
            // 如果沒開啟手動調用Callback才釋放Callback記憶體
            if (!this.manualInvokeCb) this._endCb = null;
            this._currentDialogueDatas = null;
            this._currentDialogueEventDatas = null;

            // 調用子類各別實現資源釋放方法
            this._OnRelease();
        }

        /// <summary>
        /// 重置參數設置
        /// </summary>
        private void _Reset()
        {
            this.currentDialogueIndex = 0;
            this.nextDialogueIndex = 0;
            this.isSkippingDialogue = false;
            this.isRunningDialogue = false;
            this.manualInvokeCb = false;
            this.doNotForceChooseEvent = false;
            this.dialogueUIPath = "";
        }
        #endregion

        #region 子類實作方法

        #region 必須性實作

        /// <summary>
        /// 由父類對話初始完成時時調用，並由子類各別實現開始對話方法
        /// </summary>
        protected abstract void _OnStart();

        /// <summary>
        /// 由父類對話結束時調用，並由子類各別實現結束對話方法
        /// </summary>
        /// <param name="forceFinish">是否要強迫結束對話</param>
        protected abstract void _OnFinished(bool forceFinish = false);

        /// <summary>
        /// 由父類設置對話選項按鈕調用，並由子類各別實現設置按鈕事件
        /// </summary>
        protected abstract void _OnSetDialogueEventBtn(Button button, DialogueEventData dialogueEventData);

        /// <summary>
        /// 將儲存器資料轉換成對話資料，由子類實作
        /// </summary>
        /// <param name="dialogueDataContainer">對話資料儲存器</param>
        /// <returns></returns>
        protected abstract bool _ParsingContainerDataIntoThis(object dialogueDataContainer);

        #endregion

        #region 選擇性實作

        /// <summary>
        /// 由父類做資源釋放時調用，並由子類各別實現子類成員資源釋放
        /// </summary>
        protected virtual void _OnRelease() { }

        #endregion

        #endregion
    }
}

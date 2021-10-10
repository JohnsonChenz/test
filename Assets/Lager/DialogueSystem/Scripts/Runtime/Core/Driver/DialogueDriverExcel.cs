using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSys
{
    public class DialogueDriverExcel : DialogueDriverBase
    {
        public DialogueDriverExcel(DialogueSystemBase dialogueSystem)
        {
            this._dialogueSystem = dialogueSystem;
            this.stringDB = null;
        }

        /// <summary>
        /// 事件選項類型
        /// </summary>
        private enum EventType
        {
            System = 1,
            Dialogue = 2
        }

        /// <summary>
        /// Excel對話json資料名稱
        /// </summary>
        public struct DialogueExcelDB
        {
            public string dialogueData;
            public string dialogueTextData;
            public string dialogueEventData;
        }

        public DialogueExcelDB dialogueExcelDB;  // Excel對話json資料名稱定義
        public JObject stringDB;                 // Excel對話文字大表JObject

        #region 公有方法
        /// <summary>
        /// 設置Excel Json資源名稱定義
        /// </summary>
        /// <param name="datas">Excel Json表資料名稱</param>
        public void SetDialogueExcelDB(params string[] datas)
        {
            this.dialogueExcelDB.dialogueData = datas[0];
            this.dialogueExcelDB.dialogueTextData = datas[1];
            this.dialogueExcelDB.dialogueEventData = datas[2];
        }

        /// <summary>
        /// 設置Excel文字大表JObject
        /// </summary>
        /// <param name="stringDB">Excel文字大表JObject物件</param>
        public void SetDialogueExcelStringDB(JObject stringDB)
        {
            this.stringDB = stringDB;
        }
        #endregion

        #region 實作/覆寫父類方法
        protected override void _OnStart()
        {
            // 開始更新對話資料及相關顯示
            this._BeginDialogue();
        }

        protected override void _OnFinished(bool forceFinish = false)
        {
            // 要是今天打開了不強迫選擇對話事件的開關，出現使用者可以直接點擊任一處關閉對話而不用選擇對話選項的情況
            // DriverBase就會藉由forceFinish告知子類是否要強迫結束對話，故此處子類得藉由forceFinish來判斷下一步操作
            // 對於Excel對話來說，不會有像Node對話需要運行下一對話節點的問題，故此處不判斷forceFinish，直接關閉UI

            // 結束對話，釋放資源，調用Callback
            this.Stop();
        }

        protected override void _OnSetDialogueEventBtn(Button button, DialogueEventData dialogueEventData)
        {
            // 轉型取得Excel版本的事件選項類別
            DialogueEventDataForExcel dialogueEventDataForExcel = dialogueEventData as DialogueEventDataForExcel;

            // 安全檢測
            if (dialogueEventDataForExcel == null)
            {
                Debug.Log("<color=#FF0000>【Excel Driver】事件選項類別轉型失敗</color>");
                return;
            }

            // 取得事件Id
            string funcId = dialogueEventDataForExcel.funcId;

            switch ((EventType)dialogueEventDataForExcel.type)
            {
                // 系統事件
                case EventType.System:

                    button.onClick.AddListener(() =>
                    {
                        // 結束對話，釋放資源，調用Callback
                        this.Stop();

                        // 通知EventCenter，Handle對應Id事件
                        this._dialogueSystem.eventCenter.DirectCall(EventSys.EventCenterBase.EVENT_xBASE + Convert.ToInt32(funcId));
                    });

                    break;

                // 對話事件
                case EventType.Dialogue:

                    button.onClick.AddListener(() =>
                    {
                        // 通知對話管理器，Handle對應Id事件
                        this._dialogueSystem.dialogueManager.StartDialogue(funcId, this.dialogueUIPath, this._endCb, this.manualInvokeCb);
                    });

                    break;

                default:
                    break;
            }
        }

        protected override bool _ParsingContainerDataIntoThis(object dialogueDataContainer)
        {
            // 將物件轉型為Excel對話資料專用儲存器
            DialogueDataContainerForExcel dialogueDataContainerForExcel = dialogueDataContainer as DialogueDataContainerForExcel;

            if (dialogueDataContainerForExcel == null) return false;

            // 從儲存器取得當前對話資料List
            this._currentDialogueDatas = dialogueDataContainerForExcel.dialogueDatas.ToList();

            // 從儲存器取得當前對話事件選項資料
            this._currentDialogueEventDatas = dialogueDataContainerForExcel.dialogueEventDatas.ToList();

            return true;
        }
        #endregion
    }
}
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace DialogueSys
{
    public class DialogueManager
    {
        public DialogueDataBase dialogueDataBase { get; private set; }   // 對話DataBase快取存放器
        private DialogueSystemBase _dialogueSystem;                      // 對話系統總控組件
        public DialogueDriverBase dialogueDriver { get; private set; }   // 對話驅動器

        public DialogueManager(DialogueSystemBase dialogueSystem)
        {
            // 依照Define驅動器類型初始對應對話驅動器
            // Excel對話驅動器
            if (DialogueSysDefine.bExcelType)
            {
                this.dialogueDriver = new DialogueDriverExcel(dialogueSystem);

            }
            // Node對話驅動器
            if (DialogueSysDefine.bNodeType)
            {
                this.dialogueDriver = new DialogueDriverNode(dialogueSystem);
            }

            this._dialogueSystem = dialogueSystem;
            this.dialogueDataBase = new DialogueDataBase();
        }

        /// <summary>
        /// 啟動對話
        /// </summary>
        /// <param name="dialogueId">對話群組ID</param>
        /// <param name="dialogueUIPath">所要使用的對話UI路徑</param>
        /// <param name="endCb">對話結束所調用的Callback</param>
        /// <param name="manualInvokeCb">是否開啟手動調用Callback，預設為False</param>
        /// <param name="doNotForceChooseEvent">是否不強迫選擇對話事件，預設為False</param>
        public void StartDialogue(string dialogueId, string dialogueUIPath, Action endCb = null, bool manualInvokeCb = false, bool doNotForceChooseEvent = false)
        {
            // 如果沒有驅動器就不啟動對話
            if (this.dialogueDriver == null) return;

            // 判斷驅動器種類
            switch (this.dialogueDriver)
            {
                // Excel版本對話驅動器
                case DialogueDriverExcel dialogueDriverExcel:

                    // 初始Excel對話資料存放器
                    DialogueDataContainerForExcel dialogueDataContainerForExcel = new DialogueDataContainerForExcel();

                    // 以下取得各資料表之路徑
                    // 對話群組表路徑
                    string dialogueDataDBPath = this._dialogueSystem.dialogueDBPath.dbPath + dialogueDriverExcel.dialogueExcelDB.dialogueData;
                    // 對話文本表路徑
                    string dialogueTextDataDBPath = this._dialogueSystem.dialogueDBPath.dbPath + dialogueDriverExcel.dialogueExcelDB.dialogueTextData;
                    // 對話事件表路徑
                    string dialogueEventDataDBPath = this._dialogueSystem.dialogueDBPath.dbPath + dialogueDriverExcel.dialogueExcelDB.dialogueEventData;

                    // 開始透過對話群組ID解析對話資料，並從對話資料快取存放器中拿出並代入必需的DataBase資料
                    // 如果驅動器內存有文字大表JObject，所有對話文字則會透過文字大表去解析
                    bool parsedFlag = dialogueDataContainerForExcel.ParsingDialogueExcelData
                        (
                            dialogueId,
                            new object[]
                            {
                                // 對話群組表
                                this.dialogueDataBase.GetDataBase(dialogueDataDBPath),
                                // 對話文本表
                                this.dialogueDataBase.GetDataBase(dialogueTextDataDBPath),
                                // 對話事件表                                                                 
                                this.dialogueDataBase.GetDataBase(dialogueEventDataDBPath)
                            },
                            dialogueDriverExcel.stringDB
                        );
                    if (parsedFlag)
                    {
                        // 將解析好的Excel對話資料存放器代入到對話驅動器，代入UI路徑，Callback，以及是否要手動調用Callback，開始初始對話驅動器
                        dialogueDriverExcel.Init(dialogueDataContainerForExcel, dialogueUIPath, endCb, manualInvokeCb, doNotForceChooseEvent).Forget();

                        Debug.Log(string.Format("<color=#FFC702>對話驅動器類型 : Excel </color>"));
                    }
                    // 解析失敗
                    else
                    {
                        Debug.Log("<color=#FF0000>解析Excel Json檔出錯，將不執行對話</color>");
                    }

                    break;

                // 視覺化(Node)版本對話驅動器
                case DialogueDriverNode dialogueDriverNode:

                    // 初始Node對話資料存放器
                    DialogueDataContainerForNode dialogueDataContainerForNode = ScriptableObject.CreateInstance<DialogueDataContainerForNode>();

                    // 取得Node對話資料表路徑
                    string dialogueNodeDBPath = this._dialogueSystem.dialogueDBPath.dbPath + dialogueId;

                    // 取得Node對話資料表
                    string json = this.dialogueDataBase.GetDataBase(dialogueNodeDBPath).ToString(Formatting.None);

                    try
                    {
                        // 透過對話群組ID從對話資料快取存放器取出對應Json Database，並使用JsonUtility反序列化並覆寫上面初始的Node對話資料存放器
                        JsonUtility.FromJsonOverwrite(json, dialogueDataContainerForNode);
                    }
                    // 覆寫失敗
                    catch
                    {
                        Debug.Log("<color=#FF0000>Node Json檔覆寫出錯</color>");
                        return;
                    }

                    // 將解析好的Node對話資料存放器代入到對話驅動器，代入UI路徑以及Callback，以及是否要手動調用Callback，開始初始對話驅動器
                    dialogueDriverNode.Init(dialogueDataContainerForNode, dialogueUIPath, endCb, manualInvokeCb, doNotForceChooseEvent).Forget();

                    Debug.Log(string.Format("<color=#FFC702>對話驅動器類型 : Node </color>"));

                    break;
            }
        }

        /// <summary>
        /// 停止對話，並執行Callback
        /// </summary>
        public void StopDialogue(bool disableEndCb = false)
        {
            // 如果沒有驅動器就或驅動器無運行對話中就不執行
            if (this.dialogueDriver == null || !this.IsRunningDialogue()) return;

            this.dialogueDriver.Stop(disableEndCb);
        }

        /// <summary>
        /// 跳過對話
        /// </summary>
        public void SkipDialogue()
        {
            // 如果沒有驅動器就或驅動器無運行對話中就不執行
            if (this.dialogueDriver == null || !this.IsRunningDialogue()) return;

            this.dialogueDriver.Skip();
        }

        public void InvokeEndCbManual()
        {
            // 如果沒有驅動器就不執行
            if (this.dialogueDriver == null) return;

            this.dialogueDriver.InvokeEndCbManual();
        }

        /// <summary>
        /// 是否有在運行對話
        /// </summary>
        /// <returns></returns>
        public bool IsRunningDialogue()
        {
            return this.dialogueDriver.IsRunningDialogue();
        }
    }
}

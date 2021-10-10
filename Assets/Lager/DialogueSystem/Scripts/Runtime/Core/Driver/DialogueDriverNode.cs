using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSys
{
    public class DialogueDriverNode : DialogueDriverBase
    {
        public DialogueDriverNode(DialogueSystemBase dialogueSystem)
        {
            this._dialogueSystem = dialogueSystem;
        }

        private DialogueDataContainerForNode dialogueDataContainer;   // 當前的視覺化對話資料儲存器
        private BaseNodeData currentRunningNode;                      // 當前運行節點

        private Dictionary<string, BaseNodeData> _dictAllNodes      // 全部節點儲存器
        {
            get
            {
                Dictionary<string, BaseNodeData> tmp = new Dictionary<string, BaseNodeData>();

                this.dialogueDataContainer.listDialogueNodeData?.ForEach(node =>
                {
                    tmp.Add(node.nodeGuid, node);
                });

                this.dialogueDataContainer.listStartNodeData?.ForEach(node =>
                {
                    tmp.Add(node.nodeGuid, node);
                });

                this.dialogueDataContainer.listEventNodeSOData?.ForEach(node =>
                {
                    tmp.Add(node.nodeGuid, node);
                });

                this.dialogueDataContainer.listEventNodeStrData?.ForEach(node =>
                {
                    tmp.Add(node.nodeGuid, node);
                });

                this.dialogueDataContainer.listEndNodeData?.ForEach(node =>
                {
                    tmp.Add(node.nodeGuid, node);
                });

                return tmp;
            }
        }

        #region 實作/覆寫父類方法

        protected override void _OnStart()
        {
            // 從Start節點開始運行
            this._BeginNodeProcess();
        }

        protected override void _OnFinished(bool forceFinish = false)
        {
            // 對於Node對話來說，在_OnFinished方法裡不是像Excel對話一樣結束對話，而是會運行下一節點，可能是對話、事件、終點節點
            // 因此要是今天打開了不強迫選擇對話事件的開關，出現使用者可以直接點擊任一處關閉對話而不用選擇對話選項的情況
            // DriverBase就會藉由forceFinish告知子類是否要強迫結束對話，故此處子類得藉由forceFinish來判斷下一步操作

            // 如果判斷要強迫結束對話
            if (forceFinish)
            {
                // 結束對話，釋放資源，調用Callback
                this.Stop();
            }
            // 如果無，進入下一節點
            else
            {
                this._ProceedToNextNode(this.currentRunningNode.mainNodePort.inputGuid);
            }
        }

        protected override void _OnSetDialogueEventBtn(Button button, DialogueEventData dialogueEventData)
        {
            // 轉型取得Node版本的事件選項類別
            DialogueEventDataForNode dialogueEventDataForNode = dialogueEventData as DialogueEventDataForNode;

            // 安全檢測
            if (dialogueEventDataForNode == null)
            {
                Debug.Log("<color=#FF0000>【Node Driver】事件選項類別轉型失敗</color>");
                return;
            }

            // 取得事件Id
            string inputGuid = dialogueEventDataForNode.inputGuid;
            button.onClick.AddListener(() =>
            {
                // 運行下一節點
                this._ProceedToNextNode(inputGuid);
            });
        }

        protected override void _OnRelease()
        {
            this.currentRunningNode = null;
            this.dialogueDataContainer = null;
        }

        protected override bool _ParsingContainerDataIntoThis(object dialogueDataContainer)
        {
            // 將物件轉型為視覺化對話資料專用儲存器
            this.dialogueDataContainer = dialogueDataContainer as DialogueDataContainerForNode;

            return (this.dialogueDataContainer != null);
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 透過Guid索引並取得下一節點資料，並依照種類運行其實作內容
        /// </summary>
        /// <param name="guid">目標節點Guid</param>
        private void _ProceedToNextNode(string guid)
        {
            if (!this._dictAllNodes.ContainsKey(guid))
            {
                Debug.Log("<color=#FF0000>節點輸出目標為空</color>");
                return;
            }

            Debug.Log(string.Format("<color=#FFC702>通往下一節點...</color>"));

            BaseNodeData currentRunningNode = null;

            try
            {
                currentRunningNode = this._dictAllNodes[guid];
            }
            catch 
            { 

            }

            if (currentRunningNode == null)
            {
                Debug.Log(string.Format("<color=#FF0000>無法透過Guid取得節點資料，請檢查節點Guid是否正確!! Guid : 【{0}】</color>", guid));
                return;
            }

            this.currentRunningNode = currentRunningNode;

            switch (currentRunningNode)
            {
                case DialogueNodeData node:
                    this._ProcessDialogueNodeData(node);
                    break;

                case EventNodeSOData node:
                    this._ProcessEventNodeSOData(node);
                    break;

                case EventNodeStrData node:
                    this._ProcessEventNodeStrData(node);
                    break;

                case EndNodeData node:
                    this._ProcessEndNodeData(node);
                    break;
            }
        }

        /// <summary>
        /// 開始運行節點，將會從StartNode開始
        /// </summary>
        private void _BeginNodeProcess()
        {
            StartNodeData startNodeData = this._dictAllNodes.Where(node => node.Value is StartNodeData).FirstOrDefault().Value as StartNodeData;
            if (startNodeData == null) return;
            this._ProcessStartNodeData(startNodeData);
        }

        /// <summary>
        /// 運行Start節點
        /// </summary>
        /// <param name="startNodeData">Start節點資料</param>
        private void _ProcessStartNodeData(StartNodeData startNodeData)
        {
            if (startNodeData == null) return;

            Debug.Log(string.Format("<color=#FFC702>正在運行StartNode節點</color>"));

            // 直接運行下一節點
            this._ProceedToNextNode(startNodeData.mainNodePort.inputGuid);
        }

        /// <summary>
        /// 運行Dialogue節點
        /// </summary>
        /// <param name="dialogueNodeData">Dialogue節點資料</param>
        private void _ProcessDialogueNodeData(DialogueNodeData dialogueNodeData)
        {
            // 如果初始對話資料失敗就不運行對話
            if (!this._ParsingDialogueNodeDataIntoThis(dialogueNodeData)) return;

            Debug.Log(string.Format("<color=#FFC702>正在運行DialogueNode節點</color>"));

            // 開始更新對話資料及相關顯示
            this._BeginDialogue();
        }

        /// <summary>
        /// 運行EventNodeStr節點
        /// </summary>
        /// <param name="eventNodeStrData">EventNodeStr節點資料</param>
        private void _ProcessEventNodeStrData(EventNodeStrData eventNodeStrData)
        {
            if (eventNodeStrData == null) return;

            // 運行事件
            int funcId = 0;

            try
            {
                funcId = Convert.ToInt32(eventNodeStrData.funcId);
            }
            catch
            {
                Debug.Log("<color=#FF0000>事件ID轉換失敗，請檢查事件ID格式是否輸入正確!!</color>");
            }

            // 通知EventCenter，Handle對應Id事件
            this._dialogueSystem.eventCenter.DirectCall(EventSys.EventCenterBase.EVENT_xBASE + funcId);

            // 運行下一節點
            this._ProceedToNextNode(eventNodeStrData.mainNodePort.inputGuid);
        }

        /// <summary>
        /// 運行EventNodeSO節點
        /// </summary>
        /// <param name="eventNodeSOData">EventNodeSO節點資料</param>
        private void _ProcessEventNodeSOData(EventNodeSOData eventNodeSOData)
        {
            if (eventNodeSOData == null) return;

            Debug.Log(string.Format("<color=#FFC702>正在運行EventNodeSO節點</color>"));

            // 運行下一節點
            this._ProceedToNextNode(eventNodeSOData.mainNodePort.inputGuid);
        }

        /// <summary>
        /// 運行End節點資料
        /// </summary>
        /// <param name="endNodeData">End節點資料</param>
        private void _ProcessEndNodeData(EndNodeData endNodeData)
        {
            if (endNodeData == null) return;

            Debug.Log(string.Format("<color=#FFC702>正在運行EndNode節點</color>"));

            // 結束對話，釋放資源，調用Callback
            this.Stop();
        }

        /// <summary>
        /// 初始化對話節點資料
        /// </summary>
        /// <param name="dialogueNodeData">對話節點資料</param>
        private bool _ParsingDialogueNodeDataIntoThis(DialogueNodeData dialogueNodeData)
        {
            if (dialogueNodeData == null)
            {
                Debug.Log("<color=#FF0000>對話節點資料為空，初始化錯誤!!，將不運行節點資料</color>");
                return false;
            }

            // 儲存當前對話資料List
            this._currentDialogueDatas = dialogueNodeData.dialogueDatas.ToList();

            // 從當前對話節點資料取得有效的事件選項端口
            List<DialogueNodePort> listCurrentValidNodePort = dialogueNodeData.listDialogueNodePort?.Where(x => x.inputGuid != string.Empty).ToList();

            // 重新初始事件選項List資料
            this._currentDialogueEventDatas = new List<DialogueEventData>();

            // 從有效的事件選項端口取得事件選項描述，以及下一節點Guid。
            foreach (var nodePort in listCurrentValidNodePort)
            {
                DialogueEventDataForNode eventOptionForNode = new DialogueEventDataForNode();
                eventOptionForNode.inputGuid = nodePort.inputGuid;
                eventOptionForNode.desc = nodePort.portText;

                // 將解析好的事件選項資料加到List中
                this._currentDialogueEventDatas.Add(eventOptionForNode);
            }

            return true;
        }
        #endregion
    }
}

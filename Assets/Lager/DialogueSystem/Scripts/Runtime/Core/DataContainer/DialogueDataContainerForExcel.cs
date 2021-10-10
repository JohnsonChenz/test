using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DialogueSys
{
    public class DialogueDataContainerForExcel
    {
        public List<DialogueData> dialogueDatas;           // 對話List
        public List<DialogueEventData> dialogueEventDatas; // 事件選項Lists

        public DialogueDataContainerForExcel()
        {
            this.dialogueDatas = new List<DialogueData>();
            this.dialogueEventDatas = new List<DialogueEventData>();
        }

        /// <summary>
        /// 解析Excel Json DataBase資料
        /// </summary>
        /// <param name="dialogueId">對話群組ID</param>
        /// <param name="dataBase">對話Excel Json資料</param>
        /// <returns>成功解析回傳true</returns>
        public bool ParsingDialogueExcelData(string dialogueId, object dataBase, JObject stringDB = null)
        {
            return this._ParsingDBDataIntoThis(dialogueId, dataBase, stringDB);
        }

        /// <summary>
        /// 解析Excel Json DataBase資料進入儲存器資料
        /// </summary>
        /// <param name="dialogueId">對話群組ID</param>
        /// <param name="dataBase">對話Excel Json資料</param>
        /// <returns>成功解析回傳true</returns>
        private bool _ParsingDBDataIntoThis(string dialogueId, object dataBase, JObject stringDB)
        {
            object[] data = dataBase as object[];

            // 取得對話群組表
            JObject dialogueGroupData = data[0] as JObject;
            // 取得對話文本表
            JObject dialogueTextData = data[1] as JObject;
            // 取得對話事件表
            JObject dialogueEventData = data[2] as JObject;

            if (dialogueGroupData == null || dialogueTextData == null || dialogueEventData == null) return false;

            // 以下開始解析對話資料並加入到對話List

            for (int i = 0; i < 30; i++)
            {
                // 取得文本ID
                string txtId = dialogueGroupData.SelectToken<string>(dialogueId, string.Format("TXT_ID_{0}", i));

                // 如果文本ID為0，或是文本表內沒有該文本ID就跳過
                if (txtId.IsNullOrZeroEmpty() || dialogueTextData.Property(txtId) == null) continue;

                // 對話文本音效ID
                DialogueData dialogueData = new DialogueData();
                dialogueData.audioId = dialogueTextData.SelectToken<string>(txtId, "SOUND");

                // 依是否有大表JObejct來解析講者名稱
                string speakerName = dialogueTextData.SelectToken<string>(txtId, "SPEAKER_NAME");
                dialogueData.speakerName = (stringDB != null) ? this._GetStringByCode(stringDB, speakerName) : speakerName;

                // 依是否有大表JObejct來解析對話內容
                string content = dialogueTextData.SelectToken<string>(txtId, "CONTENT");
                dialogueData.content = (stringDB != null) ? this._GetStringByCode(stringDB, content) : content;

                // 開始儲存講者資訊
                for (int j = 0; j < dialogueData.speakerDatas.Length; j++)
                {
                    JArray jArrSpeakerInfo = dialogueTextData.SelectToken<JArray>(txtId, string.Format("SPEAKER_ARGS_{0}", j));

                    // 如果取出的講者資料陣列長度為3，才取資料
                    if (jArrSpeakerInfo.Children().Count() == 3)
                    {
                        dialogueData.speakerDatas[j].gid = jArrSpeakerInfo[0].ToString();
                        dialogueData.speakerDatas[j].speakerImageReverse = (DialogueSpeakerImageReverse)jArrSpeakerInfo[1].ToObject<int>();
                        dialogueData.speakerDatas[j].speakerImageTurnBlack = Convert.ToBoolean(jArrSpeakerInfo[2].ToObject<int>());
                    }
                }

                // 將解析好的對話加入到對話List中
                this.dialogueDatas.Add(dialogueData);
            }

            // 以下開始解析選項資料並加入到選項List

            // 取得表內選項資料Array
            JArray eventOptions = dialogueGroupData.SelectToken<JArray>(dialogueId, "EVENT_OPTIONS");

            // 以實際Array長度來跑迴圈
            for (int i = 0; i < eventOptions.Count; i++)
            {
                // 取得事件ID
                string eventId = eventOptions[i].ToString();

                // 如果事件表裡沒有取出的選項ID，視為無效選項，將不被解析
                if (eventId.IsNullOrZeroEmpty() || dialogueEventData.Property(eventId) == null) continue;

                // 實例化Excel版本事件儲存類別
                DialogueEventDataForExcel dialogueEventDataForExcel = new DialogueEventDataForExcel();

                // 事件類型
                dialogueEventDataForExcel.type = Convert.ToInt32(eventId.Substring(0, 1));

                // 事件FuncId
                dialogueEventDataForExcel.funcId = dialogueEventData.SelectToken<string>(eventId, "EVENT_FUNC");

                // 依是否有大表JObejct來解析事件選項描述
                string desc = dialogueEventData.SelectToken<string>(eventId, "EVENT_DESC");
                dialogueEventDataForExcel.desc = (stringDB != null) ? this._GetStringByCode(stringDB, desc) : desc;

                // 將解析好的選項加入到選項List中
                this.dialogueEventDatas.Add(dialogueEventDataForExcel);
            }

            return true;
        }

        /// <summary>
        /// 從文字大表取得對應文字
        /// </summary>
        /// <param name="stringDB">文字大表JObject</param>
        /// <param name="strCode">文字在大表中的Key</param>
        /// <returns></returns>
        private string _GetStringByCode(JObject stringDB, string strCode)
        {
            string str = null;

            try
            {
                str = stringDB.SelectToken<string>(strCode, "QQ_MSG");
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("<color=#FF0000>{0}</color>", e));
            }

            return str;
        }
    }
}

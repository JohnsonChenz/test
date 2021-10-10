using System;
using UnityEngine;

namespace DialogueSys
{
    /// <summary>
    /// 此物件用以儲存主要對話資訊
    /// </summary>
    [Serializable]
    public class DialogueData
    {
        public string content;                                  // 對話內容
        public string audioId;                                  // 對話音效id
        public string speakerName = "";                         // 當前講者名稱
        public SpeakerData[] speakerDatas;                      // 三位講者顯示參數及資料

        public DialogueData()
        {
            this.speakerDatas = new SpeakerData[3]
            {
                    new SpeakerData(this),
                    new SpeakerData(this),
                    new SpeakerData(this)
            };
        }
    }

    /// <summary>                                               
    /// 此物件用以儲存講者顯示參數                              
    /// </summary>                                              
    [Serializable]
    public class SpeakerData
    {
        [NonSerialized] public DialogueData parent;             // 隸屬的對話資訊
        public string gid;                                      // 講者預製體模型GID
        public DialogueSpeakerImageReverse speakerImageReverse; // 講者模型/圖片面向
        public bool speakerImageTurnBlack;                      // 講者模型/圖片是否要褪黑

        public SpeakerData()
        {
            this.gid = string.Empty;
            this.speakerImageReverse = DialogueSpeakerImageReverse.None;
            this.speakerImageTurnBlack = false;
        }

        public SpeakerData(DialogueData dialogueData)
        {
            this.parent = dialogueData;
            this.gid = string.Empty;
            this.speakerImageReverse = DialogueSpeakerImageReverse.None;
            this.speakerImageTurnBlack = false;
        }
    }

    /// <summary>
    /// 對話事件選項類別
    /// </summary>
    public class DialogueEventData
    {
        public string desc;          // 事件選項描述
    }

    /// <summary>
    /// 對話事件選項類別 - Excel
    /// </summary>
    public class DialogueEventDataForExcel : DialogueEventData
    {
        public int type;             // 事件選項種類
        public string funcId;        // 事件選項ID
    }

    /// <summary>
    /// 對話事件選項類別 - Node
    /// </summary>
    public class DialogueEventDataForNode : DialogueEventData
    {
        public string inputGuid;     // 對話節點目標節點Guid

        public DialogueEventDataForNode()
        {
            this.inputGuid = string.Empty;
        }
    }
}

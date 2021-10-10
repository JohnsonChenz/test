//#define ENABLE_DIALOGUE_NODE_TYPE
#define ENABLE_DIALOGUE_EXCEL_TYPE

using EventSys;
using UnityEngine;

namespace DialogueSys
{
    public abstract class DialogueSystemBase
    {
        /// <summary>
        /// 對話json資料載入路徑
        /// </summary>
        public struct DialogueDBPath
        {
            public string dbPath;
        }

        /// <summary>
        /// 對話管理組件
        /// </summary>
        public DialogueManager dialogueManager { get; protected set; }

        /// <summary>
        /// 對話事件管理組件
        /// </summary>
        public EventCenterBase eventCenter { get; protected set; }

        // --- 對話系統相關路徑設置
        public DialogueDBPath dialogueDBPath;

        public DialogueSystemBase()
        {
            // 初始化對話管理組件
            this.dialogueManager = new DialogueManager(this);

            // 初始化子組件 (事件管理器等...)
            this._Init();
        }

        /// <summary>
        /// 初始化相關子組件
        /// </summary>
        protected virtual void _Init() { }
    }
}

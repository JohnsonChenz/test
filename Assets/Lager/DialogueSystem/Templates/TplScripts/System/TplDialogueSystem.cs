using CoreFrame.SceneFrame;
using DialogueSys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TplDialogueSystem : DialogueSystemBase
{
    private static TplDialogueSystem _instance;    // 對話系統單例

    public static TplDialogueSystem GetInstance()
    {
        if (_instance == null) _instance = new TplDialogueSystem();
        return _instance;
    }

    protected override void _Init()
    {
        // 以下會依照Define檔設定初始並設置對話管理器的對話驅動器
        if (DialogueSysDefine.bExcelType)
        {
            // 設置Excel Json資料路徑
            this.dialogueDBPath.dbPath = "DialogueDataBase/Excel/";

            // 轉型成DialogueExcelBase類別
            DialogueDriverExcel dialogueDriverExcel = this.dialogueManager.dialogueDriver as DialogueDriverExcel;
            // 調用其方法設置Excel Json資料名稱
            dialogueDriverExcel.SetDialogueExcelDB("dialogue_data", "dialogue_txt_data", "dialogue_event_data");
            // 設置文字大表JObject (選擇性)
            //dialogueDriverExcel.SetDialogueExcelStringDB(null);
        }
        if (DialogueSysDefine.bNodeType)
        {
            // 設置Excel Node資料路徑
            this.dialogueDBPath.dbPath = "DialogueDataBase/Node/";
        }

        // 初始化EventCenter組件
        this.eventCenter = EventCenterExample.GetInstance();
    }
}


using CoreFrame.SceneFrame;
using CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSys;
using TMPro;
using UnityEngine.UI;

public class DialogueDemo : MonoBehaviour
{
    /// <summary>
    /// 對話UI路徑
    /// </summary>
    public static class DialoguePathExample
    {
        public const string DialogueUIExample = "UI/DialogueUIExample"; // UI路徑
    }

    private bool doNotForceChooseEvent = false;                         // 是否不強迫選擇對話事件

    private void Start()
    {
        GameObject.Find("DriverType").GetComponent<TMP_Text>().text = DialogueSysDefine.bExcelType ? "Driver Type : Excel" : DialogueSysDefine.bNodeType ? "Driver Type : Node" : "Driver Type : None";
        this._InitEvents();
    }

    private void _InitEvents()
    {
        Toggle doNotForceChooseEventTgl = GameObject.Find("DoNotForceChooseEvent").GetComponent<Toggle>();
        doNotForceChooseEventTgl.onValueChanged.AddListener((isOn) =>
        {
            this.doNotForceChooseEvent = isOn;
        });
    }

    public void StartDialogueWithAutoCb()
    {
        DialogueSystemExample.GetInstance().dialogueManager.StartDialogue("DG0000100", DialoguePathExample.DialogueUIExample, () => { Debug.Log("<color=#FF0000>(Dialogue Debug)DialogueEndCallBack</color>"); }, false, this.doNotForceChooseEvent);
    }

    public void StartDialogueWithManualCb()
    {
        DialogueSystemExample.GetInstance().dialogueManager.StartDialogue("DG0000100", DialoguePathExample.DialogueUIExample, () => { Debug.Log("<color=#FF0000>(Dialogue Debug)DialogueEndCallBack</color>"); }, true, this.doNotForceChooseEvent);
    }

    public void StopDialogue(bool invokeEndCallback)
    {
        DialogueSystemExample.GetInstance().dialogueManager.StopDialogue(invokeEndCallback);
    }

    public void SkipDialogue()
    {
        DialogueSystemExample.GetInstance().dialogueManager.SkipDialogue();
    }

    public void InvokeEndCbManual()
    {
        DialogueSystemExample.GetInstance().dialogueManager.InvokeEndCbManual();
    }
}

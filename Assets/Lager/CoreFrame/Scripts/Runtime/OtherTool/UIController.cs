using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.UIFrame;

[AddComponentMenu("Lager/UIControl/UIController")]
public class UIController : MonoBehaviour
{
    [SerializeField, Tooltip("輸入UI資源路徑")]
    public string uiPathName = "";

    public void OnUIOpen()
    {
        UIManager.GetInstance().Show(this.uiPathName);
    }

    public void OnUIClose()
    {
        UIManager.GetInstance().Close(this.uiPathName);
    }
}

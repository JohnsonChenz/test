using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.UIFrame;

[AddComponentMenu("Lager/UIControl/UIFirstShow")]
public class UIFirstShow : MonoBehaviour
{
    [SerializeField, Tooltip("輸入UI資源路徑")]
    public string uiPathName = "";

    void Start()
    {
        UIManager.GetInstance().Show(this.uiPathName);
    }
}

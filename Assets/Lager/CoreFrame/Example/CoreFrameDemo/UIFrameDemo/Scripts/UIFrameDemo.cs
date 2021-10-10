using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.UIFrame;

public class UIFrameDemo : UIBase
{
    public static string Demo1UI = "Example/UI/Demo1UI";
    public static string Demo2UI = "Example/UI/Demo2UI";
    public static string Demo3UI = "Example/UI/Demo3UI";

    private void Awake()
    {

    }

    public async void ShowFirstUI()
    {
        await UIManager.GetInstance().Show(UIFrameDemo.Demo1UI);
    }
}

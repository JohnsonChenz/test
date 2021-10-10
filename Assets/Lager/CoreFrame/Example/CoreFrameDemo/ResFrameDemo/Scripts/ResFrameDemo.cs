using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.ResFrame;

public class ResFrameDemo : MonoBehaviour
{
    public static string DemoRS = "Example/Res/DemoRS";

    async void Start()
    {
        await ResManager.GetInstance().Show(ResFrameDemo.DemoRS);
    }
}

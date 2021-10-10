using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.SceneFrame;

public class SceneFrameDemo : MonoBehaviour
{
    public static string DemoSC = "Example/Scene/DemoSC";

    public async void PreloadSc()
    {
        await SceneManager.GetInstance().Preload(SceneFrameDemo.DemoSC);
    }

    public async void ShowSc()
    {
        await SceneManager.GetInstance().Show(SceneFrameDemo.DemoSC);
    }

    public void CloseSc()
    {
        SceneManager.GetInstance().Close(SceneFrameDemo.DemoSC);
    }

    public void CloseScWithDestroy()
    {
        SceneManager.GetInstance().Close(SceneFrameDemo.DemoSC, false, true);
    }

    public async void CloseAllSc()
    {
        await SceneManager.GetInstance().CloseAll();
    }

    public async void CloseAllScWithDestroy()
    {
        await SceneManager.GetInstance().CloseAll(false, true);
    }
}

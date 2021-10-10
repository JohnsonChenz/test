using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using CoreFrame.SceneFrame;

public class DemoPopup3UI : UIBase
{
    private Image myImage;
    private Button loadSceneBtn;

    public override void InitThis()
    {
        this.uiType = new UIType(UIFormType.Popup);
        this.maskType = new MaskType(UIMaskOpacity.OpacityHigh);
        this.isCloseAndDestroy = false;
    }

    protected override async UniTask OpenSub()
    {
        /**
        * Open Sub With Async
        */
    }

    protected override void CloseSub()
    {
        /**
        * Close Sub
        */
    }

    protected override void OnShow(object obj)
    {
        // Custom MaskEventFunc
        //this.maskEventFunc = () =>
        //{

        //};

        Debug.Log(string.Format("{0} - Do Something OnShow.", this.gameObject.name));
    }
    protected override void InitOnceComponents()
    {
        this.myImage = this.collector.GetNode("Image3")?.GetComponent<Image>();
        if (this.myImage != null) Debug.Log(string.Format("Binded GameObject: {0}", this.myImage.gameObject.name));

        this.loadSceneBtn = this.collector.GetNode("LoadSceneBtn")?.GetComponent<Button>();
        if (this.loadSceneBtn != null) Debug.Log(string.Format("Binded GameObject: {0}", this.loadSceneBtn.gameObject.name));
    }

    protected override void InitOnceEvents()
    {
        this.loadSceneBtn.onClick.AddListener(this._ShowDemoScene);
    }

    protected override void OnUpdate(float dt)
    {
        /**
         * Do Update Per FrameRate
         */
    }

    public override void OnUpdateOnceAfterProtocol(int funcId = 0)
    {
        /**
        * Do Update Once After Protocol Handle
        */
    }

    protected override void ShowAnim(AnimEndCb animEndCb)
    {
        animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void HideAnim(AnimEndCb animEndCb)
    {
        animEndCb(); // Must Keep, Because Parent Already Set AnimCallback
    }

    protected override void OnClose()
    {
        Debug.Log(string.Format("{0} - Do Something OnClose.", this.gameObject.name));
    }

    private void OnDestroy()
    {

    }

    private bool loadTrigger = false;
    private async void _ShowDemoScene()
    {
        this.loadTrigger = !this.loadTrigger;
        if (this.loadTrigger) await SceneManager.GetInstance().Show(SceneFrameDemo.DemoSC);
        else SceneManager.GetInstance().Close(SceneFrameDemo.DemoSC);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class DemoPopup1UI : UIBase
{
    private Image myImage;
    private Button oepnBtn;

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

        Debug.Log(string.Format("{0} Do Something OnShow.", this.gameObject.name));
    }

    protected override void InitOnceComponents()
    {
        this.myImage = this.collector.GetNode("Image1")?.GetComponent<Image>();
        if (this.myImage != null) Debug.Log(string.Format("Binded GameObject: {0}", this.myImage.name));

        this.oepnBtn = this.collector.GetNode("OpenBtn")?.GetComponent<Button>();
        if (this.oepnBtn != null) Debug.Log(string.Format("Binded GameObject: {0}", this.oepnBtn.name));
    }

    protected override void InitOnceEvents()
    {
        this.oepnBtn.onClick.AddListener(this._ShowDemoPopup2UI);
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

    private async void _ShowDemoPopup2UI()
    {
        await UIManager.GetInstance().Show(UIFrameDemo.Demo2UI);
    }
}

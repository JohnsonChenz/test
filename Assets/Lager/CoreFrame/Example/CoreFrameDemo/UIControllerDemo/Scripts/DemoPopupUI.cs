using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.UIFrame;
using Cysharp.Threading.Tasks;

public class DemoPopupUI : UIBase
{
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

        /**
         * Do Something Init With Every Showing In Here
         */
    }

    protected override void InitOnceComponents()
    {
        /**
         * Do Somthing Init Once In Here (For Components)
         */
    }

    protected override void InitOnceEvents()
    {
        /**
          * Do Somthing Init Once In Here (For Events)
          */
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

    }

    private void OnDestroy()
    {

    }
}

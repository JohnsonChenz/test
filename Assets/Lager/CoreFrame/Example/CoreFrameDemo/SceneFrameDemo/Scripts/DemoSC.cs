using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrame.SceneFrame;
using Cysharp.Threading.Tasks;

public class DemoSC : SceneBase
{
    public override void InitThis()
    {
        this.sceneType = new SceneType(SceneNodeType.Normal);
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

    protected override void OnClose()
    {

    }

    private void OnDestroy()
    {

    }
}

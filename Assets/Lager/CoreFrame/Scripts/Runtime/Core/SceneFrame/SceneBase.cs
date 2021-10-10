using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    namespace SceneFrame
    {
        public class SceneBase : FrameBase
        {
            [HideInInspector] public string scName = "";            // 用於存放ScenePath整個路徑名稱
            [HideInInspector] public SceneType sceneType = null;    // 定義Scene類型, 用於取決於要新增至SceneRoot中哪個對應的節點

            public override void InitThis() { }

            public sealed override void InitFirst()
            {
                base.InitFirst();
            }

            protected override async UniTask OpenSub()
            {
                await UniTask.Yield();
            }

            protected override void CloseSub() { }

            protected override void InitOnceComponents() { }

            protected override void InitOnceEvents() { }

            protected override void OnShow(object obj) { }

            public override void OnUpdateOnceAfterProtocol(int funcId = 0) { }

            protected override void OnUpdate(float dt) { }

            public sealed override void Display(object obj)
            {
                this.gameObject.SetActive(true);
                this.OnShow(obj);
            }

            public sealed override void Hide(bool disableDoSub = false)
            {
                if (!disableDoSub) this.CloseSub();
                this.OnClose();
                this.gameObject.SetActive(false);
            }

            protected sealed override void CloseSelf()
            {
                SceneManager.GetInstance().Close(this.scName);
            }
        }
    }
}
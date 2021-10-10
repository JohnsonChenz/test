using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrame
{
    public interface IFrameBase
    {
        void InitThis();

        void InitFirst();

        UniTask PreInit();

        void Display(object obj);

        void Hide(bool disableDoSub);

        void OnRelease();
    }
}
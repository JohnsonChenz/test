using CoreFrame.UIFrame;
using DialogueSys;
using KoganeUnityLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueUIExample : DialogueBaseUI
{
    /// <summary>
    /// 講者模型路徑Data模擬
    /// </summary>
    public Dictionary<string, string> modelPathData = new Dictionary<string, string>()
    {
          {"TestModel_1" ,"Models/Char/TestModel_1"},
          {"TestModel_2" ,"Models/Char/TestModel_2"},
          {"TestModel_3" ,"Models/Char/TestModel_3"}
    };

    protected override void OnSpeakerImageStatusChanged(Speaker speaker, SpeakerImageStatus speakerImageStatus)
    {
        switch (speakerImageStatus)
        {
            case SpeakerImageStatus.Black:

                /*
                 * Do something whlie speaker image is turning Black. For example :
                 */

                // 將階層位置設置最前 (即會顯示在最底下)
                speaker.speakerRoot.transform.SetAsFirstSibling();

                // 調整講者圖片顏色
                speaker.rawImage.color = new Color32(25, 25, 25, 255);

                // 播放TweenAnimation動畫
                speaker.speakerRoot.GetComponent<TweenAnimation>()?.PlayTween(false);

                break;
            case SpeakerImageStatus.Bright:

                /*
                 * Do something whlie speaker image is turning Bright. For example :
                 */

                // 將階層位置設置最後 (即會顯示在最上面)
                speaker.speakerRoot.transform.SetAsLastSibling();

                // 播放TweenAnimation動畫
                speaker.speakerRoot.GetComponent<TweenAnimation>()?.PlayTween(true);

                break;
        }
    }

    protected override string _ParsingModelPathName(SpeakerData speakerData)
    {
        return this.modelPathData[speakerData.gid];
    }
}
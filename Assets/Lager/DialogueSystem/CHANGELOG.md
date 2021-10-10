## CHANGELOG
---
### v1.3.6
- Adjusted : Variable "originSet(int) in DialogueCamStudio.cs is being set to 20000.

---
### v1.3.5
- Fixed the parent and position offset problem on Camstudio-Related prefab while it's being instantiated.
- Adjusted the pivot of speaker model prefab.
- Modified the name of folder "Concrete" to "Template".
- Dialogue System is **STOPPED using Cinemachine** and is back to use default camera in Unity to process Camstudio-Related camera instead.

### v1.3.4
- Fixes : DialogueDriverNode can't stop dialogue properly while boolean doNotForceChooseEvent is set to "True" in DialogueDriverBase.
---
### v1.3.3
- Fixes : Scale problem when instantiate on UI Element and CamStudio prefab.
- Adjusted :Private method "_SetSpeakerModel" in DialogueBaseUI.cs is changed to virtual method for derived class to override.
- Adjusted : (WIP) CamStudio is being planned to use Cinemachine to correct camera offset on dialogue speaker model.
- Added : Boolean parameter "doNotForceChooseEvent" on method "StartDialogue" in DialogueManager.cs, when set to "True", dialogue system won't force the user to choose event option to trigger event, instead, users is allowed to click anywhere on screen to end dialogue.

---
### v1.3.2
- Adjusted : Parameter in DialogueBaseUI's method "_ParsingModelPathName(string gid)" modified to "_ParsingModelPathName(SpeakerData speakerData)".
- Added : String Database function for DialogueDriverExcel.
    1. Set your String Database JObject into DialogueDriverExcel.
    ```csharp
    public class DialogueSystemExample : DialogueSystemBase
    {
        protected override void _Init()
        {
            if (DialogueSysDefine.bExcelType)
            {
                // Set String Database JObject into DialogueDriverExcel
                dialogueDriverExcel.SetDialogueExcelStringDB(StringDatabaseJObject);
            }
        }
    }
    ```
	2. If String Databse JObject is valid,when parsing dialogue data, DialogueContainerForExcel will get value from String Databse according to the StringCode that user filled in dialogue excel sheets. This change is made for users that using excel sheets to maintain text data in project.
	3. Example for String Database is being planned.
---
### v1.3.1
- Fixes : InvokeEndCbManual button doesn't work properly in DemoScene.
---
### v1.3.0
- Refactored DialogueDriver-Related scripts : 
    1. Integrated dialogue running method into DialogueDriverBase.cs such as _UpdateDialogue, _UpdateDialogueEventOptions.
	2. Added abstract method for derived class to implement, and will called by DialogueDriverBase.cs:
       ```csharp
	   protected override void _OnStart()
       {
           // This method will be called by parent class while current dialogue is successful init and about to start.
		   // Do some stuff here to start process your dialogue display.
       }

       protected override void _OnFinished()
       {
           // This method will be called by parent class while current dialogue is about to finish.
		   // Do some stuff here to finish current dialogue process.
       }

       protected override void _OnSetDialogueEventBtn(Button button, DialogueEventData dialogueEventData)
       {
           // This method will be called by parent class while current there are event options in current dialogue.
		   // Do some stuff to add event to button with dialogueEventData.
       }
	   ```
    3. Added virtual method for derived class to implement, and will called by DialogueDriverBase.cs:
       ```csharp
       protected override void _OnRelease()
       {
           // This method will be called by parent class while releasing memory of member variable.
		   // Do some stuff to release your declared member variable.
       }
	   ```
- EndCallback in dialogue system can now be called by manual :
    1. Added optional boolean parameter **manualInvokeCb** to method **StartDialogue** in **DialogueManager.cs**,**when set to true**,EndCallback **will not** auto handled while dialogue is finished. Instead, you will have to call method **"InvokeEndCbManual"** in DialogueManager.cs to invoke your custom dialogue EndCallback.
	   ```csharp
	   public class DialogueManager
       {
           public void StartDialogue(string dialogueId, string dialogueUIPath, Action endCb = null, bool manualInvokeCb = false)
	       
	       public void InvokeEndCbManual()
	   }
	   ```
	2. DemoScene is also updated for user to test these new functions.
- Adjusted DialogueBaseUI : Mask event will now handled by UIBase's mask function **MaskEvent**. 
- Fixes : Disable typewriter effect while dialogue is being skipped.
---
### v1.2.0
- Changed speaker model's loading method in order to remain consistent with loading method in dialogue-related resources :
    1. Removed loading type define of speaker model (SpeakerImageLoadType).
    2. Modified Excel sheet : Removed "speaker_model_type" in dialogue_data, and adjusted speaker_args in dialogue_txt_data.
    3. Modified Graphic Node Editor : object field is replaced with text field. Users will have to fill file's gid/name in it, and load it through customized path set by method in Dialogue UI.
- Modified DialogueBaseUI virtual methods :
    1. Added virtual method in DialogueBaseUI for subclasses to customize their speaker model load path. For Example:
       ```csharp
       public class DialogueUIExample : DialogueBaseUI
       {
           string pathExample = "Resources/Models/Chars/";
       	
           protected override string _ParsingModelPathName(string gid)
           {
       	    string parsedPath = pathExample + gid;
               return parsedPath;
           }
       }
       ```
	2. Added virtual method in DialogueBaseUI for subclasses to customize their dialogue audio play/load method.
	   ```csharp
	   public class DialogueUIExample : DialogueBaseUI
       {
           public override void SetAudio(string audioGid)
           {
               // Load your sound file by custom file path with audioGid and play your audio etc....
           }
	   }
       ```
- Added/Modified public method in DialogueManager.cs : 
    1. StartDialogue : Added callback parameter , this callback will handled after running dialogue has judged finished by Dialogue Driver.
       ```csharp
       public void StartDialogue(string dialogueId, string dialogueUIPath, Action endCallback = null)
       ```
    2. StopDialogue : Call this method through dialogue manager to stop your current running dialogue, and invoke EndCallback as you set in StartDialogue method.
       ```csharp
       public void StopDialogue(bool invokeEndCallback = true)
       ```
    3. SkipDialogue : Call this method through dialogue manager to skip your current running dialogue process.
       ```csharp
       public void StopDialogue(bool invokeEndCallback = true)
       ```
	4. DemoScene is updated for user to test these method.
- Refactored Dialogue Driver-related scripts :
    1. Removed IDialogueDriverBase, related method is moved into DialogueDriverBase.
	2. DialogueDriverNode/ExcelBase is renamed to DialogueDriverNode/Excel, Dialogue System will now use both for default dialogue driver,therefore,all subclasses script is Removed.
---
### v1.1.2
- Move dialogue excel json data define into DialogueDriverExcelBase. Set it up like below in your custom DialogueSystem script, or see DialogueSystemExample.cs provided in project.
```csharp
public class DialogueSystemExample : DialogueSystemBase
{
    protected override void _Init()
    {
        if (DialogueSysDefine.bExcelType)
        {
            DialogueDriverExcelExample dialogueDriverExcel = this.dialogueDriver as DialogueDriverExcelExample;
            dialogueDriverExcel.SetDialogueExcelDBDefine("dialogue_data", "dialogue_txt_data", "dialogue_event_data");
        }
    }
}
```
---
### v1.1.1
- Removed const members that are used to define related resource path in DialogueSystem in DialogueSysDefine.cs
- Added struct members for define related resource path in DialogueSystem in DialogueSystemBase.cs, example for how to claim them is provided in template scripts.
---
### v1.1.0
- Removed DialogueEvent-related scripts : dialogue event in dialogue system will handled by **EventCenter**, which is CoreFrame v1.4.1's new feature.
- Added duplicatable dialogue templates: TplDialogueUI.cs , TplDialogueUI.prefab , TplDialogueExcel.xls, TplDialogueSO.assets
- Removed interface scripts: ITMPTypeWritter, IDialogueBaseUI, related method is simplified and implemented in DialogueBaseUI.
- Added a method that will called by DialogueBaseUI while dialogue speaker's image status has changed.  This method is made to implement by subclasses, for example:
```csharp
public class DialogueUIExample : DialogueBaseUI
{
    protected override void OnSpeakerImageStatusChanged(Speaker speaker, SpeakerImageStatus speakerImageStatus)
    {
        switch (speakerImageStatus)
        {
            case SpeakerImageStatus.Black:
    
                /*
                 * Do something whlie speaker image is turning Black.
                 */
    
                break;
            case SpeakerImageStatus.Bright:
    
                /*
                 * Do something whlie speaker image is turning Bright.
                 */
    
                break;
        }
    }
}
```
- Refactorede dialogue driver(Node/Excel) scripts : Moving all of the implemented driver logic from subclasses to DialogueDriverExcel/NodeBase. 
- Removed meaningless namespace in scripts.
---
### v1.0.1
- Fixes build error on WebGL due to the using of GraphView api.
---
### v1.0.0
- Node & Excel based dialogue system.
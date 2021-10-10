# Node & Excel based dialogue system for Unity
A unity dialogue system that allows user create dialogue in game via graph node or excel sheets.

## Quick Installnation:
### Required for installation:
- Unity **2020.3.11** or higher
- CoreFrame **v1.4.1** or higher, view [CoreFrame release](http://192.168.1.139/lager-framework/unity_framework_dev/-/releases) here.
- UniTask **v2.2.5** or higher, view [UniTask release](https://github.com/Cysharp/UniTask/releases) here.
- TextMeshPro **v3.0.6** or higher, you can install it via Package Manager.
- My Box **v1.7.0** or higher, you can install it via git url : https://github.com/Deadcows/MyBox.git
- DOTween free(HOTween v2)/pro version **v1.2.632** or higher, you can install it via Asset Store.
### Recommend for installation:
- ExcelConverter **v1.1.5** or higher for Excel converting to json, view [ExcelConverter release](http://192.168.1.139/lager-framework/unity_plugins_dev/-/releases) here.


## Features:
- Create custom dialogue by editing graph node(ScriptableObject) or excel sheets.
- Save/Load (both Json and ScriptableObject) system in graph editor.
- Fully json serialize/deserialize of graph node data.
- Backed by Unity's embedded GraphView api.
- Search window for node creation.
- Allow using sprite or 2/3D model for dialogue speaker display.
- Auto save to json when saving graph node data into ScriptableObjects.
- Sample provided in the package.

## Create dialogue graph node ScriptableObjects:
- Right click **"Dialogue -> New Dialogue"** to create dialogue node ScriptableObject, double click to open then create and edit node in GraphView window.  
- Right click **"Dialogue -> New Dialogue Event"** to create dialogue event ScriptableObject.

## Excel for dialogue system
- Please look format example provided in package ("Example/MiscResources/DialogueExcelExample/")
- Contains 3 sheets (classes):
- **dialogue_data**: Stores group of event data, txt data, and resources type of speaker image.
- **dialogue_txt_data**: Stores dialogue text content, speaker args, speaker name etc..
- **dalogue_event_data**: Stores event explanation, id,and type.
- We RECOMMEND your using ExcelConverter for excel converting to json (i'm serious) (or not).

## Dialogue Events
- After V1.1.0 update, events in dialogue system will handle by **EventCenter**. Featured in **CoreFrame v1.4.1**,**EventCenter** is made to register and handle custom system events, for more infomation about EventCenter, view [CoreFrame release](http://192.168.1.139/lager-framework/unity_framework_dev/-/releases) here.

## DialogueDataContainerForExcel
- DialogueDataContainer For Excel is a class that holds dialogue group, text, event data for excel dialogue json data,for more infomation about excel json data please look **Excel for dialogue system** above.

## DialogueDataContainerForNode
- DialogueDataContainer For Node is a serialized class that holds dialogue node data for graph (Start Node, Dialogue Node, EventStr Node, EventSO Node, End Node) and can fully serialized/deserialized to/from json.

## DialogueDriverForNode/Excel
- A class that runs node or excel dialogue data and push it into UI display.
- You can switch what type of the driver you want to execute by changing #define identifier at the top of DialogueSysDefine.cs.

## DialogueSystemBase
- A base class that controls/init manager or other custom components in dialogue system, 
- create your own derived class to init or access your custom dialogue related components.

## DialogueManager
- Init by DialogueSystemBase, DialogueManager is made to init,access DialogueDataBase and DialogueDriver, and push database into the latter to run dialogue data and show UI display.

## DialogueDataBase
- Init by DialogueManager, DialogueDataBase is made to load dialogue json database file from disk and store it into cache

## DialogueBaseUI
- A derived class that inherits CoreFrame UIBase,provides dialogue data display methods for derived class to override.

## DialogueSysDefine
- A script that defines system settings or declare static classes.

## License
This library is under the MIT License.
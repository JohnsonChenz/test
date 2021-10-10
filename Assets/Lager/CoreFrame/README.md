## Basic Architecture
![](http://192.168.1.139/lager-framework/unity_framework_dev/raw/upm_CoreFrame/cf_desc_img.PNG)

## Quick Install
### Required for installation
- Unity 2020.3 or higher
- Needs [UnitTask Version 2.2.5 or higher](https://github.com/Cysharp/UniTask), install first.

## CoreFrame Features
### UIFrame
- Auto generate the UIRoot in canvas.
- UIFrame has 7 types of UI Layer to manage all UI for rendering priority (Normal -> Fixed -> Popup -> IndiePopup -> Independent -> SysPopup -> SysMsg), basically every UI Layer has sorting order -1000 to -7000 【Bottom to Top corresponding to Normal to SysMsg】, also can custom your UI Layers whatever you want.
- Only Popup Type uses stack cache mode to control the display order, in contrast Non-Popup Types are Independent cache.
- The UIManager is core will be used as a singleton to control UI Preload, Loading, Show, Close and Destroy and so on.
- Auto bind GameObject with specific character 【_Node@Name of GameObject】 into the collector.
- Popup Type has UIMask to generate by automatically, also can custom MaskClickEvent () => \{\} by delegate.
- UI has freeze feature on Show and Close, the main reason is to avoid the UI animation has not finished playing.

### SceneFrame
- Auto generate the SceneRoot in game scene.
- Auto to manage every scene prefab with cache by SceneFrame, the SceneManager is core will be used as a sigleton to control Scene Preload, Loading, Show, Close and Destroy and so on.
- Auto bind GameObject with specific character 【_Node@Name of GameObject】 into the collector.

### ResFrame
- Auto generate the ResRoot in game scene.
- Auto to manage every res (resources) prefab with cache by ResFrame, the ResManager is core will be used as a sigleton to control Res Preload, Loading, Show, Close and Destroy and so on.
- Auto bind GameObject with specific character 【_Node@Name of GameObject】 into the collector.

### Template
- In [v1.2.0 or higher](http://192.168.1.139/lager-framework/unity_framework_dev/-/releases) has new feature for Editor, can create a template script and template prefab by right click.

### Remarks
- The Examples aspect ratio are 1080 x 1920 (2K Portrait) and use Unity New InputSystem.
- Recommended to use OnShow() instead of OnEnable().
- Recommended to use OnClose() instead of OnDisable().

### EventCenter
- EventerCenter can help to manage events, It's use map by hex funcId to cached event.

## License
This library is under the MIT License.
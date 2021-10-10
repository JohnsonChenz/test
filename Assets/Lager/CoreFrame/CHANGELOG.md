## CHANGELOG

### v1.4.1
- Modified EventBase HandleEvent with async method.

### v1.4.0
- Add new EventCenter to help manage events.

### v1.3.4
- Modified UIFrame MaskEvent by virtual function, allow inherit class to override with clear method.
- Modified CloseSub with check condition when Manager use Close or CloseAll.

### v1.3.3
- Fix CoreHelper CachedResource when add to cache will be skipping duplicate keys.

### v1.3.2
- Fix SysPopup when load has wrong reference problem.

### v1.3.1
- Fix Close method bug.
- Modified string path name with safety check.

### v1.3.0
- Rename InitByShowing with OnShow more clearly.
- Remove AsyncLoadByShowing() method.
- Implement class FrameManager<T>, separate common methods.
- Modified ResManager, SceneManager, UIManager to inherit FrameManager<T>.
- Add OnRelease() to use it instead of OnDestroy(). 
- Remove method of specific name from all of manager.

### v1.2.0
- New feature for Editor, can create a template script and template prefab by right click.
- Add new feature for ResFrame and SceneFrame there are two types to create. The types are Normal and Permanet, when you invoke CloseAll method will auto to skip type of Permanet.
- Modified CloseAll method with params which to skip close or destroy.
- Remove Template folder. Because has new feature for Editor can Auto create by right click instead of manual create.
- Modified Example.

### v1.1.0
- Add OnClose() to use it instead of OnDisable(), because avoid stop game in Unity Editor onDisable() will be called.
- Modified and fix Template scripts replaced OnDisable() with OnClose().
- Modified Examples to be clearly show and close status.

### v1.0.0
- UIFrame
- SceneFrame
- ResFrame
- Auto bind GameObject with specific character 【_Node@Name of GameObject】 into the collector.
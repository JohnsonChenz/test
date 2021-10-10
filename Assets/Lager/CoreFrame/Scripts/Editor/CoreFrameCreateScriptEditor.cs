using UnityEngine;
using UnityEditor;

public static class CoreFrameCreateScriptEditor
{
    // ResFrame
    private const string TPL_NORMAL_RS_PATH = "TplScripts/ResFrame/TplNormalRS.cs.txt";
    private const string TPL_PERMANENT_RS_PATH = "TplScripts/ResFrame/TplPermanentRS.cs.txt";

    // SceneFrame
    private const string TPL_NORMAL_SC_PATH = "TplScripts/SceneFrame/TplNormalSC.cs.txt";
    private const string TPL_PERMANENT_SC_PATH = "TplScripts/SceneFrame/TplPermanentSC.cs.txt";

    // UIFrame
    private const string TPL_NORMAL_UI_PATH = "TplScripts/UIFrame/TplNormalUI.cs.txt";
    private const string TPL_FIXED_UI_PATH = "TplScripts/UIFrame/TplFixedUI.cs.txt";
    private const string TPL_POPUP_UI_PATH = "TplScripts/UIFrame/TplPopupUI.cs.txt";
    private const string TPL_INDIE_POPUP_UI_PATH = "TplScripts/UIFrame/TplIndiePopupUI.cs.txt";
    private const string TPL_INDEPENDENT_UI_PATH = "TplScripts/UIFrame/TplIndependentUI.cs.txt";
    private const string TPL_SYS_POPUP_UI_PATH = "TplScripts/UIFrame/TplSysPopupUI.cs.txt";
    private const string TPL_SYS_MSG_UI_PATH = "TplScripts/UIFrame/TplSysMsgUI.cs.txt";

    // find current file path
    private static string pathFinder
    {
        get
        {
            var g = AssetDatabase.FindAssets("t:Script CoreFrameCreateScriptEditor");
            return AssetDatabase.GUIDToAssetPath(g[0]);
        }
    }

    #region ResFrame Script Create
    [MenuItem(itemName: "Assets/Create/CoreFrame/ResFrame/TplScripts/TplNormalRS", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplNormalRS()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_NORMAL_RS_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplNormalRS.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/ResFrame/TplScripts/TplPermanentRS", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplPermanentRS()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_PERMANENT_RS_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplPermanentRS.cs");
    }
    #endregion

    #region SceneFrame Script Create
    [MenuItem(itemName: "Assets/Create/CoreFrame/SceneFrame/TplScripts/TplNormalSC", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplNormalSC()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_NORMAL_SC_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplNormalSC.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/SceneFrame/TplScripts/TplPermanentSC", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplPermanentSC()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_PERMANENT_SC_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplPermanentSC.cs");
    }
    #endregion

    #region UIFrame Script Create
    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplNormalUI", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplNormalUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_NORMAL_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplNormalUI.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplFixedUI", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplFixedUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_FIXED_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplFixedUI.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplPopupUI", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplPopupUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_POPUP_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplPopupUI.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplIndiePopupUI", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplIndiePopupUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_INDIE_POPUP_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplIndiePopupUI.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplIndependentUI", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplIndependentUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_INDEPENDENT_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplIndependentUI.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplSysPopupUI (Permanent)", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplSysPopupUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_SYS_POPUP_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplSysPopupUI.cs");
    }

    [MenuItem(itemName: "Assets/Create/CoreFrame/UIFrame/TplScripts/TplSysMsgUI (Permanent)", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplSysMsgUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_SYS_MSG_UI_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplSysMsgUI.cs");
    }
    #endregion
}
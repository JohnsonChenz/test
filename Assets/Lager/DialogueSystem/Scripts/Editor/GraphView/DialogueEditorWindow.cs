using DialogueSys;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class DialogueEditorWindow : EditorWindow
{
    private DialogueDataContainerForNode currentDialogueContainer; // 當前的視覺化對話容器
    private DialogueGraphView graphView;                           // 視覺化GraphView視窗
    private DialogueNodeSaveAndLoad saveAndLoad;                   // 視覺化節點讀寫器
    private string currentDialogueContainerAssetPath;

    private VisualElement settingWindow;                           // 拖曳式設定訂單
    private ToolbarMenu toolbarMenuSaveAs;                         // 另存新檔下拉式選單
    private ToolbarMenu toolbarMenuLoadFrom;                       // 讀檔下拉式選單
    private Label nameOfDialogurContainer;                         // 當前開啟的檔案名稱

    // --- EditorPref相關設定Key
    private const string keyAutoSaveJsonPath = "KeyAutoSaveJsonPath";
    private const string keySaveAsJsonPath = "KeySaveAsJsonPath";
    private const string keySaveAsSOPath = "KeySaveAsSOPath";
    private const string keyLoadFromJsonPath = "KeyLoadFromJsonPath";
    private const string keyLoadFromSOPath = "KeyLoadFromSOPath";
    private const string keyAutoSaveToJsonBool = "KeyAutoSaveToJsonBool";

    private void OnEnable()
    {
        // --- 建構相關編輯視窗，並讀取解析資料顯示在編輯器上
        this._ConstructGraphView();
        this._GenerateToolBar();
        this._GenerateSettingWindow();
        this._Load();
    }

    private void OnInspectorUpdate()
    {

    }

    private void OnDisable()
    {
        this.rootVisualElement.Remove(this.graphView);
    }

    [OnOpenAsset(1)]
    public static bool ShowWindow(int instanceId, int line)
    {
        // 透過InstanceID取得物件
        UnityEngine.Object item = EditorUtility.InstanceIDToObject(instanceId);

        // 如果物件類別為視覺化對話容器
        if (item is DialogueDataContainerForNode)
        {
            // --- 建構視覺化編輯器視窗，並讀取解析資料顯示在編輯器上
            DialogueEditorWindow window = (DialogueEditorWindow)GetWindow(typeof(DialogueEditorWindow));
            window.titleContent = new GUIContent("Dialogue Editor");
            window.currentDialogueContainer = item as DialogueDataContainerForNode;
            window.currentDialogueContainerAssetPath = AssetDatabase.GetAssetPath(item);
            window.minSize = new Vector2(500, 250);
            window._Load();
        }

        return false;
    }

    /// <summary>
    /// 建構GraphView視窗
    /// </summary>
    private void _ConstructGraphView()
    {
        this.graphView = new DialogueGraphView(this);
        this.graphView.StretchToParentSize();
        this.rootVisualElement.Add(this.graphView);
        this.saveAndLoad = new DialogueNodeSaveAndLoad(this.graphView);
    }

    /// <summary>
    /// 建構工具列
    /// </summary>
    private void _GenerateToolBar()
    {
        // 添加StyleSheet
        StyleSheet styleSheet = Resources.Load<StyleSheet>("GraphViewStyleSheet");
        rootVisualElement.styleSheets.Add(styleSheet);

        Toolbar toolbar = new Toolbar();

        // 儲存當前檔案按鈕
        Button saveBtn = new Button()
        {
            text = "Save"
        };
        saveBtn.clicked += () =>
        {
            this._Save();
        };

        toolbar.Add(saveBtn);

        // 另存新檔下拉式選單
        this.toolbarMenuSaveAs = new ToolbarMenu()
        {
            text = "Save As"
        };

        this.toolbarMenuSaveAs.menu.AppendAction("Json", new Action<DropdownMenuAction>(x =>
        {
            string path = EditorUtility.SaveFilePanel("Save As Json", EditorPrefs.GetString(keySaveAsJsonPath, Application.dataPath), this.currentDialogueContainer.name + "", "json");

            if (path.IsNullOrZeroEmpty()) return;

            EditorPrefs.SetString(keySaveAsJsonPath, this._GetCorrectedPathWithoutFileName(path));

            this.SaveAs(path);
        }));

        this.toolbarMenuSaveAs.menu.AppendAction("ScriptableObject", new Action<DropdownMenuAction>(x =>
        {
            string path = EditorUtility.SaveFilePanelInProject("Save As ScriptableObject", this.currentDialogueContainer.name + "", "asset", "", EditorPrefs.GetString(keySaveAsSOPath, Application.dataPath));

            if (path.IsNullOrZeroEmpty()) return;

            EditorPrefs.SetString(keySaveAsSOPath, this._GetCorrectedPathWithoutFileName(path));

            this.SaveAs(path);
        }));

        toolbar.Add(this.toolbarMenuSaveAs);

        // 讀檔下拉式選單
        this.toolbarMenuLoadFrom = new ToolbarMenu()
        {
            text = "Load From"
        };

        this.toolbarMenuLoadFrom.menu.AppendAction("Json", new Action<DropdownMenuAction>(x =>
        {
            string path = EditorUtility.OpenFilePanel("Load From Json", EditorPrefs.GetString(keyLoadFromJsonPath, Application.dataPath), "json");

            if (path.IsNullOrZeroEmpty()) return;

            EditorPrefs.SetString(keyLoadFromJsonPath, this._GetCorrectedPathWithoutFileName(path));

            this._LoadFileFromJson(path);
        }));

        this.toolbarMenuLoadFrom.menu.AppendAction("ScriptableObject", new Action<DropdownMenuAction>(x =>
        {
            string path = EditorUtility.OpenFilePanel("Load From ScriptableObject", EditorPrefs.GetString(keyLoadFromSOPath, Application.dataPath), "asset");

            if (path.IsNullOrZeroEmpty()) return;

            path = path.Replace(Application.dataPath, "Assets");

            EditorPrefs.SetString(keyLoadFromSOPath, this._GetCorrectedPathWithoutFileName(path));

            this._LoadFileFromSO(path);
        }));

        toolbar.Add(this.toolbarMenuLoadFrom);

        // 拖曳式設定選單按鈕開關
        Button settingWindowSwitcher = new Button()
        {
            text = "Setting Window"
        };

        settingWindowSwitcher.clickable.clicked += () =>
        {
            this.settingWindow.visible = !this.settingWindow.visible;
        };

        toolbar.Add(settingWindowSwitcher);

        // 當前開啟的對話So檔案名稱
        this.nameOfDialogurContainer = new Label("");
        toolbar.Add(this.nameOfDialogurContainer);
        this.nameOfDialogurContainer.AddToClassList("nameOfDialogurContainer");

        this.rootVisualElement.Add(toolbar);
    }

    /// <summary>
    /// 產生可拖曳的設定視窗
    /// </summary>
    private void _GenerateSettingWindow()
    {
        // 產生VisualElement容器
        this.settingWindow = new VisualElement();

        // 添加StyleSheet
        StyleSheet styleSheet = Resources.Load<StyleSheet>("SettingWindowStyleSheet");
        this.settingWindow.styleSheets.Add(styleSheet);

        // 添加StyleSheet屬性
        this.settingWindow.AddToClassList("mainContainer");

        rootVisualElement.Add(this.settingWindow);

        // 自動儲存Json開關
        Toggle autoSaveToJsonToggle = new Toggle()
        {
            text = "AutoSaveToJson"
        };
        autoSaveToJsonToggle.AddToClassList("toggle");
        autoSaveToJsonToggle.SetValueWithoutNotify(EditorPrefs.GetBool(keyAutoSaveToJsonBool));
        autoSaveToJsonToggle.RegisterValueChangedCallback(value =>
        {
            EditorPrefs.SetBool(keyAutoSaveToJsonBool, value.newValue);
        });

        this.settingWindow.Add(autoSaveToJsonToggle);

        // 自動儲存Json檔案路徑
        TextField jsonSavePathTextField = new TextField();
        jsonSavePathTextField.SetValueWithoutNotify(EditorPrefs.GetString(keyAutoSaveJsonPath));

        this.settingWindow.Add(jsonSavePathTextField);

        // 自動儲存Json檔案路徑瀏覽按鈕
        Button browseAutoSaveJsonPathBtn = new Button()
        {
            text = "Browse"
        };
        browseAutoSaveJsonPathBtn.clickable.clicked += () =>
        {
            string path = EditorUtility.OpenFolderPanel("Choose Json Save Path", EditorPrefs.GetString(keyAutoSaveJsonPath), "");

            if (path.IsNullOrZeroEmpty()) return;

            EditorPrefs.SetString(keyAutoSaveJsonPath, path);

            jsonSavePathTextField.SetValueWithoutNotify(EditorPrefs.GetString(keyAutoSaveJsonPath));
        };

        this.settingWindow.Add(browseAutoSaveJsonPathBtn);

        // 初始化拖曳器，並將VisualElement容器設定為拖曳目標
        UIElementDragger textureDragger = new UIElementDragger(this.settingWindow);
        textureDragger.parentVisualElement = this.rootVisualElement;
        textureDragger.OffsetMaxDistanceFromTop = 21;
        textureDragger.OffsetMaxDistanceFromLeft = 0;
    }

    #region Load

    /// <summary>
    /// 讀取並顯示資料，用於點擊開啟ScriptableObject初始顯示資料用。
    /// </summary>
    private void _Load()
    {
        if (this.currentDialogueContainer != null)
        {
            this.nameOfDialogurContainer.text = "Name: " + this.currentDialogueContainer.name;
            this.saveAndLoad.Load(this.currentDialogueContainer);
        }
    }

    /// <summary>
    /// 讀取Json功能，透過反序列化Json資料，獲得視覺化對話容器資料並顯示
    /// </summary>
    /// <param name="path">Json檔案路徑</param>
    private void _LoadFileFromJson(string path)
    {
        string json = File.ReadAllText(path);
        if (json.IsNullOrZeroEmpty()) return;
        DialogueDataContainerForNode dialogueDataContainerForNode = CreateInstance<DialogueDataContainerForNode>();
        try
        {
            JsonUtility.FromJsonOverwrite(json, dialogueDataContainerForNode);
        }
        catch
        {
            Debug.LogError("Json檔案讀取出錯!! 請檢查檔案格式是否正確!!");
            return;
        }

        this.saveAndLoad.Load(dialogueDataContainerForNode);
    }

    /// <summary>
    /// 讀取ScriptableObject功能，直接Load Asset轉型取得視覺化對話容器資料並顯示
    /// </summary>
    /// <param name="path">ScriptableObject檔案路徑</param>
    private void _LoadFileFromSO(string path)
    {
        DialogueDataContainerForNode dialogueDataContainerForNode = AssetDatabase.LoadAssetAtPath<DialogueDataContainerForNode>(path);
        if (dialogueDataContainerForNode == null) return;

        this.saveAndLoad.Load(dialogueDataContainerForNode);
    }
    #endregion

    #region Save

    /// <summary>
    /// 另存新檔功能
    /// </summary>
    /// <param name="path">存檔路徑(需含有副檔名)</param>
    private void SaveAs(string path)
    {
        // 實例化一個視覺化對話容器
        DialogueDataContainerForNode asset = CreateInstance<DialogueDataContainerForNode>();

        // 將當前顯示在GraphView上的節點及連線資訊寫入到容器中
        this.saveAndLoad.Save(asset);

        // 存成Json
        if (path.Contains(".json"))
        {
            // 直接序列化成Json字串
            string json = JsonUtility.ToJson(asset);
            // 寫入檔案到路徑
            File.WriteAllText(path, json);
            // 刷新Asset
            AssetDatabase.Refresh();
        }
        // 存成Scriptable Object
        else if (path.Contains(".asset"))
        {
            // 判定如果有重複檔案就刪除該重複檔案，為防止出現一個重複寫入SO的Bug (有點難解釋，真的想知道再請來問我 Thanks)
            if (File.Exists(path))
            {
                // 刪除該重複檔案
                AssetDatabase.DeleteAsset(path);

                // 如果刪到當前開啟的檔案，重新指定當前視覺化對話容器的記憶體到新實例化的視覺化對話容器，防止抓取不到記憶體
                if (path == this.currentDialogueContainerAssetPath)
                {
                    this.currentDialogueContainer = asset;
                }
            }
            // 產生新Asset
            AssetDatabase.CreateAsset(asset, path);
            // 將物件標記為已修改，通知編輯器將資料寫入到硬碟
            EditorUtility.SetDirty(asset);
            // 儲存新Asset
            AssetDatabase.SaveAssets();
            // 刷新Asset
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 存檔功能
    /// </summary>
    private void _Save()
    {
        if (this.currentDialogueContainer != null)
        {
            // 將當前顯示在GraphView上的節點及連線資訊寫入到容器中
            this.saveAndLoad.Save(this.currentDialogueContainer);

            // 將物件標記為已修改，通知編輯器將資料寫入到硬碟
            EditorUtility.SetDirty(this.currentDialogueContainer);

            // 儲存Asset
            AssetDatabase.SaveAssets();

            // 刷新Asset
            AssetDatabase.Refresh();

            // 自動存檔Json功能
            if (EditorPrefs.GetBool(keyAutoSaveToJsonBool))
            {
                this.SaveAs(EditorPrefs.GetString(keyAutoSaveJsonPath) + "/" + this.currentDialogueContainer.name + ".json");
            }
        }
    }
    #endregion

    private string _GetCorrectedPathWithoutFileName(string path)
    {
        if (path == null) return null;

        int index = path.LastIndexOf("/");

        return path.Substring(0, index + 1);
    }
}


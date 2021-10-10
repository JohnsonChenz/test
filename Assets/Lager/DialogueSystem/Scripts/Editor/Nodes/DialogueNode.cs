using DialogueSys;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 講者Box容器顯示
/// </summary>
public class SpeakerInfoBox
{
    public Box box;                       // 主要存放容器
    public TextField gid;                 // 模型GID文字區域
    public EnumField speakerImageReverse; // 講者圖片反轉選單區域
    public Toggle speakerImageTurnBlack;  // 講者圖片褪黑開關Toggle

    public SpeakerInfoBox()
    {
        // 初始化主存放容器
        this.box = new Box();
        // 添加StyleSheet屬性
        this.box.AddToClassList("speakerInfoBox");
        // 模型ID文字區域
        this.gid = new TextField();
        // 添加StyleSheet屬性
        this.gid.AddToClassList("speakerInfoTextField");
        // 初始化講者圖片反轉選單區域
        this.speakerImageReverse = new EnumField();
        // 初始化講者圖片褪黑開關Toggle
        this.speakerImageTurnBlack = new Toggle()
        {
            text = "Turn Black"
        };
    }

    /// <summary>
    /// 刷新講者Box容器顯示
    /// </summary>
    /// <param name="dialogueSpeakerInfoType"></param>
    public void RefreshBoxElement()
    {
        // 清空主存放容器
        this.box.Clear();

        // 以下添加UI元素到Box

        // 模型名稱標題
        Label labelModelGid = new Label("Model GID:");
        labelModelGid.AddToClassList("speakerInfoLabel");
        labelModelGid.AddToClassList("Label");
        this.box.Add(labelModelGid);
        // 模型名稱文字欄位
        this.box.Add(this.gid);
        // 圖片反轉標題
        Label labelImageReverse = new Label("Reverse:");
        labelImageReverse.AddToClassList("speakerInfoLabel");
        labelImageReverse.AddToClassList("Label");
        this.box.Add(labelImageReverse);
        // 圖片反轉選擇欄位
        this.box.Add(this.speakerImageReverse);
        // 圖片褪黑標題
        Label labelImageTurnBlack = new Label("TurnBlack:");
        labelImageTurnBlack.AddToClassList("speakerInfoLabel");
        labelImageTurnBlack.AddToClassList("Label");
        this.box.Add(labelImageTurnBlack);
        // 圖片褪黑勾選欄位
        this.box.Add(this.speakerImageTurnBlack);
    }

    /// <summary>
    /// 初始化UI組件
    /// </summary>
    /// <param name="speakerData">講者資料參數</param>
    public void _InitCompponents(SpeakerData speakerData)
    {
        this.speakerImageReverse.Init(speakerData.speakerImageReverse);
    }

    /// <summary>
    /// 初始化UI點擊或Callback事件
    /// </summary>
    /// <param name="speakerData">講者資料參數</param>
    public void InitEvents(SpeakerData speakerData)
    {
        this.gid.RegisterValueChangedCallback((value) =>
        {
            speakerData.gid = value.newValue;
        });

        this.speakerImageReverse.RegisterValueChangedCallback((value) =>
        {
            speakerData.speakerImageReverse = (DialogueSpeakerImageReverse)value.newValue;
        });

        this.speakerImageTurnBlack.RegisterValueChangedCallback((value) =>
        {
            speakerData.speakerImageTurnBlack = value.newValue;
        });
    }

    /// <summary>
    /// 讀取資料參數，更新到UI顯示
    /// </summary>
    /// <param name="speakerData">講者資料參數</param>
    public void LoadValueIntoField(SpeakerData speakerData)
    {
        this.gid.SetValueWithoutNotify(speakerData.gid);
        this.speakerImageReverse.SetValueWithoutNotify(speakerData.speakerImageReverse);
        this.speakerImageTurnBlack.SetValueWithoutNotify(speakerData.speakerImageTurnBlack);
    }
}

/// <summary>
/// 單一對話顯示容器
/// </summary>
public class DialogueBox
{

    public VisualElement container;             // 主要UI元素存放容器                                                         
    private TextField textsField;               // 對話內容文字區域
    private TextField audioIdField;             // 音效Id文字區域
    private TextField nameField;                // 講者名稱文字區域
    public Button deleteBtn;                    // 刪除對話容器按鈕
    public Button duplicateBtn;                 // 複製對話容器按鈕
    public Button upBtn;                        // 對話容器上移按鈕
    public Button downBtn;                      // 對話容器下移按鈕

    private VisualElement speakerInfoContainer; // 講者資訊顯示容器存放容器
    private VisualElement buttonContainer;      // 功能按鈕存放容器

    /// <summary>
    /// 三位講者顯示容器
    /// </summary>
    public SpeakerInfoBox[] arrSpeakerInfoBox = new SpeakerInfoBox[3]
    {
                new SpeakerInfoBox(),
                new SpeakerInfoBox(),
                new SpeakerInfoBox()
    };

    public DialogueBox()
    {
        // 單一對話顯示容器
        this.container = new VisualElement();
        this.container.AddToClassList("dialogueContainer");

        // 功能按鈕存放容器
        this.buttonContainer = new VisualElement();
        this.buttonContainer.AddToClassList("buttonContainer");

        // 刪除對話容器按鈕
        this.deleteBtn = new Button()
        {
            text = "X"
        };

        this.deleteBtn.AddToClassList("Label");

        // 複製對話容器按鈕
        this.duplicateBtn = new Button()
        {
            text = "Duplicate"
        };

        this.duplicateBtn.AddToClassList("Label");

        // 上移對話容器按鈕
        this.upBtn = new Button()
        {
            text = "▲"
        };

        this.upBtn.AddToClassList("Label");

        // 下移對話容器按鈕
        this.downBtn = new Button()
        {
            text = "▼"
        };

        this.downBtn.AddToClassList("Label");

        // 添加各功能按鈕到對話容器中
        this.buttonContainer.Add(this.upBtn);
        this.buttonContainer.Add(this.downBtn);
        this.buttonContainer.Add(this.duplicateBtn);
        this.buttonContainer.Add(this.deleteBtn);

        // 將功能按鈕容器添加到主顯示容器中
        this.container.Add(this.buttonContainer);

        // 講者欄位Layout初始
        this.speakerInfoContainer = new VisualElement();
        this.speakerInfoContainer.AddToClassList("speakerInfoContainer");

        this.container.Add(this.speakerInfoContainer);

        // 音效Clip欄位
        Label labelAudio = new Label("Audio GID:");
        labelAudio.AddToClassList("label_texts");
        labelAudio.AddToClassList("Label");

        this.audioIdField = new TextField("");

        this.container.Add(labelAudio);
        this.container.Add(this.audioIdField);

        // 講者名稱文字欄位
        Label labelName = new Label("Name:");
        labelName.AddToClassList("label_name");
        labelName.AddToClassList("Label");

        this.nameField = new TextField("");
        this.nameField.AddToClassList("TextName");

        this.container.Add(labelName);
        this.container.Add(this.nameField);

        // 文字方塊欄位
        Label labelTexts = new Label("Texts:");
        labelTexts.AddToClassList("label_texts");
        labelTexts.AddToClassList("Label");

        this.textsField = new TextField("");
        this.textsField.multiline = true;
        this.textsField.AddToClassList("TextBox");

        this.container.Add(labelTexts);
        this.container.Add(this.textsField);
    }

    /// <summary>
    /// UI組件相關按鈕事件或Callback初始
    /// </summary>
    /// <param name="dialogueData">單一對話資料</param>
    public void InitEvents(DialogueData dialogueData)
    {
        this.audioIdField.RegisterValueChangedCallback(value =>
        {
            dialogueData.audioId = value.newValue;
        });

        this.nameField.RegisterValueChangedCallback(value =>
        {
            dialogueData.speakerName = value.newValue;
        });

        this.textsField.RegisterValueChangedCallback(value =>
        {
            dialogueData.content = value.newValue;
        });
    }

    /// <summary>
    /// 更新資料顯示到UI組件
    /// </summary>
    /// <param name="dialogueData">單一對話資料</param>
    public void LoadValueIntoField(DialogueData dialogueData)
    {
        this.audioIdField.SetValueWithoutNotify(dialogueData.audioId);
        this.nameField.SetValueWithoutNotify(dialogueData.speakerName);
        this.textsField.SetValueWithoutNotify(dialogueData.content);
    }

    /// <summary>
    /// 初始化三位講者顯示容器
    /// </summary>
    /// <param name="dialogueData"></param>
    /// <param name="dialogueSpeakerType"></param>
    public void InitSpeakerInfoBoxes(DialogueData dialogueData)
    {
        for (int i = 0; i < this.arrSpeakerInfoBox.Length; i++)
        {
            this.speakerInfoContainer.Add(this.arrSpeakerInfoBox[i].box);
            this.arrSpeakerInfoBox[i].RefreshBoxElement();
            this.arrSpeakerInfoBox[i]._InitCompponents(dialogueData.speakerDatas[i]);
            this.arrSpeakerInfoBox[i].InitEvents(dialogueData.speakerDatas[i]);
            this.arrSpeakerInfoBox[i].LoadValueIntoField(dialogueData.speakerDatas[i]);
        }
    }
}

[Serializable]
public class DialogueNode : BaseNode
{
    // --- 資料參數
    public List<DialogueNodePort> listDialogueNodePort = new List<DialogueNodePort>(); // 對話節點輸出端口
    public List<DialogueData> dialogueDatas = new List<DialogueData>();                // 單一對話資料List
                                                                                       
    // --- 顯示組件                                                                    
    private List<DialogueBox> listDialogueBox = new List<DialogueBox>();               // 單一講者顯示容器List

    public DialogueNode() { }

    public override void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        this.editorWindow = editorWindow;
        this.graphView = graphView;

        // 節點標題
        this.title = "Dialogue";

        // 設定節點位置
        this.SetPosition(new Rect(position, this.defaultNodeSize));

        // 取得節點Guid
        this.nodeGuid = Guid.NewGuid().ToString();

        // 新增StyleSheet屬性
        this.AddToClassList("DialogueNode");

        // 新增輸入端口
        this.AddInputPort("Input", Port.Capacity.Multi);

        // 新增輸出端口
        this.AddOutputPort("Output", Port.Capacity.Single);

        // 增加選擇輸出端口按鈕
        Button addChoiceBtn = new Button()
        {
            text = "Add Choice"
        };
        addChoiceBtn.clicked += () =>
        {
            this.AddChoicePort(this);
        };

        addChoiceBtn.AddToClassList("Label");
        this.titleButtonContainer.Add(addChoiceBtn);

        // 添加單一講者顯示容器按鈕
        Button addDialogueBtn = new Button()
        {
            text = "Add Dialogue"
        };

        addDialogueBtn.clicked += () =>
        {
            this._AddDialogue();
        };

        addDialogueBtn.AddToClassList("Label");
        this.titleButtonContainer.Add(addDialogueBtn);

        this.titleContainer.Q<Label>().AddToClassList("Label");
    }

    public override void LoadValueIntoField()
    {
        for (int i = 0; i < this.dialogueDatas.Count; i++)
        {
            this._AddDialogue(this.dialogueDatas[i]);
        }
    }

    /// <summary>
    /// 新增單一對話
    /// </summary>
    /// <param name="dialogueData">對話資料</param>
    private void _AddDialogue(DialogueData dialogueData = null)
    {
        // 如果沒有對話資料，代表是新建而不是複製的對話
        if (dialogueData == null)
        {
            // 初始單一對話資料
            dialogueData = new DialogueData();

            // 新增到對話資料列表中
            this.dialogueDatas.Add(dialogueData);
        }

        // 創立一個新的單一對話顯示容器
        this._CreateDialogueBox(dialogueData);
    }

    /// <summary>
    /// 複製單一對話
    /// </summary>
    /// <param name="dialogueData">對話資料</param>
    private void _DuplicateDialogue(DialogueData dialogueData)
    {
        // 初始化一個新的對話資料
        DialogueData newDialogue = new DialogueData();

        // 深層複製對話資料
        newDialogue.content = dialogueData.content;
        newDialogue.audioId = dialogueData.audioId;
        newDialogue.speakerName = dialogueData.speakerName;

        // 初始化三位講者資訊
        for (int j = 0; j < dialogueData.speakerDatas.Length; j++)
        {
            // 初始化一個新的講者資訊
            SpeakerData speakerData = new SpeakerData();

            // 深層複製講者顯示資訊
            speakerData.speakerImageTurnBlack = dialogueData.speakerDatas[j].speakerImageTurnBlack;
            speakerData.speakerImageReverse = dialogueData.speakerDatas[j].speakerImageReverse;
            speakerData.gid = dialogueData.speakerDatas[j].gid;
            newDialogue.speakerDatas[j] = speakerData;
        }

        // 將新建的對話資料新增到對話列表內
        this.dialogueDatas.Add(newDialogue);

        // 代入對話資料，新增單一對話
        this._AddDialogue(newDialogue);
    }

    /// <summary>
    /// 新增單一對話顯示容器
    /// </summary>
    /// <param name="dialogueData">對話資料</param>
    private void _CreateDialogueBox(DialogueData dialogueData)
    {
        // 初始單一對話顯示容器
        DialogueBox dialogueBox = new DialogueBox();

        // 初始化相關UI顯示
        dialogueBox.InitEvents(dialogueData);
        dialogueBox.InitSpeakerInfoBoxes(dialogueData);
        dialogueBox.LoadValueIntoField(dialogueData);

        // 新增到單一對話顯示容器列表內
        this.listDialogueBox.Add(dialogueBox);

        // 將單一對話顯示容器VisualElement新增到對話節點主要顯示容器內
        this.mainContainer.Add(dialogueBox.container);

        // 以下註冊相關功能按鈕事件
        // 上移單一對話顯示容器
        dialogueBox.upBtn.clickable.clicked += (() =>
        {
            int index = this.dialogueDatas.IndexOf(dialogueData);
            this._MoveDialogueBox(index, index - 1);
        });

        // 下移單一對話顯示容器
        dialogueBox.downBtn.clickable.clicked += (() =>
        {
            int index = this.dialogueDatas.IndexOf(dialogueData);
            this._MoveDialogueBox(index, index + 1);
        });

        // 複製單一對話顯示容器
        dialogueBox.duplicateBtn.clickable.clicked += (() =>
        {
            this._DuplicateDialogue(dialogueData);
        });

        // 刪除單一對話顯示容器
        dialogueBox.deleteBtn.clickable.clicked += (() =>
        {
            this.listDialogueBox.Remove(dialogueBox);
            this.dialogueDatas.Remove(dialogueData);
            this.mainContainer.Remove(dialogueBox.container);
            dialogueBox = null;
        });
    }

    /// <summary>
    /// 移動單一對話顯示容器位置
    /// </summary>
    /// <param name="oldIndex">單一對話顯示容器列表內的舊Index</param>
    /// <param name="newIndex">單一對話顯示容器列表內的新Index</param>
    private void _MoveDialogueBox(int oldIndex, int newIndex)
    {
        // 如果容器是在第一個而要往上移，或是在最後一個而要往下移就Return
        if ((oldIndex == 0 && oldIndex > newIndex) || (oldIndex == this.dialogueDatas.Count - 1 && oldIndex < newIndex)) return;

        // 透過oldIndex取出對話
        DialogueData newDialogue = this.dialogueDatas[oldIndex];
        // 刪除該Index的對話
        this.dialogueDatas.RemoveAt(oldIndex);
        // 插入新建的對話到指定Index
        this.dialogueDatas.Insert(newIndex, newDialogue);

        // 清除主顯示容器內的所有單一對話顯示容器
        foreach (var box in this.listDialogueBox)
        {
            this.mainContainer.Remove(box.container);
        }

        // 清除單一對話顯示容器列表
        this.listDialogueBox.Clear();

        // 重新透過排序好的對話資料列表來產生對話容器
        for (int i = 0; i < this.dialogueDatas.Count; i++)
        {
            this._AddDialogue(this.dialogueDatas[i]);
        }
    }

    /// <summary>
    /// 新增選擇端口
    /// </summary>
    /// <param name="baseNode">要新增端口的目標節點</param>
    /// <param name="dialogueNodePort">對話選擇端口</param>
    /// <returns></returns>
    public Port AddChoicePort(BaseNode baseNode, DialogueNodePort dialogueNodePort = null)
    {
        Port port = this.GetPortInstance(Direction.Output);

        //int outputPortCount = baseNode.outputContainer.Query("connector").ToList().Count();
        //string outputPortName = string.Format("Choice {0}", outputPortCount + 1);

        DialogueNodePort tempDialogueNodePort = new DialogueNodePort();

        // 以載入對話容器資料方式來新增對話選擇端口
        if (dialogueNodePort != null)
        {
            // 設置輸入/出端口Guid和相關資訊
            tempDialogueNodePort.inputGuid = dialogueNodePort.inputGuid;
            tempDialogueNodePort.outputGuid = dialogueNodePort.outputGuid;
            tempDialogueNodePort.portText = dialogueNodePort.portText;
        }

        // 接口的文字
        TextField textField = new TextField();
        textField.RegisterValueChangedCallback(value =>
        {
            tempDialogueNodePort.portText = value.newValue;
        });
        textField.SetValueWithoutNotify(tempDialogueNodePort.portText.Length > 0 ? tempDialogueNodePort.portText : "Text Content");
        port.contentContainer.Add(textField);

        // 刪除按鈕
        Button deleteButton = new Button(() => this._DeletePort(baseNode, port))
        {
            text = "X",
        };
        port.contentContainer.Add(deleteButton);

        tempDialogueNodePort.port = port;
        port.portName = "";

        this.listDialogueNodePort.Add(tempDialogueNodePort);

        baseNode.outputContainer.Add(port);

        // 刷新端口
        baseNode.RefreshPorts();
        baseNode.RefreshExpandedState();

        return port;
    }

    /// <summary>
    /// 刪除對話選擇端
    /// </summary>
    /// <param name="node"></param>
    /// <param name="port"></param>
    private void _DeletePort(BaseNode node, Port port)
    {
        // 從端口列表中查找到符合的端口
        DialogueNodePort tmp = this.listDialogueNodePort.Find(tempPort => tempPort.port == port);

        // 刪除端口
        this.listDialogueNodePort.Remove(tmp);

        // 查找到跟此輸出端口有連結的線段
        Edge portEdge = this.graphView.edges.ToList().Where(edge => edge.output == port).FirstOrDefault();

        // 如果有線段存在
        if (portEdge != null)
        {
            // 以下斷開線段連結
            Edge edge = portEdge;
            // 斷開Input端
            edge.input.Disconnect(edge);
            // 斷開Output端
            edge.output.Disconnect(edge);
            // 最後，刪除線段
            this.graphView.RemoveElement(edge);
        }

        // 從節點的輸出端口容器中刪除該端口
        node.outputContainer.Remove(port);

        // 刷新節點端口
        node.RefreshPorts();
        node.RefreshExpandedState();
    }
}

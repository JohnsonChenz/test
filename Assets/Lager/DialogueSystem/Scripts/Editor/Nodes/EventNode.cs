using DialogueSys;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class EventNode : BaseNode
{
    public EventNode() { }

    public override void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        this.editorWindow = editorWindow;
        this.graphView = graphView;

        // 節點標題
        this.title = "Event";

        // 設定節點位置
        this.SetPosition(new Rect(position, this.defaultNodeSize));

        // 取得節點Guid
        this.nodeGuid = Guid.NewGuid().ToString();

        // 新增StyleSheet屬性
        this.AddToClassList("EventNode");

        // 新增輸入端口
        this.AddInputPort("Input", Port.Capacity.Multi);

        // 新增輸出端口
        this.AddOutputPort("Output", Port.Capacity.Single);
    }
}

/// <summary>
/// ScriptableObject版本事件節點
/// </summary>
public class EventNodeSO : EventNode
{
    // --- 資料參數
    public DialogueEventSO dialogueEvent;

    // --- 顯示組件
    private ObjectField objectField;

    public EventNodeSO() { }

    public override void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        // 調用父類創建基本Event節點
        base.CreateNode(position, editorWindow, graphView);

        // 初始物件拖放區域
        this.objectField = new ObjectField()
        {
            // 設定可拖放入種類為DialogueEventSO
            objectType = typeof(DialogueEventSO),
            // 是否可放場景物件
            allowSceneObjects = false,
            // 設定數值
            value = this.dialogueEvent
        };

        // 設定參數變化Callback
        this.objectField.RegisterValueChangedCallback(value =>
        {
                // 將UI參數寫入到資料參數中
                this.dialogueEvent = objectField.value as DialogueEventSO;
        });

        // 讀取資料參數更新到UI顯示
        this.objectField.SetValueWithoutNotify(this.dialogueEvent);

        // 加入到節點主VisualElement容器中
        this.mainContainer.Add(this.objectField);
    }


    public override void LoadValueIntoField()
    {
        // 讀取資料參數更新到UI顯示
        this.objectField.SetValueWithoutNotify(this.dialogueEvent);
    }
}

/// <summary>
/// 字串版本事件節點
/// </summary>
public class EventNodeStr : EventNode
{
    // --- 資料參數
    public string eventId;

    // --- 顯示組件
    private TextField textField;

    public EventNodeStr() { }

    public override void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        base.CreateNode(position, editorWindow, graphView);

        // 初始文字輸入區域
        this.textField = new TextField("");

        // 設定參數變化Callback
        this.textField.RegisterValueChangedCallback(value =>
        {
                // 將UI參數寫入到資料參數中
                this.eventId = textField.value;
        });

        // 讀取資料參數更新到UI顯示
        this.textField.SetValueWithoutNotify(this.eventId);

        // 加入到節點主VisualElement容器中
        this.mainContainer.Add(this.textField);
    }

    public override void LoadValueIntoField()
    {
        // 讀取資料參數更新到UI顯示
        this.textField.SetValueWithoutNotify(this.eventId);
    }

}



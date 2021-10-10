using DialogueSys;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseNode : Node
{
    public string nodeGuid;                                    // 節點Guid
    public MainNodePort mainNodePort;                          // 主輸出端口
    protected DialogueGraphView graphView;                     // 視覺化圖像容器
    protected DialogueEditorWindow editorWindow;               // 視覺化編輯器視窗
    protected Vector2 defaultNodeSize = new Vector2(200, 250); // 初始節點大小

    public BaseNode()
    {
        StyleSheet styleSheet = Resources.Load<StyleSheet>("NodeStyleSheet");
        this.styleSheets.Add(styleSheet);
        this.mainNodePort = new MainNodePort();
    }

    /// <summary>
    /// 新增輸出端口
    /// </summary>
    /// <param name="name">端口名稱</param>
    /// <param name="capacity">端口同時可連結之數量</param>
    protected void AddOutputPort(string name, Port.Capacity capacity = Port.Capacity.Single)
    {
        Port outputPort = GetPortInstance(Direction.Output, capacity);
        outputPort.portName = name;
        this.outputContainer.Add(outputPort);

        // 存下輸出端端口資料
        this.mainNodePort.port = outputPort;
    }

    /// <summary>
    /// 新增輸入端口
    /// </summary>
    /// <param name="name">端口名稱</param>
    /// <param name="capacity">端口同時可被連結之數量</param>
    protected void AddInputPort(string name, Port.Capacity capacity = Port.Capacity.Multi)
    {
        Port inputPort = GetPortInstance(Direction.Input, capacity);
        inputPort.portName = name;
        this.inputContainer.Add(inputPort);
    }

    /// <summary>
    /// 實例化端口
    /// </summary>
    /// <param name="nodeDirection">端口種類 (In Or Out)</param>
    /// <param name="capacity">端口同時可被連結之數量</param>
    /// <returns></returns>
    protected Port GetPortInstance(Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
    }

    /// <summary>
    /// 讀取更新數值到UI顯示，由子類實作
    /// </summary>
    public virtual void LoadValueIntoField() { }

    /// <summary>
    /// 創建節點，由子類實作
    /// </summary>
    /// <param name="position">節點位置</param>
    /// <param name="editorWindow">編輯器視窗</param>
    /// <param name="graphView">視覺化節點容器</param>
    public abstract void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView);
}

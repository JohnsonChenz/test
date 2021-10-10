using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EndNode : BaseNode
{
    public EndNode() { }

    public override void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        this.editorWindow = editorWindow;
        this.graphView = graphView;

        // 節點標題
        this.title = "End";

        // 設定節點位置
        this.SetPosition(new Rect(position, this.defaultNodeSize));

        // 取得節點Guid
        this.nodeGuid = Guid.NewGuid().ToString();

        // 新增StyleSheet屬性
        this.AddToClassList("EndNode");

        // 新增輸入端口
        this.AddInputPort("Input", Port.Capacity.Multi);
    }
}

using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StartNode : BaseNode
{
    public StartNode() { }

    public override void CreateNode(Vector2 position, DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        this.editorWindow = editorWindow;
        this.graphView = graphView;

        this.title = "Start";
        this.SetPosition(new Rect(position, this.defaultNodeSize));
        this.nodeGuid = Guid.NewGuid().ToString();
        this.AddToClassList("StartNode");

        this.AddOutputPort("OutPut", Port.Capacity.Single);

        this.RefreshExpandedState();
        this.RefreshPorts();
    }
}

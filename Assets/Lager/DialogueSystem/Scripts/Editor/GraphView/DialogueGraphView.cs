using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    private string styleSheetsName = "GraphViewStyleSheet";
    private DialogueEditorWindow editorWindow;
    private NodeSearchWindow searchWindow;

    public DialogueGraphView(DialogueEditorWindow editorWindow)
    {
        this.editorWindow = editorWindow;

        StyleSheet tmpStyleSheet = Resources.Load<StyleSheet>(this.styleSheetsName);
        this.styleSheets.Add(tmpStyleSheet);

        this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        GridBackground grid = new GridBackground();
        this.Insert(0, grid);
        grid.StretchToParentSize();

        this._AddSearchWindow();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();
        Port startPortView = startPort;

        this.ports.ForEach((port) =>
        {
            Port portView = port;

            if (startPortView != portView && startPortView.node != portView.node && startPortView.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    private void _AddSearchWindow()
    {
        this.searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        this.searchWindow.Configure(this.editorWindow, this);
        this.nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this.searchWindow);
    }

    public T CreateNode<T>(Vector2 pos) where T : BaseNode, new()
    {
        var node = new T();
        node.CreateNode(pos, this.editorWindow, this);

        return node;
    }
}
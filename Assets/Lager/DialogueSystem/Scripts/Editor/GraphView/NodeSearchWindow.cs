using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueEditorWindow editorWindow;
    private DialogueGraphView graphView;

    private Texture2D pic;

    public void Configure(DialogueEditorWindow editorWindow, DialogueGraphView graphView)
    {
        this.editorWindow = editorWindow;
        this.graphView = graphView;

        this.pic = new Texture2D(1, 1);
        this.pic.SetPixel(0, 0, new Color(0, 0, 0, 0));
        this.pic.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>
                {
                    new SearchTreeGroupEntry(new GUIContent("Dialogue Node"),0),

                    this._AddNodeSearch("Start Node", new StartNode()),
                    this._AddNodeSearch("Dialogue Node", new DialogueNode()),
                    this._AddNodeSearch("Event Node - SO Version", new EventNodeSO()),
                    this._AddNodeSearch("Event Node - Str Version", new EventNodeStr()),
                    this._AddNodeSearch("End Node", new EndNode()),
                };

        return tree;
    }

    private SearchTreeEntry _AddNodeSearch(string name, BaseNode baseNode)
    {
        SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(name, this.pic))
        {
            level = 1,
            userData = baseNode
        };

        return tmp;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        Vector2 mousePosition = this.editorWindow.rootVisualElement.ChangeCoordinatesTo
            (
            editorWindow.rootVisualElement.parent, context.screenMousePosition - this.editorWindow.position.position
            );

        Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);

        return this._CheckForNodeType(searchTreeEntry, graphMousePosition);
    }

    private bool _CheckForNodeType(SearchTreeEntry searchTreeEntry, Vector2 pos)
    {
        // 製造節點
        switch (searchTreeEntry.userData)
        {
            case StartNode node:
                this.graphView.AddElement(this.graphView.CreateNode<StartNode>(pos));
                return true;
            case DialogueNode node:
                this.graphView.AddElement(this.graphView.CreateNode<DialogueNode>(pos));
                return true;
            case EventNodeSO node:
                this.graphView.AddElement(this.graphView.CreateNode<EventNodeSO>(pos));
                return true;
            case EventNodeStr node:
                this.graphView.AddElement(this.graphView.CreateNode<EventNodeStr>(pos));
                return true;
            case EndNode node:
                this.graphView.AddElement(this.graphView.CreateNode<EndNode>(pos));
                return true;
            default:
                break;
        }

        return false;
    }
}


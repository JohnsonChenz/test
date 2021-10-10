using DialogueSys;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;


public class DialogueNodeSaveAndLoad
{
    private List<Edge> edges => this.graphView.edges.ToList();
    public List<BaseNode> nodes => this.graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();

    private DialogueGraphView graphView;

    public DialogueNodeSaveAndLoad(DialogueGraphView graphView)
    {
        this.graphView = graphView;
    }

    public void Save(DialogueDataContainerForNode dialogueContainerSO)
    {
        this._SaveEdges(dialogueContainerSO);
        this._SaveNodes(dialogueContainerSO);
    }

    public void Load(DialogueDataContainerForNode dialogueContainerSO)
    {
        this._ClearGraph();
        this._GenerateNodes(dialogueContainerSO);
        this._ConnectNodes();
    }

    #region 儲存Node和連線資訊
    private void _SaveEdges(DialogueDataContainerForNode dialogueContainerSO)
    {
        // 找到所有有連線的Port (也就是input node不為空)
        Edge[] connectedEdges = edges.Where(edge => edge.input.node != null).ToArray();

        for (int i = 0; i < connectedEdges.Count(); i++)
        {
            BaseNode outputNode = (BaseNode)connectedEdges[i].output.node;
            BaseNode inputNode = (BaseNode)connectedEdges[i].input.node;
        }
    }

    private void _SaveNodes(DialogueDataContainerForNode dialogueContainerSO)
    {
        // 清空原有的SO資料
        dialogueContainerSO.listDialogueNodeData.Clear();
        dialogueContainerSO.listStartNodeData.Clear();
        dialogueContainerSO.listEndNodeData.Clear();
        dialogueContainerSO.listEventNodeSOData.Clear();
        dialogueContainerSO.listEventNodeStrData.Clear();

        // 查找所有Node，並依照Node型別進行儲存
        this.nodes.ForEach(node =>
        {
            switch (node)
            {
                case DialogueNode dialogueNode:
                    dialogueContainerSO.listDialogueNodeData.Add(this._SaveNodeData(dialogueNode));
                    break;
                case StartNode startNode:
                    dialogueContainerSO.listStartNodeData.Add(this._SaveNodeData(startNode));
                    break;
                case EndNode endNode:
                    dialogueContainerSO.listEndNodeData.Add(this._SaveNodeData(endNode));
                    break;
                case EventNodeSO eventNodeSO:
                    dialogueContainerSO.listEventNodeSOData.Add(this._SaveNodeData(eventNodeSO));
                    break;
                case EventNodeStr eventNodeStr:
                    dialogueContainerSO.listEventNodeStrData.Add(this._SaveNodeData(eventNodeStr));
                    break;
                default:
                    break;
            }
        });
    }

    private DialogueNodeData _SaveNodeData(DialogueNode node)
    {
        // 初始化，並從節點中複製必要資料並儲存
        DialogueNodeData nodeData = new DialogueNodeData
        {
            nodeGuid = node.nodeGuid,
            position = node.GetPosition().position,
        };

        // 深層複製講者資料
        for (int i = 0; i < node.dialogueDatas.Count; i++)
        {
            DialogueData dialogueData = new DialogueData();
            dialogueData.content = node.dialogueDatas[i].content;
            dialogueData.audioId = node.dialogueDatas[i].audioId;
            dialogueData.speakerName = node.dialogueDatas[i].speakerName;

            for (int j = 0; j < node.dialogueDatas[i].speakerDatas.Length; j++)
            {
                SpeakerData speakerData = new SpeakerData();
                speakerData.speakerImageTurnBlack = node.dialogueDatas[i].speakerDatas[j].speakerImageTurnBlack;
                speakerData.speakerImageReverse = node.dialogueDatas[i].speakerDatas[j].speakerImageReverse;
                speakerData.gid = node.dialogueDatas[i].speakerDatas[j].gid;
                dialogueData.speakerDatas[j] = speakerData;
            }

            nodeData.dialogueDatas.Add(dialogueData);
        }

        // 深層複製事件子節點資料
        for (int i = 0; i < node.listDialogueNodePort.Count; i++)
        {
            DialogueNodePort dielogueNodePort = new DialogueNodePort();
            dielogueNodePort.portText = node.listDialogueNodePort[i].portText;
            dielogueNodePort.port = node.listDialogueNodePort[i].port;
            nodeData.listDialogueNodePort.Add(dielogueNodePort);
        }

        // 主連線Guid儲存
        this._SavePortGuidIntoData(nodeData, node);

        // Dialogue節點 子連線Guid儲存
        // 遍歷整個Dialogue節點底下的選擇端口
        foreach (DialogueNodePort nodePort in nodeData.listDialogueNodePort.ToList())
        {
            // 重置該選擇端口的Guid
            nodePort.outputGuid = string.Empty;
            nodePort.inputGuid = string.Empty;

            // 遍歷整個GraphView所有的連線
            foreach (Edge edge in this.edges.ToList())
            {
                // 如果線的輸出(Output)端口等於選擇端口
                if (edge.output == nodePort.port)
                {
                    // 將線的Output端口連著的Node節點Guid存下來，以便讀取資料時能動態產生此線
                    nodePort.outputGuid = (edge.output.node as BaseNode).nodeGuid;
                    // 將線的Iutput端口連著的Node節點Guid存下來，以便讀取資料時能動態產生此線
                    nodePort.inputGuid = (edge.input.node as BaseNode).nodeGuid;
                }
            }
        }

        return nodeData;
    }

    private StartNodeData _SaveNodeData(StartNode node)
    {
        // 初始化，並從節點中複製必要資料並儲存
        StartNodeData nodeData = new StartNodeData()
        {
            nodeGuid = node.nodeGuid,
            position = node.GetPosition().position,
        };

        // 主連線Guid儲存
        this._SavePortGuidIntoData(nodeData, node);

        return nodeData;
    }

    private EndNodeData _SaveNodeData(EndNode node)
    {
        // 初始化，並從節點中複製必要資料並儲存
        EndNodeData nodeData = new EndNodeData()
        {
            nodeGuid = node.nodeGuid,
            position = node.GetPosition().position,
        };

        return nodeData;
    }

    private EventNodeSOData _SaveNodeData(EventNodeSO node)
    {
        // 初始化，並從節點中複製必要資料並儲存
        EventNodeSOData nodeData = new EventNodeSOData()
        {
            nodeGuid = node.nodeGuid,
            position = node.GetPosition().position,
            dialogueEventSO = node.dialogueEvent
        };

        // 主連線Guid儲存
        this._SavePortGuidIntoData(nodeData, node);
        return nodeData;
    }

    private EventNodeStrData _SaveNodeData(EventNodeStr node)
    {
        // 初始化，並從節點中複製必要資料並儲存
        EventNodeStrData nodeData = new EventNodeStrData()
        {
            nodeGuid = node.nodeGuid,
            position = node.GetPosition().position,
            funcId = node.eventId
        };

        // 主連線Guid儲存
        this._SavePortGuidIntoData(nodeData, node);
        return nodeData;
    }

    private void _SavePortGuidIntoData(BaseNodeData nodeData, BaseNode node)
    {
        // 主連線Guid儲存
        // 遍歷整個GraphView所有的連線
        foreach (Edge edge in this.edges.ToList())
        {
            // 如果線的輸出(Output)端口等於節點的主輸出端口
            if (edge.output == node.mainNodePort.port)
            {
                // 將線的Output端口連著的Node節點Guid存下來，以便讀取資料時能動態產生此線
                nodeData.mainNodePort.outputGuid = (edge.output.node as BaseNode).nodeGuid;
                // 將線的Input端口連著的Node節點Guid存下來，以便讀取資料時能動態產生此線
                nodeData.mainNodePort.inputGuid = (edge.input.node as BaseNode).nodeGuid;
            }
        }
    }
    #endregion

    #region 讀取Node和連線資訊

    private void _ClearGraph()
    {
        // 清空所有的連線
        this.edges.ForEach(edge => this.graphView.RemoveElement(edge));

        // 清空所有的節點
        foreach (BaseNode node in this.nodes)
        {
            this.graphView.RemoveElement(node);
        }
    }

    private void _GenerateNodes(DialogueDataContainerForNode dialogueContainerSO)
    {
        // Start Node
        foreach (StartNodeData node in dialogueContainerSO.listStartNodeData.ToList())
        {
            // 初始化，並從SO中複製所有必要資料
            StartNode tempNode = this.graphView.CreateNode<StartNode>(node.position);
            tempNode.nodeGuid = node.nodeGuid;
            tempNode.mainNodePort.inputGuid = node.mainNodePort.inputGuid;
            tempNode.mainNodePort.outputGuid = node.mainNodePort.outputGuid;

            // 將讀取的資料更新到Graph顯示
            tempNode.LoadValueIntoField();

            this.graphView.AddElement(tempNode);
        }

        // End Node
        foreach (EndNodeData node in dialogueContainerSO.listEndNodeData.ToList())
        {
            // 初始化，並從SO中複製所有必要資料
            EndNode tempNode = this.graphView.CreateNode<EndNode>(node.position);
            tempNode.nodeGuid = node.nodeGuid;

            // 將讀取的資料更新到Graph顯示
            tempNode.LoadValueIntoField();

            this.graphView.AddElement(tempNode);
        }

        // Event Node SO Ver
        foreach (EventNodeSOData node in dialogueContainerSO.listEventNodeSOData.ToList())
        {
            // 初始化，並從SO中複製所有必要資料
            EventNodeSO tempNode = this.graphView.CreateNode<EventNodeSO>(node.position);
            tempNode.nodeGuid = node.nodeGuid;
            tempNode.dialogueEvent = node.dialogueEventSO;
            tempNode.mainNodePort.inputGuid = node.mainNodePort.inputGuid;
            tempNode.mainNodePort.outputGuid = node.mainNodePort.outputGuid;

            // 將讀取的資料更新到Graph顯示
            tempNode.LoadValueIntoField();

            this.graphView.AddElement(tempNode);
        }

        // Event Node Str Ver
        foreach (EventNodeStrData node in dialogueContainerSO.listEventNodeStrData.ToList())
        {
            // 初始化，並從SO中複製所有必要資料
            EventNodeStr tempNode = this.graphView.CreateNode<EventNodeStr>(node.position);
            tempNode.nodeGuid = node.nodeGuid;
            tempNode.eventId = node.funcId;
            tempNode.mainNodePort.inputGuid = node.mainNodePort.inputGuid;
            tempNode.mainNodePort.outputGuid = node.mainNodePort.outputGuid;

            // 將讀取的資料更新到Graph顯示
            tempNode.LoadValueIntoField();

            this.graphView.AddElement(tempNode);
        }

        // Dialogue Node
        foreach (DialogueNodeData node in dialogueContainerSO.listDialogueNodeData.ToList())
        {
            // 初始化，並從SO中複製所有必要資料
            DialogueNode tempNode = this.graphView.CreateNode<DialogueNode>(node.position);
            tempNode.nodeGuid = node.nodeGuid;
            tempNode.mainNodePort.inputGuid = node.mainNodePort.inputGuid;
            tempNode.mainNodePort.outputGuid = node.mainNodePort.outputGuid;

            // 深層複製講者資訊
            for (int i = 0; i < node.dialogueDatas.Count; i++)
            {
                DialogueData dialogueData = new DialogueData();
                dialogueData.content = node.dialogueDatas[i].content;
                dialogueData.audioId = node.dialogueDatas[i].audioId;
                dialogueData.speakerName = node.dialogueDatas[i].speakerName;

                for (int j = 0; j < node.dialogueDatas[i].speakerDatas.Length; j++)
                {
                    SpeakerData speakerData = new SpeakerData();
                    speakerData.speakerImageTurnBlack = node.dialogueDatas[i].speakerDatas[j].speakerImageTurnBlack;
                    speakerData.speakerImageReverse = node.dialogueDatas[i].speakerDatas[j].speakerImageReverse;
                    speakerData.gid = node.dialogueDatas[i].speakerDatas[j].gid;
                    dialogueData.speakerDatas[j] = speakerData;
                }

                tempNode.dialogueDatas.Add(dialogueData);
            }

            // 新增子節點
            foreach (DialogueNodePort nodePort in node.listDialogueNodePort)
            {
                tempNode.AddChoicePort(tempNode, nodePort);
            }

            // 將讀取的資料更新到Graph顯示
            tempNode.LoadValueIntoField();
            this.graphView.AddElement(tempNode);
        }
    }

    private void _ConnectNodes()
    {
        // 處理主連線
        for (int i = 0; i < this.nodes.Count; i++)
        {
            // 如果含有目標節點Guid
            if (this.nodes[i].mainNodePort.inputGuid != string.Empty)
            {
                // 從全部節點資料中透過Guid找出目標節點
                BaseNode targetNode = this.nodes.First(node => node.nodeGuid == this.nodes[i].mainNodePort.inputGuid);

                // 連線
                this._LinkNodesTogether(this.nodes[i].mainNodePort.port, (Port)targetNode.inputContainer[0]);
            }
        }

        // 以下開始處理DialogueNode節點的子Port

        // 尋找到所有類型為DialogueNode的節點，存成List
        List<DialogueNode> dialogueNodes = this.nodes.FindAll(node => node is DialogueNode).Cast<DialogueNode>().ToList();

        // 遍歷之中的DialogueNode
        foreach (DialogueNode dialogueNode in dialogueNodes)
        {
            // 遍歷DialogueNode中的選擇節點
            foreach (DialogueNodePort nodePort in dialogueNode.listDialogueNodePort)
            {
                // 如果含有目標節點Guid
                if (nodePort.inputGuid != string.Empty)
                {
                    // 從全部節點資料中透過Guid找出目標節點
                    BaseNode targetNode = this.nodes.First(node => node.nodeGuid == nodePort.inputGuid);

                    // 連線
                    this._LinkNodesTogether(nodePort.port, (Port)targetNode.inputContainer[0]);
                }
            }
        }
    }

    private void _LinkNodesTogether(Port outputPort, Port inputPort)
    {
        // 新增連線線段
        Edge tempEdge = new Edge()
        {
            output = outputPort,
            input = inputPort
        };

        // 進行連線
        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        // 新增到視圖中
        this.graphView.Add(tempEdge);
    }

    #endregion
}

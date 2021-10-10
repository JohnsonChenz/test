using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSys
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue")]
    [System.Serializable]
    public class DialogueDataContainerForNode : ScriptableObject
    {
        public List<DialogueNodeData> listDialogueNodeData = new List<DialogueNodeData>();

        public List<StartNodeData> listStartNodeData = new List<StartNodeData>();

        public List<EndNodeData> listEndNodeData = new List<EndNodeData>();

        public List<EventNodeSOData> listEventNodeSOData = new List<EventNodeSOData>();

        public List<EventNodeStrData> listEventNodeStrData = new List<EventNodeStrData>();
    }

    [System.Serializable]
    public class BaseNodeData
    {
        public string nodeGuid;
        public Vector2 position;
        public MainNodePort mainNodePort = new MainNodePort();
    }

    [System.Serializable]
    public class DialogueNodeData : BaseNodeData
    {
        public List<DialogueData> dialogueDatas = new List<DialogueData>();
        public List<DialogueNodePort> listDialogueNodePort = new List<DialogueNodePort>();
    }

    [System.Serializable]
    public class EventNodeSOData : BaseNodeData
    {
        public DialogueEventSO dialogueEventSO;
    }

    [System.Serializable]
    public class EventNodeStrData : BaseNodeData
    {
        public string funcId;
    }


    [System.Serializable]
    public class StartNodeData : BaseNodeData
    {

    }

    [System.Serializable]
    public class EndNodeData : BaseNodeData
    {

    }

    [System.Serializable]
    public class NodePort
    {
#if UNITY_EDITOR
        public Port port;
#endif
        public string inputGuid;
        public string outputGuid;

        public NodePort()
        {
            this.inputGuid = string.Empty;
            this.outputGuid = string.Empty;
        }
    }

    [System.Serializable]
    public class MainNodePort : NodePort
    {

    }

    [System.Serializable]
    public class DialogueNodePort : NodePort
    {
        public string portText;

        public DialogueNodePort()
        {
            this.portText = string.Empty;
        }
    }
}

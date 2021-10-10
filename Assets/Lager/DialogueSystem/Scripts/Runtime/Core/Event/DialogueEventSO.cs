using UnityEngine;

namespace DialogueSys
{

    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Event")]
    [System.Serializable]
    public class DialogueEventSO : ScriptableObject
    {
        public virtual void RunEvent()
        {
            Debug.Log("Event was Called");
        }
    }

}

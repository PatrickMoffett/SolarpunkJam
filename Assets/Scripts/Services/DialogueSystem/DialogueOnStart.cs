using Services;
using UnityEngine;
using UnityEngine.Assertions;

public class DialogueOnStart : MonoBehaviour
{
    [SerializeField] private Dialogue dialogueToShow;
    private void Start()
    {
        Assert.IsNotNull(dialogueToShow, "Dialogue to show is not assigned in the inspector.");
        ServiceLocator.Instance.Get<DialogueSystem>().StartDialogue(dialogueToShow);
    }
}
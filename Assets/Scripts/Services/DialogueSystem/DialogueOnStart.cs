using NUnit.Framework;
using Services;
using UnityEngine;

public class DialogueOnStart : MonoBehaviour
{
    [SerializeField] private Dialogue dialogueToShow;
    private void Start()
    {
        Assert.IsNotNull(dialogueToShow, "Dialogue to show is not assigned in the inspector.");
        ServiceLocator.Instance.Get<DialogueSystem>().StartDialogue(dialogueToShow);
    }
}
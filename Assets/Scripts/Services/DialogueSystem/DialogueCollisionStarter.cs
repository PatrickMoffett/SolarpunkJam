using Services;
using UnityEngine;
using UnityEngine.Assertions;
public class DialogueCollisionStarter : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    bool DialogueWasStarted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartDialogue();
    }
    private void StartDialogue()
    {
        Assert.IsNotNull(dialogue, "Dialogue is not assigned in the inspector.");
        ServiceLocator.Instance.Get<DialogueSystem>().StartDialogue(dialogue);
    }
}
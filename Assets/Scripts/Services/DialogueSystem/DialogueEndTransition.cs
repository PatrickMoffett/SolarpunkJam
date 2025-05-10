using Services;
using System;
using UnityEngine;
using UnityEngine.Assertions;
public class DialogueEndTransition : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd += OnDialogueEnd;
    }
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnDialogueEnd;
    }
    private void OnDialogueEnd()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnDialogueEnd;
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameWonState>(true);
    }
}
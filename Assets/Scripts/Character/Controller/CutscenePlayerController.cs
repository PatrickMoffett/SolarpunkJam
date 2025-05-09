using System;
using System.Collections;
using System.Collections.Generic;
using Abilities;
using Services;
using UnityEngine;

public class CutscenePlayerController : MonoBehaviour
{
    private InputSystem_Actions _input;

    private void SpeedUpDialogue(bool shouldSpeedUp)
    {
        ServiceLocator.Instance.Get<DialogueSystem>().SetSpeedUpText(shouldSpeedUp);
    }

    private void SkipDialogue()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().ForceCompleteLine();
    }

    private void NextDialogue()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().RequestNextLine();
    }
    private void OnDialogueEnd()
    {
        _input.Player.Enable();
        _input.Dialogue.Disable();
    }

    private void OnDialogueStart()
    {
        _input.Dialogue.Enable();
        _input.Player.Disable();
    }

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }
    private void OnEnable()
    {

        _input.Dialogue.NextDialogue.performed += ctx => NextDialogue();
        _input.Dialogue.SkipDialogue.performed += ctx => SkipDialogue();
        _input.Dialogue.SpeedUpDialogue.started += ctx => SpeedUpDialogue(true);
        _input.Dialogue.SpeedUpDialogue.canceled += ctx => SpeedUpDialogue(false);

        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueStart += OnDialogueStart;
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd += OnDialogueEnd;

        _input.Enable();
        _input.Dialogue.Disable();
    }

    private void OnDisable()
    {
        _input.Disable();

        _input.Dialogue.NextDialogue.performed -= ctx => NextDialogue();
        _input.Dialogue.SkipDialogue.performed -= ctx => SkipDialogue();
        _input.Dialogue.SpeedUpDialogue.started -= ctx => SpeedUpDialogue(true);
        _input.Dialogue.SpeedUpDialogue.canceled -= ctx => SpeedUpDialogue(false);

        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueStart -= OnDialogueStart;
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnDialogueEnd;
    }
}
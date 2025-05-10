using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class DialogueSystem : IService
{
    private DialogueSystemSettings _settings = Resources.Load<DialogueSystemSettings>("DialogueSystemSettings");

    private UIWidget _dialogueUI;        // parent UIWidget to show/hide
    private Image _portraitImage;        // UI Image for speaker portrait
    private TMP_Text _dialogueText;          // UI Text (or TMP_Text)

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    private Coroutine typingCoroutine;
    private Coroutine audioBlipCoroutine;
    private string currentFullText;
    private bool isSpeedUpActive = false;
    private bool isCurrentLineFinished = false;
    private bool isWaitingForDelay = false;
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public DialogueSystem()
    {
        Assert.IsNotNull(_settings, "DialogueSystemSettings not found in Resources.");
        
        _dialogueUI = ServiceLocator.Instance.Get<UIManager>().LoadUI(_settings.dialoguePrefab);
        Assert.IsNotNull(_dialogueUI, "Dialogue UI prefab not found in Resources.");
        
        GameObject portraitImageObject =_dialogueUI.UIObject.GetComponentsInChildren<Transform>()
                            .FirstOrDefault(c => c.gameObject.name == "PortraitImage")?.gameObject;
        _portraitImage = portraitImageObject.GetComponent<Image>();
        Assert.IsNotNull(_portraitImage, "PortraitImage not found in Dialogue UI prefab.");

        GameObject dialogueTextObject = _dialogueUI.UIObject.GetComponentsInChildren<Transform>()
                            .FirstOrDefault(c => c.gameObject.name == "DialogueText")?.gameObject;
        _dialogueText = dialogueTextObject.GetComponent<TMP_Text>();
        Assert.IsNotNull(_dialogueText, "Dialogue Text not found in Dialogue UI prefab.");

        _dialogueUI.UIObject.SetActive(false);
    }

    public void SetSpeedUpText(bool shouldSpeedUp)
    {
        isSpeedUpActive = shouldSpeedUp;
    }

    public bool IsDialogueActive()
    {
        return _dialogueUI.UIObject.activeSelf;
    }
    /// <summary>
    /// Call this to start a conversation.
    /// </summary>
    public void StartDialogue(Dialogue dialogue)
    {
        if (_settings.allowPauseTimeDuringDialogue)
        {
            Time.timeScale = 0f;
        }
        lines.Clear();
        foreach (var line in dialogue.lines)
        {
            lines.Enqueue(line);
        }
        OnDialogueStart?.Invoke();
        ShowNextLine();
    }

    void ShowNextLine()
    {
        // Stop any in-progress typing
        if (typingCoroutine != null)
        {
            ServiceLocator.Instance.Get<MonoBehaviorService>().StopCoroutine(typingCoroutine);
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        var line = lines.Dequeue();
        _portraitImage.sprite = line.character.portrait;
        currentFullText = line.text;

        typingCoroutine = ServiceLocator.Instance.Get<MonoBehaviorService>().
            StartCoroutine(StartDialogueLine(line));
    }

    IEnumerator StartDialogueLine(DialogueLine dialogueLine)
    {
        if (dialogueLine.eventToBroadcast != null)
        {
            dialogueLine.eventToBroadcast.Raise();
        }
        isWaitingForDelay = true;
        yield return new WaitForSecondsRealtime(dialogueLine.startDelay);
        isWaitingForDelay = false;
        // Set UI to active if it wasn't already
        _dialogueUI.UIObject.SetActive(true);

        typingCoroutine = ServiceLocator.Instance.Get<MonoBehaviorService>().
            StartCoroutine(StartDialogueText(dialogueLine));
        audioBlipCoroutine = ServiceLocator.Instance.Get<MonoBehaviorService>().
            StartCoroutine(PlayDialogueBlips(dialogueLine));
    }
    IEnumerator StartDialogueText(DialogueLine dialogueLine)
    {
        isCurrentLineFinished = false;
        AudioManager audioManager = ServiceLocator.Instance.Get<AudioManager>();
        _dialogueText.text = "";
        for (int i = 0; i < dialogueLine.text.Length; i++)
        {
            _dialogueText.text += dialogueLine.text[i];

            float delay = isSpeedUpActive ? _settings.speedUpDelay : _settings.letterDelay;
            yield return new WaitForSecondsRealtime(delay);
        }
        // Stop blips
        ServiceLocator.Instance.Get<MonoBehaviorService>()
            .StopCoroutine(audioBlipCoroutine);
        isCurrentLineFinished = true;
    }
    IEnumerator PlayDialogueBlips(DialogueLine dialogueLine)
    {
        yield return new WaitForSecondsRealtime(dialogueLine.startDelay);
        GameObject go = ServiceLocator.Instance.Get<MonoBehaviorService>().gameObject;
        while (true)
        {
            float playTime = dialogueLine.character.voiceBlip.Play();
            yield return new WaitForSecondsRealtime(playTime);
        }
    }
    public void ForceCompleteLine()
    {
        if(isWaitingForDelay)
        {
            return;
        }
        if (typingCoroutine != null)
        {
            ServiceLocator.Instance.Get<MonoBehaviorService>()
                .StopCoroutine(typingCoroutine);
        }
        if(audioBlipCoroutine != null)
        {
            ServiceLocator.Instance.Get<MonoBehaviorService>()
                .StopCoroutine(audioBlipCoroutine);
        }
        _dialogueText.text = currentFullText;
        isCurrentLineFinished = true;
    }

    public void RequestNextLine()
    {
        if(isWaitingForDelay)
        {
            return;
        }
        if(isCurrentLineFinished)
        {
            ShowNextLine();
        }
    }

    void EndDialogue()
    {
        if(_settings.allowPauseTimeDuringDialogue)
        {
            Time.timeScale = 1f; // Ensure time is running at normal speed
        }
        _dialogueUI.UIObject.SetActive(false);
        OnDialogueEnd?.Invoke();
    }
}
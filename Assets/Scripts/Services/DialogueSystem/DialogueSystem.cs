using Services;
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
    private string currentFullText;
    private bool isSpeedUpActive = false;
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

    /// <summary>
    /// Call this to start a conversation.
    /// </summary>
    public void StartDialogue(Dialogue dialogue)
    {
        lines.Clear();
        foreach (var line in dialogue.lines)
        {
            lines.Enqueue(line);
        }

        _dialogueUI.UIObject.SetActive(true);
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
            StartCoroutine(RevealDialogueLine(line));
    }

    IEnumerator RevealDialogueLine(DialogueLine dialogueLine)
    {
        AudioManager audioManager = ServiceLocator.Instance.Get<AudioManager>();
        _dialogueText.text = "";
        for (int i = 0; i < dialogueLine.text.Length; i++)
        {
            _dialogueText.text += dialogueLine.text[i];
            //audioManager.PlaySfx(dialogueLine.character.voiceBlip);

            float delay = isSpeedUpActive ? _settings.speedUpDelay : _settings.letterDelay;
            yield return new WaitForSeconds(delay);
        }
    }

    void ForceCompleteLine()
    {
        if (typingCoroutine != null)
        {
            ServiceLocator.Instance.Get<MonoBehaviorService>()
                .StopCoroutine(typingCoroutine);
        }

        _dialogueText.text = currentFullText;
    }

    void EndDialogue()
    {
        _dialogueUI.UIObject.SetActive(false);
    }
}
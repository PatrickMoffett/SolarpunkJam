using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIPromptText : MonoBehaviour
{
    private TMP_Text _text;
    public static event Action<string> PromptTextChangeRequest;

    public static void RequestPromptText(string text)
    {
        PromptTextChangeRequest?.Invoke(text);
    }
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TMP_Text>();
        _text.text = "";
        PromptTextChangeRequest += SetPromptText;
    }

    private void SetPromptText(string newText)
    {
        if (newText != "")
        {
            newText = "[Interact] " + newText;
        }
        _text.text = newText;
    }
}

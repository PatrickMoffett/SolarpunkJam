using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public DialogueCharacter character;
    [TextArea(2, 5)]
    public string text;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    public List<DialogueLine> lines;
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public DialogueCharacter character;
    [TextArea(2, 5)]
    public string text;
    public float startDelay;
    public GameEvent eventToBroadcast;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    public List<DialogueLine> lines;
}

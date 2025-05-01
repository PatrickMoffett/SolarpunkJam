using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueCharacter")]
public class DialogueCharacter : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public AudioEvent voiceBlip;
}
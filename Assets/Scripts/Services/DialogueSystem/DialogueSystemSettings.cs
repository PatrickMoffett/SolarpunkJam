using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueSystemSettings")]
public class DialogueSystemSettings : ScriptableObject
{
    [Tooltip("Time Between Each Letter")]
    public float letterDelay = 0.05f;
    [Tooltip("Time Between Each Letter While Sped Up")]
    public float speedUpDelay = 0.01f;
    public bool allowPauseTimeDuringDialogue = true;
    // / The prefab to use for the dialogue UI
    // Must have the following:
    // - Portrait Image, named "PortraitImage"
    // - Dialogue Text, named "DialogueText"
    public GameObject dialoguePrefab;
}
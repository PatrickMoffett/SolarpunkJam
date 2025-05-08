using Services;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossEncounterTrigger : MonoBehaviour
{
    [SerializeField] GameObject _staticCameraTarget;
    [SerializeField] Dialogue _dialogue;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            // Trigger the boss encounter
            TriggerBossEncounter();
        }
    }

    private void TriggerBossEncounter()
    {
        // Set the static camera target
        CameraFollow camera =ServiceLocator.Instance.Get<PlayerManager>().GetPlayerFollowCamera();
        camera.SetStaticCameraMode(true);
        camera.SetFollowTarget(_staticCameraTarget.transform);

        // Start the boss intro dialogue
        DialogueSystem ds = ServiceLocator.Instance.Get<DialogueSystem>();
        ds.OnDialogueEnd += OnBossIntroDialogueEnd;
        ds.StartDialogue(_dialogue);

        // TODO: Close the doors
        
    }

    private void OnBossIntroDialogueEnd()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnBossIntroDialogueEnd;
        StartBossFight();
    }

    private void StartBossFight()
    {
        // Todo: Start the boss fight
        // Start the boss fight music
        // Start the boss fight logic
    }
}
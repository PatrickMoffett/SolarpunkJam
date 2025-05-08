using Services;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossEncounterManager : MonoBehaviour
{
    [SerializeField] GameObject _staticCameraTarget;
    [SerializeField] Dialogue _dialogue;

    enum BossEncounterState
    {
        Uninitiated,
        Intro,
        Fight,
        Completed
    }
    private BossEncounterState _state = BossEncounterState.Uninitiated;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_state == BossEncounterState.Uninitiated
            && col.gameObject.CompareTag("Player"))
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

        _state = BossEncounterState.Intro;
        // TODO: Close the doors

    }

    private void OnBossIntroDialogueEnd()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnBossIntroDialogueEnd;
        StartBossFight();
    }

    private void StartBossFight()
    {
        _state = BossEncounterState.Fight;
        // Todo: Start the boss fight
        // Start the boss fight music
        // Start the boss fight logic
    }
}
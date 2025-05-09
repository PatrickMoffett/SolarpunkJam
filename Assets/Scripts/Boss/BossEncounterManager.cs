using Services;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class BossEncounterManager : MonoBehaviour
{
    [SerializeField] GameObject _staticCameraTarget;
    [SerializeField] Dialogue _dialogue;
    [SerializeField] DoorHandler _entranceDoor;
    [SerializeField] DoorHandler _exitDoor;
    [SerializeField] AudioClip _bossFightMusic;
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
        _entranceDoor.CloseDoor();
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

        ServiceLocator.Instance.Get<MusicManager>().StartSong(_bossFightMusic, .2f, false);
        // Todo: Start the boss fight
        // Start the boss fight logic
    }


    /*TODO: end the boss fight
    _exitDoor.OpenDoor();
    */
}
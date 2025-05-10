using Services;
using UnityEngine;

public class BossEncounterManager : MonoBehaviour
{
    [SerializeField] GameObject _staticCameraTarget;
    [SerializeField] Dialogue _startdialogue;
    [SerializeField] Dialogue _endDialogue;
    [SerializeField] DoorHandler _entranceDoor;
    [SerializeField] DoorHandler _exitDoor;
    [SerializeField] AudioClip _bossFightMusic;
    [SerializeField] AudioClip _postBossMusic;

    [SerializeField] GameEvent BossFightStart;
    [SerializeField] GameEvent BossFightEnd;

    enum BossEncounterState
    {
        Uninitiated,
        Intro,
        Fight,
        Completed
    }
    private BossEncounterState _state = BossEncounterState.Uninitiated;

    private void OnEnable()
    {
        BossFightEnd.OnGameEvent += OnBossFightEnd;
    }
    private void OnDisable()
    {
        BossFightEnd.OnGameEvent -= OnBossFightEnd;
    }
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
        ds.StartDialogue(_startdialogue);

        _state = BossEncounterState.Intro;
        _entranceDoor.CloseDoor();

    }

    private void OnBossIntroDialogueEnd()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd -= OnBossIntroDialogueEnd;
        StartBossFight();
    }

    private void StartBossFight()
    {
        _state = BossEncounterState.Fight;


        ServiceLocator.Instance.Get<MusicManager>().StartSong(_bossFightMusic, 2f, true);

        BossFightStart.Raise();
    }

    private void OnBossFightEnd()
    {
        _exitDoor.OpenDoor();
        ServiceLocator.Instance.Get<DialogueSystem>().StartDialogue(_endDialogue);
        ServiceLocator.Instance.Get<MusicManager>().StartSong(_postBossMusic, 2f, true);
        // Set the camera target back to the player
        CameraFollow camera = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerFollowCamera();
        GameObject player = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter().gameObject;
        camera.SetStaticCameraMode(false);
        camera.SetFollowTarget(player.transform);
    }
}

using Services;
using System;

public class PlayerManager : IService
{
    public event Action<PlayerCharacter> OnPlayerCharacterChanged;
    public event Action<PlayerController> OnPlayerControllerChanged;

    private PlayerCharacter _playerCharacter;
    private PlayerController _playerController;
    
    public void SetPlayerCharacter(PlayerCharacter playerCharacter)
    {
        if (_playerCharacter != playerCharacter)
        {
            _playerCharacter = playerCharacter;
            OnPlayerCharacterChanged?.Invoke(_playerCharacter);
        }

    }
    public PlayerCharacter GetPlayerCharacter()
    {
        return _playerCharacter;
    }
    public PlayerController GetPlayerController()
    {
        return _playerController;
    }
    public void SetPlayerController(PlayerController playerController)
    {
        if (_playerController != playerController)
        {
            _playerController = playerController;
            OnPlayerControllerChanged?.Invoke(_playerController);
        }
    }
}
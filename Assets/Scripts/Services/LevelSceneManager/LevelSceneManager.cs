using System;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

public class LevelSceneManager : IService
{
    public event Action LevelLoaded;
    
    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
    public string GetLevelName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public int GetLevelIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
}
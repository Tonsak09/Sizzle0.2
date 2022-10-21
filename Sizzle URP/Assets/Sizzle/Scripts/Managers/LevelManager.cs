using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelManager
{

    private static string currentCavern;
    private static string currentStory;

    public static void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// Load a scene specific by index 
    /// </summary>
    /// <param name="index"></param>
    public static void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    /// <summary>
    /// Loads the default cavern "lobby" area 
    /// </summary>
    /// <param name="cavern"></param>
    public static void LoadScene(string cavern)
    {
        SceneManager.LoadScene(cavern);
    }

    /// <summary>
    /// Loads a specific short story from a cavern section 
    /// </summary>
    /// <param name="cavern"></param>
    /// <param name="shortStory"></param>
    public static void LoadScene(string cavern, string shortStory)
    {
        SceneManager.LoadScene(cavern + shortStory);
    }

    /// <summary>
    /// Reloads the current scene 
    /// </summary>
    public static void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

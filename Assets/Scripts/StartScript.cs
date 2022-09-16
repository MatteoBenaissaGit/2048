using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScript : MonoBehaviour
{
    private void Start()
    {
        LevelUnlockedScript.Level = 1;
        SceneManager.LoadScene("MenuScene");
    }
}

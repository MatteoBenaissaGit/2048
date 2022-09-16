using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPrefabScript : MonoBehaviour
{
    public MenuButtonScript _menuButtonScript;
    [SerializeField] private GameObject _line;

    public void DeactivateLine() => _line.SetActive(false);
}

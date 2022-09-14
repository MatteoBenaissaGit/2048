#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    [BoxGroup("Levels")] [SerializeField] private int _numberOfLevel;

    [BoxGroup("References")] [SerializeField] private GameObject _buttonPrefab;
    [BoxGroup("References")] [SerializeField] private RectTransform _contentScrollView;
    
    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        const float widthAdding = 400f;
        for (int i = 1; i <= _numberOfLevel; i++)
        {
            //instantiate button level
            var button = Instantiate(_buttonPrefab, _contentScrollView.transform);
            var buttonScript = button.GetComponent<MenuButtonScript>();
            //text on button
            
            //if (buttonScript!=null) buttonScript.TextButton.text = i.ToString();
            
            //content width
            _contentScrollView.sizeDelta = new Vector2 (_contentScrollView.rect.width+widthAdding, _contentScrollView.rect.height);
            
            //associate with the right scene
            //buttonScript.SceneNameToGoTo = $"level{i}";
        }
    }
}

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelSelectionManager : MonoBehaviour
{
    [BoxGroup("Levels")] [SerializeField] private int _numberOfLevel;

    [BoxGroup("Parameters")] [SerializeField] private float _cursorOffsetY = 2f;
    [BoxGroup("Parameters")] [SerializeField] private float _cursorAnimationtime = .5f;

    [BoxGroup("References")] [SerializeField] private GameObject _buttonPrefab;
    [BoxGroup("References")] [SerializeField] private RectTransform _contentScrollView;
    [BoxGroup("References")] [SerializeField] private GameObject _cursorPrefab;
    [BoxGroup("References")] [SerializeField] private ScrollRect _scrollRect;

    private List<Transform> _buttonTransformList = new();
    private GameObject _cursor;
    private int _currentLevelSelected = 0;
    private Tween _currentTween;

    private void Start()
    {
        GenerateLevel();
        SetupCursor();
    }

    private void Update()
    {
        CursorInputCheck();
    }

    private void CursorInputCheck()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveCursor(Direction.Left);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveCursor(Direction.Right);
    }

    private void GenerateLevel()
    {
        const float widthAdding = 400f;
        for (int i = 1; i <= _numberOfLevel; i++)
        {
            //instantiate button level
            var button = Instantiate(_buttonPrefab, _contentScrollView.transform);
            var buttonScript = button.GetComponent<ButtonPrefabScript>();
            if (i==_numberOfLevel) buttonScript.DeactivateLine();
            _buttonTransformList.Add(buttonScript.transform);
            //text on button
            if (buttonScript!=null) buttonScript._menuButtonScript.TextButton.text = i.ToString();

            //content width
            _contentScrollView.sizeDelta = new Vector2 (_contentScrollView.rect.width+widthAdding, _contentScrollView.rect.height);
            
            //associate with the right scene
            buttonScript._menuButtonScript.SceneNameToGoTo = $"level{i}";
        }
    }

    private void SetupCursor()
    {
        var cursorPos = new Vector3(_buttonTransformList[0].position.x,_buttonTransformList[0].position.y + _cursorOffsetY, _buttonTransformList[0].transform.position.z);
        _cursor = Instantiate(_cursorPrefab, _buttonTransformList[0]);
        _cursor.transform.position = cursorPos;
        _buttonTransformList[_currentLevelSelected].GetComponent<ButtonPrefabScript>()._menuButtonScript.ScaleUpAnimation();
    }

    private void MoveCursor(Direction direction)
    {
        if (_currentTween.IsActive()) return;
        
        _cursor.transform.DOComplete();
        _buttonTransformList[_currentLevelSelected].GetComponent<ButtonPrefabScript>()._menuButtonScript.ScaleDownAnimation();
        switch (direction)
        {
            case Direction.Left:
                if (_currentLevelSelected == 0) break;
                _currentLevelSelected--;
                _currentTween = _cursor.transform.DOMoveX(_buttonTransformList[_currentLevelSelected].position.x, _cursorAnimationtime).OnComplete(MoveScrollBar);
                break;
            case Direction.Right:
                if (_currentLevelSelected == _numberOfLevel-1) break;
                _currentLevelSelected++;
                _currentTween = _cursor.transform.DOMoveX(_buttonTransformList[_currentLevelSelected].position.x, _cursorAnimationtime).OnComplete(MoveScrollBar);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
        _buttonTransformList[_currentLevelSelected].GetComponent<ButtonPrefabScript>()._menuButtonScript.ScaleUpAnimation();
        _cursor.transform.parent = _buttonTransformList[_currentLevelSelected];
        
    }

    private void MoveScrollBar()
    {
        //offset setup
        float levelNumberHalf = (float)_numberOfLevel / 2f;
        bool isFirstHalf = _currentLevelSelected < levelNumberHalf;
        float offsetValue = isFirstHalf ? _currentLevelSelected / levelNumberHalf - 0.5f : (_currentLevelSelected - levelNumberHalf) / levelNumberHalf + 0.5f;
        float offset = _currentLevelSelected == 0 ? 0 : isFirstHalf ? - offsetValue : offsetValue;
        //animation
        _currentTween = _scrollRect.DOHorizontalNormalizedPos((_currentLevelSelected+offset) / _numberOfLevel, _cursorAnimationtime);
    }
}

public enum Direction
{
    Left,
    Right
}

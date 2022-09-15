#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    [BoxGroup("Levels")] [SerializeField] private int _numberOfLevel;

    [BoxGroup("Parameters")] [SerializeField] private float _cursorOffsetY = 2f;
    [BoxGroup("Parameters")] [SerializeField] private float _cursorAnimationtime = .5f;

    [BoxGroup("References")] [SerializeField] private GameObject _buttonPrefab;
    [BoxGroup("References")] [SerializeField] private RectTransform _contentScrollView;
    [BoxGroup("References")] [SerializeField] private GameObject _cursorPrefab;
    [BoxGroup("References")] [SerializeField] private ScrollRect _scrollRect;

    private Dictionary<Transform,bool> _buttonTransformAndUnlockBoolList = new();
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
        if (Input.GetKeyDown(KeyCode.Return))
            SelectLevel();
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
            
            //setup lock
            bool isUnlocked = i == 1; //setup this with a dont destroy object that got the lock of each level
            _buttonTransformAndUnlockBoolList.Add(buttonScript.transform, isUnlocked);
            if (isUnlocked) buttonScript._menuButtonScript.Unlock();
            if (i==1) buttonScript._menuButtonScript.ScaleUpAnimation();
            
            //text on button
            if (buttonScript != null)
            {
                buttonScript._menuButtonScript.TextButton.text = i.ToString();
                //associate with the right scene
                buttonScript._menuButtonScript.SceneNameToGoTo = $"level{i}";
            }

            //content width
            _contentScrollView.sizeDelta = new Vector2 (_contentScrollView.rect.width+widthAdding, _contentScrollView.rect.height);
        }
    }

    private void SetupCursor()
    {
        var cursorPos = new Vector3(_buttonTransformAndUnlockBoolList.ElementAt(0).Key.position.x,
            _buttonTransformAndUnlockBoolList.ElementAt(0).Key.position.y + _cursorOffsetY, 
            _buttonTransformAndUnlockBoolList.ElementAt(0).Key.transform.position.z);
        _cursor = Instantiate(_cursorPrefab, _buttonTransformAndUnlockBoolList.ElementAt(0).Key);
        _cursor.transform.position = cursorPos;
        GetMenuButtonScriptOfCurrentLevel().ScaleUpAnimation();
    }

    private void MoveCursor(Direction direction)
    {
        if (_currentTween.IsActive()) return;
        
        _cursor.transform.DOComplete();
        if (GetMenuButtonScriptOfCurrentLevel().IsUnlocked)
            GetMenuButtonScriptOfCurrentLevel().ScaleDownAnimation();
        switch (direction)
        {
            case Direction.Left:
                if (_currentLevelSelected == 0) break;
                _currentLevelSelected--;
                _currentTween = _cursor.transform.
                    DOMoveX(_buttonTransformAndUnlockBoolList.ElementAt(_currentLevelSelected).Key.position.x, _cursorAnimationtime).OnComplete(MoveScrollBar);
                break;
            case Direction.Right:
                if (_currentLevelSelected == _numberOfLevel-1) break;
                _currentLevelSelected++;
                _currentTween = _cursor.transform.
                    DOMoveX(_buttonTransformAndUnlockBoolList.ElementAt(_currentLevelSelected).Key.position.x, _cursorAnimationtime).OnComplete(MoveScrollBar);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
        if (GetMenuButtonScriptOfCurrentLevel().IsUnlocked)
            GetMenuButtonScriptOfCurrentLevel().ScaleUpAnimation();
        _cursor.transform.parent = _buttonTransformAndUnlockBoolList.ElementAt(_currentLevelSelected).Key;
        
    }

    private MenuButtonScript GetMenuButtonScriptOfCurrentLevel()
    {
        return _buttonTransformAndUnlockBoolList.ElementAt(_currentLevelSelected).Key.GetComponent<ButtonPrefabScript>()._menuButtonScript;
    }

    private bool IsUnlocked()
    {
        return GetMenuButtonScriptOfCurrentLevel().IsUnlocked;
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

    private void SelectLevel()
    {
        if (IsUnlocked())
            GetMenuButtonScriptOfCurrentLevel().GoToScene();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}

public enum Direction
{
    Left,
    Right
}

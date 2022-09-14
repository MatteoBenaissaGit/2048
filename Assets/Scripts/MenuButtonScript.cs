using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //animation parameters
    [BoxGroup("Button")] [SerializeField] private float _animationSpeed = .5f;
    [BoxGroup("Button")] [EnumToggleButtons] [SerializeField] private AnimationType _animationType = AnimationType.SideSlide;
    private bool _isSlide => _animationType == AnimationType.SideSlide;
    [BoxGroup("Button")] [ShowIf("_isSlide")] [SerializeField] private float _animationOffsetX = .5f;
    [BoxGroup("Button")] [ShowIf("_isSlide")] [SerializeField] private float _positionOffScreenX = -5f;
    private bool _isScale => _animationType == AnimationType.ScaleUp;
    [BoxGroup("Button")] [ShowIf("_isScale")] [SerializeField] private float _animationScale = 1.25f;
    private Vector3 _scaleVector => new(_animationScale, _animationScale, _animationScale);

    //referencing
    [BoxGroup("Referencing")] public string SceneNameToGoTo;
    [BoxGroup("Referencing")] public TextMeshProUGUI TextButton;

    private Tween _currentTween;
    private Vector3 _startPosition;
    
    private void Start()
    {
        _startPosition = transform.position;
        transform.DOComplete();
        switch (_animationType)
        {
            case AnimationType.SideSlide:
                var position = transform.position;
                var basePositionX = position.x;
                position = new Vector3(position.x + _positionOffScreenX, position.y, position.z);
                transform.position = position;
                _currentTween = transform.DOMoveX(basePositionX,_animationSpeed);
                break;
            case AnimationType.ScaleUp:
                transform.localScale = Vector3.one;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleUpAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleDownAnimation();
    }

    //scaling animation functions
    public void ScaleUpAnimation()
    {
        _currentTween = _animationType switch
        {
            AnimationType.SideSlide => transform.DOMoveX(_startPosition.x + _animationOffsetX, _animationSpeed),
            AnimationType.ScaleUp => transform.DOScale(_scaleVector, _animationSpeed),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    public void ScaleDownAnimation()
    {
        _currentTween = _animationType switch
        {
            AnimationType.SideSlide => transform.DOMoveX(_startPosition.x, _animationSpeed),
            AnimationType.ScaleUp => transform.DOScale(Vector3.one, _animationSpeed),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void GoToScene()
    {
        SceneManager.LoadScene(SceneNameToGoTo);
    }
}

public enum AnimationType
{
    SideSlide,
    ScaleUp
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [BoxGroup("Variable")] [SerializeField] private float _animationSpeed = .5f;
    [BoxGroup("Variable")] [SerializeField] private float _animationOffsetX = .5f;
    [BoxGroup("Variable")] [SerializeField] private float _positionOffScreenX = -5f;

    [BoxGroup("Referencing")] [SerializeField] private string _sceneNameToGoTo;
    
    private void Start()
    {
        var basePositionX = transform.position.x;
        transform.position = new Vector3(transform.position.x + _positionOffScreenX, transform.position.y, transform.position.z);
        transform.DOComplete();
        transform.DOMoveX(basePositionX,_animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOComplete();
        transform.DOMoveX(transform.position.x + _animationOffsetX,_animationSpeed);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOComplete();
        transform.DOMoveX(transform.position.x - _animationOffsetX,_animationSpeed);
    }

    public void GoToScene()
    {
        SceneManager.LoadScene(_sceneNameToGoTo);
    }
}

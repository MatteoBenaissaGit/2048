using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class PointEffectController : MonoBehaviour
{
    [BoxGroup("Parameters")] [SerializeField] private float _slerpEffectSpeed = 1f;
    
    private bool _isGoingToPointController = false;
    [HideInInspector] public Vector3 _pointControllerPosition;

    //slerp variables
    private Vector3 _startPosition => transform.position;
    
    private void Update()
    {
        if (!_isGoingToPointController) return;

        transform.position = Vector3.Slerp(_startPosition, _pointControllerPosition, _slerpEffectSpeed);
        if (transform.position == _pointControllerPosition) 
            EndAnimation();
    }

    public void StartPointAnimation()
    {
        const float effectSpeed = .5f;
        transform.localScale = Vector3.zero;
        Vector3 effectSize = new Vector3(.3f, .3f, .3f);
        transform.DOScale(effectSize, effectSpeed).OnComplete(ActivateSlerpToPointController);
    }

    private void ActivateSlerpToPointController()
    {
        _isGoingToPointController = true;
    }

    private void EndAnimation()
    {
        const float effectSpeed = .25f;
        transform.DOScale(Vector3.zero, effectSpeed).OnComplete(DestroyItself);
    }

    private void DestroyItself()
    {
        Destroy(gameObject);
    }
}

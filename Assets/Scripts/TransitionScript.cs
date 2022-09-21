using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    [BoxGroup("Variable")] [SerializeField] private float _movementAmplitudeX;
    [BoxGroup("Variable")] [SerializeField] private float _animSpeed;
    
    private Vector3 _startPosition;

    public void Start()
    {
        StartPosition(MoveType.StartScene);
        _startPosition = transform.position;
    }

    public void StartPosition(MoveType moveType)
    {
        float endMove = 0;
        switch (moveType)
        {
            case MoveType.EndScene:
                transform.position = new Vector3(_startPosition.x - _movementAmplitudeX, 0, 0);
                endMove = _startPosition.x - _movementAmplitudeX;
                transform.DOMoveX(endMove, _animSpeed);
                break;
            case MoveType.StartScene:
                transform.position = _startPosition;
                endMove = transform.position.x + _movementAmplitudeX;
                transform.DOMoveX(endMove, _animSpeed);
                break;
        }
    }
}

public enum MoveType
{
    EndScene,
    StartScene
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [BoxGroup("Variable")] [SerializeField] private float _gearTurnSpeed;
    
    [BoxGroup("References")] [SerializeField] private Transform _gear;
    [BoxGroup("References")] [SerializeField] private TextMeshProUGUI _goalText;
    [BoxGroup("References")] [SerializeField] private GameManager _gameManager;

    private void Start()
    {
        _goalText.text = _gameManager.WinConditionType == WinCondition.ShieldValue
            ? $"obtains a shield of {_gameManager.WinShieldCondition}"
            : $"obtains {_gameManager.WinPointsCondition} points";
    }

    private bool _isTurning = true;

    private void Update()
    {
        GearTurning();
    }

    private void GearTurning()
    {
        if (_isTurning) _gear.transform.Rotate( 0, 0, _gearTurnSpeed);
    }

    public void EndAnim()
    {
        _isTurning = false;
        const float punchSpeed = .25f;
        const float punchForce = .2f;
        Vector3 punchForceVector = new Vector3(punchForce, punchForce, punchForce);
        _gear.transform.DOPunchScale(punchForceVector, punchSpeed);
    }
    
    
}

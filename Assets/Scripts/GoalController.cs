using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [BoxGroup("References")] [SerializeField] private Transform _gear;

    private bool _isTurning = true;

    private void Update()
    {
        GearTurning();
    }

    private void GearTurning()
    {
        const float rotationSpeed = 1f;
        if (_isTurning) _gear.transform.Rotate( 0, 0, 1);
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

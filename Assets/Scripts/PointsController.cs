using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class PointsController : MonoBehaviour, INumericValueController
{
    [BoxGroup("Referencing")] [SerializeField] private TextMeshProUGUI _valueText;

    [BoxGroup("Debug")] [SerializeField] [ReadOnly] private int _valueNumber = 0;
    
    private void Start()
    {
        ControllerResetToZero();
    }

    public void ControllerUpdate(int value)
    { 
        _valueNumber += value;
        //guard over 999999
        if (_valueNumber > 999999) _valueNumber = 999999;
        UpdateText();
    }

    public void ControllerResetToZero()
    {
        _valueNumber = 0;
        UpdateText();
    }

    public void UpdateText()
    {
        _valueText.text = $"{_valueNumber.ToString()}";
    }
}

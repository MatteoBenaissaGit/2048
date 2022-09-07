using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class PointsController : MonoBehaviour, INumericValueController
{
    [BoxGroup("Referencing")] [SerializeField] private TextMeshProUGUI _valueText;

    [BoxGroup("Debug")] [SerializeField] [ReadOnly] public int ValueNumber = 0;
    
    private void Start()
    {
        ControllerResetToZero();
    }

    public void ControllerUpdate(int value)
    { 
        ValueNumber += value;
        //guard over 999999
        if (ValueNumber > 999999) ValueNumber = 999999;
        UpdateText();
    }

    public void ControllerResetToZero()
    {
        ValueNumber = 0;
        UpdateText();
    }

    public void UpdateText()
    {
        _valueText.text = $"{ValueNumber.ToString()}";
    }
}

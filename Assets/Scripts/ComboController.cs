using Sirenix.OdinInspector;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;


public class ComboController : MonoBehaviour, INumericValueController
{
        [BoxGroup("Referencing")] [SerializeField] private TextMeshProUGUI _valueText;

        [BoxGroup("Visual Effects")] [SerializeField] private Color _baseComboColor;
        [BoxGroup("Visual Effects")] [SerializeField] private Color _endComboColor;
        
        [BoxGroup("Debug")] [ReadOnly] public int ValueNumber = 1;

        private void Start()
        {
                ControllerResetToZero();
        }

        public void ControllerUpdate(int value)
        { 
                ValueNumber += value;
                //guard over 99
                if (ValueNumber > 99) ValueNumber = 99;
                UpdateText();
                
                //visual effects
                //punch effect
                float punchEffectScale = (10 + value) / 10;
                float punchEffectSpeed = 0.2f;
                PunchEffectMaker(punchEffectScale, punchEffectSpeed, _valueText.transform);
                //color
                _valueText.color = Color.Lerp(_baseComboColor, _endComboColor, (float)ValueNumber / 10); 

        }

        public void ControllerResetToZero()
        {
                var oldComboNumber = ValueNumber;
                ValueNumber = 0;
                UpdateText();

                //doesnt make effects if the combo was 0 and still 0
                if (ValueNumber == oldComboNumber) return;
                
                //effects
                //punch effect
                const float punchEffectScale = -0.5f;
                float punchEffectSpeed = 0.5f;
                PunchEffectMaker(punchEffectScale, punchEffectSpeed, _valueText.transform);
                //color
                _valueText.color = _baseComboColor;
        }

        public void UpdateText()
        {
                _valueText.text = $"x{ValueNumber.ToString()}";
        }

        private void PunchEffectMaker(float punchValue, float punchSpeed,Transform punchTransform)
        {
                Vector3 punchEffectScaleVector3 = new Vector3(punchValue, punchValue, punchValue);
                float punchEffectDuration = punchSpeed;
                punchTransform.DOComplete();
                punchTransform.DOPunchScale(punchEffectScaleVector3, punchEffectDuration);
        }

        [BoxGroup("Debug")]
        [Button]
        private void AddOneToCombo()
        {
                ControllerUpdate(1);
        }
}

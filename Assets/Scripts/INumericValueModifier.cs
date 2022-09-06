using System;

    public interface INumericValueController
    {
        void ControllerUpdate(int value);
        void ControllerResetToZero();
        void UpdateText();
    }

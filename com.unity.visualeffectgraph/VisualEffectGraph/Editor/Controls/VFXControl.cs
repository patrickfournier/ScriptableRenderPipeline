using UnityEngine;

using System.Collections.Generic;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleEnums;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements;
#endif

namespace UnityEditor.VFX.UIElements
{
    public static class VFXControlConstants
    {
        public const string indeterminateText = "\u2014";
        public static readonly Color indeterminateTextColor = new Color(0.82f, 0.82f, 0.82f);
    }

    public abstract class VFXControl<T> : VisualElement, INotifyValueChanged<T>
    {
        T m_Value;
        public T value
        {
            get { return m_Value; }
            set
            {
                SetValueAndNotify(value);
            }
        }
        public void SetValueAndNotify(T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(value, newValue))
            {
                using (ChangeEvent<T> evt = ChangeEvent<T>.GetPooled(value, newValue))
                {
                    evt.target = this;
                    SetValueWithoutNotify(newValue);
                    SendEvent(evt);
                }
            }
        }

        public void SetValueWithoutNotify(T newValue)
        {
            m_Value = newValue;
            ValueToGUI(false);
        }

        public void ForceUpdate()
        {
            ValueToGUI(true);
        }

        public abstract bool indeterminate {get; set; }

        protected abstract void ValueToGUI(bool force);

        public void OnValueChanged(EventCallback<ChangeEvent<T>> callback)
        {
            RegisterCallback(callback);
        }

        public void RemoveOnValueChanged(EventCallback<ChangeEvent<T>> callback)
        {
            UnregisterCallback(callback);
        }
    }
}

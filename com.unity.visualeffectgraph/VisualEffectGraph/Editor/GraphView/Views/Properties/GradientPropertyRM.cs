using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor.VFX;
using UnityEditor.VFX.UIElements;
using Object = UnityEngine.Object;
using Type = System.Type;


#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements.StyleEnums;
using GradientField = UnityEditor.VFX.UIElements.VFXLabeledField<UnityEditor.UIElements.GradientField, UnityEngine.Gradient>;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using GradientField = UnityEditor.VFX.UIElements.VFXLabeledField<UnityEditor.Experimental.UIElements.GradientField, UnityEngine.Gradient>;
#endif

namespace UnityEditor.VFX.UI
{
    class GradientPropertyRM : PropertyRM<Gradient>
    {
        public GradientPropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
            m_GradientField = new GradientField(m_Label);
            m_GradientField.RegisterCallback<ChangeEvent<Gradient>>(OnValueChanged);

            m_GradientField.style.flexDirection = FlexDirection.Column;
            m_GradientField.style.alignItems = Align.Stretch;
            m_GradientField.style.flex = new Flex(1, 0);

            Add(m_GradientField);
        }

        public override float GetPreferredControlWidth()
        {
            return 120;
        }

        public void OnValueChanged(ChangeEvent<Gradient> e)
        {
            Gradient newValue = m_GradientField.value;
            m_Value = newValue;
            NotifyValueChanged();
        }

        GradientField m_GradientField;

        protected override void UpdateEnabled()
        {
            m_GradientField.SetEnabled(propertyEnabled);
        }

        protected override void UpdateIndeterminate()
        {
            m_GradientField.visible = !indeterminate;
        }

        public override void UpdateGUI(bool force)
        {
            m_GradientField.SetValueWithoutNotify(m_Value);
        }

        public override bool showsEverything { get { return true; } }
    }
}

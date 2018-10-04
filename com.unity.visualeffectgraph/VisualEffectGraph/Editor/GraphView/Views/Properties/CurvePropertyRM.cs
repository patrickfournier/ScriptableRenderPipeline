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
using MyCurveField = UnityEditor.VFX.UIElements.VFXLabeledField<UnityEditor.UIElements.CurveField, UnityEngine.AnimationCurve>;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using MyCurveField = UnityEditor.VFX.UIElements.VFXLabeledField<UnityEditor.Experimental.UIElements.CurveField, UnityEngine.AnimationCurve>;
#endif

namespace UnityEditor.VFX.UI
{
    class CurvePropertyRM : PropertyRM<AnimationCurve>
    {
        public CurvePropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
            m_CurveField = new MyCurveField(m_Label);
            m_CurveField.control.renderMode = CurveField.RenderMode.Mesh;
            m_CurveField.RegisterCallback<ChangeEvent<AnimationCurve>>(OnValueChanged);

            m_CurveField.style.flexDirection = FlexDirection.Column;
            m_CurveField.style.alignItems = Align.Stretch;
#if UNITY_2019_1_OR_NEWER
            m_CurveField.style.flexGrow = 1;
#else
            m_CurveField.style.flex = new Flex(1, 0);
#endif

            Add(m_CurveField);
        }

        public override float GetPreferredControlWidth()
        {
            return 110;
        }

        public void OnValueChanged(ChangeEvent<AnimationCurve> e)
        {
            AnimationCurve newValue = m_CurveField.value;
            m_Value = newValue;
            NotifyValueChanged();
        }

        MyCurveField m_CurveField;

        protected override void UpdateEnabled()
        {
            m_CurveField.SetEnabled(propertyEnabled);
        }

        protected override void UpdateIndeterminate()
        {
            m_CurveField.visible = !indeterminate;
        }

        public override void UpdateGUI(bool force)
        {
            m_CurveField.SetValueWithoutNotify(m_Value);
        }

        public override bool showsEverything { get { return true; } }
    }
}

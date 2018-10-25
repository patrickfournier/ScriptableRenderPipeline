using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.VFX.UI
{
    partial class VFXEditableDataAnchor : VFXDataAnchor
    {
        readonly CustomStyleProperty<Texture2D> SelectedFieldBackgroundProperty = new CustomStyleProperty<Texture2D>("--selected-field-background");
        readonly CustomStyleProperty<int> IMBorderProperty = new CustomStyleProperty<int>("--im-border");
        readonly CustomStyleProperty<int> IMPaddingProperty = new CustomStyleProperty<int>("--im-padding");

        Texture2D m_SelectedFieldBackground;
        int m_IMBorder;
        int m_IMPadding;
        public Texture2D selectedFieldBackground
        {
            get { return m_SelectedFieldBackground; }
        }

        public int IMBorder
        {
            get { return m_IMBorder; }
        }

        public int IMPadding
        {
            get { return m_IMPadding; }
        }

        protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            styles.TryGetValue(SelectedFieldBackgroundProperty, out m_SelectedFieldBackground);
            styles.TryGetValue(IMBorderProperty, out m_IMBorder);
            styles.TryGetValue(IMPaddingProperty, out m_IMPadding);
            /*
            if (m_GUIStyles != null)
            {
                m_GUIStyles.baseStyle.active.background = selectedFieldBackground;
                m_GUIStyles.baseStyle.focused.background = m_GUIStyles.baseStyle.active.background;

                m_GUIStyles.baseStyle.border.top = m_GUIStyles.baseStyle.border.left = m_GUIStyles.baseStyle.border.right = m_GUIStyles.baseStyle.border.bottom = IMBorder;
                m_GUIStyles.baseStyle.padding = new RectOffset(IMPadding, IMPadding, IMPadding, IMPadding);
            }
            */
        }
    }
}

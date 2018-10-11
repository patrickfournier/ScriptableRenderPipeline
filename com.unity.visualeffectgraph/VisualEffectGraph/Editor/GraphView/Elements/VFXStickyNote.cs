using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace UnityEditor.VFX.UI
{
    class VFXStickyNoteController : VFXUIController<VFXUI.StickyNoteInfo>
    {
        public VFXStickyNoteController(VFXViewController viewController, VFXUI ui, int index) : base(viewController, ui, index)
        {
        }

        public string contents
        {
            get
            {
                if (m_Index < 0) return "";

                return m_UI.stickyNoteInfos[m_Index].contents;
            }
            set
            {
                if (m_Index < 0) return;

                m_UI.stickyNoteInfos[m_Index].contents = value;

                Modified();
            }
        }
        override protected VFXUI.StickyNoteInfo[] infos {get {return m_UI.stickyNoteInfos; }}
        public string theme
        {
            get
            {
                return m_UI.stickyNoteInfos[m_Index].theme;
            }
            set
            {
                m_UI.stickyNoteInfos[m_Index].theme = value;
                Modified();
            }
        }
        public string textSize
        {
            get
            {
                return m_UI.stickyNoteInfos[m_Index].textSize;
            }
            set
            {
                m_UI.stickyNoteInfos[m_Index].textSize = value;
                Modified();
            }
        }
    }

    class VFXStickyNote : StickyNote, IControlledElement<VFXStickyNoteController>, IVFXMovable
    {
        public void OnMoved()
        {
#if UNITY_2019_1_OR_NEWER
            controller.position = new Rect(resolvedStyle.left, resolvedStyle.top, resolvedStyle.width, resolvedStyle.height);
#else
            controller.position = new Rect(style.positionLeft, style.positionTop, style.width, style.height);
#endif
        }

        public override void OnResized()
        {
#if UNITY_2019_1_OR_NEWER
            controller.position = new Rect(resolvedStyle.left, resolvedStyle.top, resolvedStyle.width, resolvedStyle.height);
#else
            controller.position = new Rect(style.positionLeft, style.positionTop, style.width, style.height);
#endif
        }

        Controller IControlledElement.controller
        {
            get { return m_Controller; }
        }
        public VFXStickyNoteController controller
        {
            get { return m_Controller; }
            set
            {
                if (m_Controller != null)
                {
                    m_Controller.UnregisterHandler(this);
                }
                m_Controller = value;
                if (m_Controller != null)
                {
                    m_Controller.RegisterHandler(this);
                }
            }
        }

        VFXStickyNoteController m_Controller;
        public VFXStickyNote() : base(Vector2.zero)
        {
            OnChange += OnUIChange;
        }

        void OnUIChange(StickyNodeChangeEvent.Change change)
        {
            if (m_Controller == null) return;

            switch (change)
            {
                case StickyNodeChangeEvent.Change.title:
                    m_Controller.title = title;
                    break;
                case StickyNodeChangeEvent.Change.contents:
                    m_Controller.contents = contents;
                    break;
                case StickyNodeChangeEvent.Change.theme:
                    m_Controller.theme = theme.ToString();
                    break;
                case StickyNodeChangeEvent.Change.textSize:
                    m_Controller.textSize = textSize.ToString();
                    break;
            }
        }

        void IControlledElement.OnControllerEvent(VFXControllerEvent e) {}
        void IControlledElement.OnControllerChanged(ref ControllerChangedEvent e)
        {
            if (m_TitleField != null && !m_TitleField.HasFocus())
                title = controller.title;
            if (m_ContentsField != null && !m_ContentsField.HasFocus())
                contents = controller.contents;

            if (!string.IsNullOrEmpty(controller.theme))
            {
                try
                {
                    theme = (Theme)System.Enum.Parse(typeof(Theme), controller.theme, true);
                }
                catch (System.ArgumentException)
                {
                    controller.theme = Theme.Classic.ToString();
                    Debug.LogError("Unknown theme name");
                }
            }

            if (!string.IsNullOrEmpty(controller.textSize))
            {
                try
                {
                    textSize = (TextSize)System.Enum.Parse(typeof(TextSize), controller.textSize, true);
                }
                catch (System.ArgumentException)
                {
                    controller.theme = TextSize.Medium.ToString();
                    Debug.LogError("Unknown text size name");
                }
            }

            SetPosition(controller.position);
        }
    }
}

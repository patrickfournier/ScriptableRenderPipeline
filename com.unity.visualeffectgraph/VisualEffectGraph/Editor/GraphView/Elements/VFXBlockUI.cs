using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.Profiling;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleSheets;
using UnityEngine.UIElements.StyleEnums;
using UnityEditor.Experimental.GraphView;
#else
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
#endif

namespace UnityEditor.VFX.UI
{
    class VFXBlockUI : VFXNodeUI
    {
        Toggle m_EnableToggle;

        public new VFXBlockController controller
        {
            get { return base.controller as VFXBlockController; }
            set { base.controller = value; }
        }

        public override VFXDataAnchor InstantiateDataAnchor(VFXDataAnchorController controller, VFXNodeUI node)
        {
            VFXContextDataAnchorController anchorController = controller as VFXContextDataAnchorController;

            VFXEditableDataAnchor anchor = VFXBlockDataAnchor.Create(anchorController, node);
            return anchor;
        }
        protected override bool HasPosition()
        {
            return false;
        }

        public VFXContextUI context
        {
            get { return this.GetFirstAncestorOfType<VFXContextUI>(); }
        }

        public VFXBlockUI()
        {
            Profiler.BeginSample("VFXBlockUI.VFXBlockUI");
            AddStyleSheetPath("VFXBlock");
            pickingMode = PickingMode.Position;
            m_EnableToggle = new Toggle();
            m_EnableToggle.RegisterCallback<ChangeEvent<bool>>(OnToggleEnable);
            titleContainer.Insert(1, m_EnableToggle);

            capabilities &= ~Capabilities.Ascendable;
            capabilities |= Capabilities.Selectable;

            //this.AddManipulator(new TrickleClickSelector());

            Profiler.EndSample();
#if UNITY_2019_1_OR_NEWER
            style.position = UnityEngine.UIElements.StyleEnums.Position.Relative;
#else
            style.positionType = PositionType.Relative;
#endif
        }

        // On purpose -- until we support Drag&Drop I suppose
        public override void SetPosition(Rect newPos)
        {
#if UNITY_2019_1_OR_NEWER
            style.position = UnityEngine.UIElements.StyleEnums.Position.Relative;
#else
            style.positionType = PositionType.Relative;
#endif
        }

        void OnToggleEnable(ChangeEvent<bool> e)
        {
            controller.block.enabled = !controller.block.enabled;
        }

        protected override void SelfChange()
        {
            base.SelfChange();

            if (controller.block.enabled)
            {
                titleContainer.RemoveFromClassList("disabled");
            }
            else
            {
                titleContainer.AddToClassList("disabled");
            }

            m_EnableToggle.SetValueWithoutNotify(controller.block.enabled);
            if (inputContainer != null)
                inputContainer.SetEnabled(controller.block.enabled);
            if (settingsContainer != null)
                settingsContainer.SetEnabled(controller.block.enabled);
        }
        public override bool superCollapsed
        {
            get { return false; }
        }
    }
}

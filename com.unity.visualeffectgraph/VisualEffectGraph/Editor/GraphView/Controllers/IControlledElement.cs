using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
#endif

namespace UnityEditor.VFX.UI
{
    interface IControlledElement
    {
        Controller controller
        {
            get;
        }
        void OnControllerChanged(ref ControllerChangedEvent e);
        void OnControllerEvent(VFXControllerEvent e);
    }

    interface IControlledElement<T> : IControlledElement where T : Controller
    {
        new T controller
        {
            get;
        }
    }
    interface ISettableControlledElement<T> where T : Controller
    {
        T controller
        {
            get;
            set;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace UnityEditor.VFX.UI
{
    interface IVFXMovable
    {
        void OnMoved();
    }
    interface IVFXResizable
    {
        void OnStartResize();
        void OnResized();
    }
}

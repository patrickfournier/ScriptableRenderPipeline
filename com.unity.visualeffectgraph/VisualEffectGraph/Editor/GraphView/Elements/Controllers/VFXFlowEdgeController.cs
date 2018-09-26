using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.VFX.UI
{
    internal class VFXFlowEdgeController : VFXEdgeController<VFXFlowAnchorController>
    {
        public VFXFlowEdgeController(VFXFlowAnchorController input, VFXFlowAnchorController output) : base(input, output)
        {
        }
    }
}

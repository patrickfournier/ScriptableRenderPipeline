using System;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [ExecuteAlways]
    public sealed partial class PlanarReflectionProbe : HDProbe
    {
        // Serialized data
        [SerializeField]
        Vector3 m_LocalReferencePosition = -Vector3.forward;

        /// <summary>Reference position to mirror to find the capture point. (local space)</summary>
        public Vector3 localReferencePosition => m_LocalReferencePosition;
        /// <summary>Reference position to mirror to find the capture point. (world space)</summary>
        public Vector3 referencePosition => transform.TransformPoint(m_LocalReferencePosition);

        protected override void Awake()
        {
            base.Awake();
            type = ProbeSettings.ProbeType.PlanarProbe;
            k_Migration.Migrate(this);
        }

        protected override void PopulateSettings(ref ProbeSettings settings)
        {
            base.PopulateSettings(ref settings);

            ComputeTransformRelativeToInfluence(
                out settings.proxySettings.mirrorPositionProxySpace,
                out settings.proxySettings.mirrorRotationProxySpace
            );
        }
    }
}

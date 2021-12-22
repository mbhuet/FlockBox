using UnityEngine;
using UnityEngine.Serialization;
using System;

#if FLOCKBOX_DOTS
using Unity.Transforms;
using CloudFine.FlockBox.DOTS;
using Unity.Entities;
#endif

namespace CloudFine.FlockBox
{
    /// <summary>
    /// As of v2.2, this component behaves identically to Agent unless using DOTS mode.
    /// </summary>
    [Obsolete("Replace with Agent component. As of Flocbox v2.2, ExternalAgent and Agent behave identically.")]
    public class ExternalAgent : Agent
    {
#pragma warning disable 0649
        [SerializeField, FormerlySerializedAs("neighborhood"), HideInInspector, System.Obsolete("use Agent.FlockBox")]
        private FlockBox _autoJoinFlockBox;
#pragma warning restore 0649

        protected void OnValidate()
        {
#pragma warning disable 0618 
            if (FlockBox == null && _autoJoinFlockBox != null) FlockBox = _autoJoinFlockBox;
#pragma warning restore 0618
        }
    }
}

using UnityEngine;
using UnityEngine.Serialization;

namespace CloudFine.FlockBox
{
    public class ExternalAgent : Agent
    {
        [SerializeField, FormerlySerializedAs("neighborhood"), HideInInspector, System.Obsolete("use Agent.FlockBox")]
        private FlockBox _autoJoinFlockBox;


        protected void OnValidate()
        {
            if (FlockBox == null && _autoJoinFlockBox != null) FlockBox = _autoJoinFlockBox;
        }

        protected override void OnJoinFlockBox(FlockBox flockBox)
        {
#if FLOCKBOX_DOTS
            if (flockBox.DOTSEnabled)
            {
                //Create Agent Entity,
                //have it follow this transform
            }
#endif
        }
    }
}

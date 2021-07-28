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

    }
}

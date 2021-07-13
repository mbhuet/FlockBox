using UnityEngine;
using UnityEngine.Serialization;

namespace CloudFine.FlockBox
{
    public class ExternalAgent : Agent
    {
        [SerializeField, FormerlySerializedAs("neighborhood")]
        private FlockBox _autoJoinFlockBox;
        private Vector3 _lastPosition;

        protected override FlockBox AutoFindFlockBox()
        {
            return _autoJoinFlockBox;
        }

        protected override void OnJoinFlockBox(FlockBox flockBox)
        {
            _autoJoinFlockBox = flockBox;
            _lastPosition = transform.position;
        }

        public override void FlockingLateUpdate()
        {
            if (isAlive && transform.hasChanged)
            {
                if (_autoJoinFlockBox != null)
                {
                    Position = WorldToFlockBoxPosition(transform.position);
                    Velocity = WorldToFlockBoxDirection((transform.position - _lastPosition)/Time.deltaTime);
                    ValidateVelocity();
                    if(Velocity != Vector3.zero)
                    {
                        Forward = Velocity.normalized;
                    }
                }
                if (ValidatePosition())
                {
                    FindOccupyingCells();
                }
                else
                {
                    RemoveFromAllCells();
                }
                transform.hasChanged = false;
            }
            _lastPosition = transform.position;

        }
    }
}

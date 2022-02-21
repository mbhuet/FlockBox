using UnityEngine;

namespace CloudFine.FlockBox
{
    public partial class ContainmentBehavior : ForecastSteeringBehavior
    {
        public override bool CanUseTagFilter { get { return false; } }
        public override bool CanToggleActive { get { return false; } }

        private Vector3 containedPosition;
        private Vector3 unclampedPosition;
        private Vector3 worldDimensions;
        private float containmentMargin;


        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if (mine.FlockBox.wrapEdges)
            {
                steer = Vector3.zero;
                return;
            }

            worldDimensions = mine.FlockBox.WorldDimensions;
            containmentMargin = mine.FlockBox.boundaryBuffer;

            unclampedPosition = mine.Position + mine.Velocity * lookAheadSeconds;
            containedPosition = unclampedPosition;

            float distanceToBorder = float.MaxValue;

            if (worldDimensions.x > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, Mathf.Min(mine.Position.x, worldDimensions.x - mine.Position.x));
                containedPosition.x = Mathf.Clamp(containedPosition.x, containmentMargin, worldDimensions.x - containmentMargin);
            }
            else
            {
                containedPosition.x = 0;
            }

            if (worldDimensions.y > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, Mathf.Min(mine.Position.y, worldDimensions.y - mine.Position.y));
                containedPosition.y = Mathf.Clamp(containedPosition.y, containmentMargin, worldDimensions.y - containmentMargin);
            }
            else
            {
                containedPosition.y = 0;
            }

            if (worldDimensions.z > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, Mathf.Min(mine.Position.z, worldDimensions.z - mine.Position.z));
                containedPosition.z = Mathf.Clamp(containedPosition.z, containmentMargin, worldDimensions.z - containmentMargin);
            }
            else
            {
                containedPosition.z = 0;
            }

            if (containedPosition == unclampedPosition)
            {
                steer = Vector3.zero;
                return;
            }
            if (distanceToBorder <= 0) distanceToBorder = .001f;

            mine.GetSeekVector(out steer, containedPosition);
            steer *= containmentMargin / distanceToBorder;
        }
    }
}

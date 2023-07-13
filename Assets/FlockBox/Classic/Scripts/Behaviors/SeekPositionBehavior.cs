using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class SeekPositionBehavior : SteeringBehavior
    {
        public override bool CanUseTagFilter => false;

        public const string seekPositionPropName = "seekPositionID";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if(!mine.HasAgentVector3Property(seekPositionPropName))
            {
                steer = Vector3.zero;
                return;
            }

            mine.GetSeekVector(out steer, mine.GetSeekPosition());

        }
    }

    public static class SeekPositionExtensions
    {
        /// <summary>
        /// Sets the target position for the SeekPosition behavior.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="position">Target position in the local space of the FlockBox.</param>
        public static void SetSeekPosition(this SteeringAgent agent, Vector3 position)
        {
            agent.SetAgentVector3Property(SeekPositionBehavior.seekPositionPropName, position);
        }

        /// <summary>
        /// Gets the target position for the SeekPosition behavior.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="position">Target position in the local space of the FlockBox.</param>
        public static Vector3 GetSeekPosition(this SteeringAgent agent)
        {
            return agent.GetAgentVector3Property(SeekPositionBehavior.seekPositionPropName);
        }


    }
}
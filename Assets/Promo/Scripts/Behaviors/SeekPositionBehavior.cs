using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class SeekPositionBehavior : SteeringBehavior
    {
        public const string seekPositionPropName = "seekPositionID";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if(!mine.HasAgentProperty(seekPositionPropName))
            {
                steer = Vector3.zero;
                return;
            }

            mine.GetSeekVector(out steer, mine.GetSeekPosition());
        }
    }

    public static class SeekPositionExtensions
    {
        public static void SetSeekPosition(this SteeringAgent agent, Vector3 position)
        {
            agent.SetAgentProperty<Vector3>(SeekPositionBehavior.seekPositionPropName, position);
        }

        public static Vector3 GetSeekPosition(this SteeringAgent agent)
        {
            return agent.GetAgentProperty<Vector3>(SeekPositionBehavior.seekPositionPropName);
        }


    }
}
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public partial class WanderBehavior : SteeringBehavior
    {
        [Range(0,360f), Tooltip("The maximum deviation from the current direction of travel that a wander force can be.")]
        public float wanderScope = 90;
        [Tooltip("How quickly the wander force can change direction.")]
        public float wanderIntensity = 1;
        public override bool CanUseTagFilter { get { return false; } }

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            float uniqueId = mine.gameObject.GetInstanceID() * .001f;
            uniqueId = uniqueId*uniqueId;
            Vector3 wanderDirection = new Vector3(
                        (Mathf.PerlinNoise((Time.time * wanderIntensity), uniqueId) - .5f) * wanderScope,
                        (Mathf.PerlinNoise((Time.time * wanderIntensity) + uniqueId, uniqueId) - .5f) * wanderScope,
                        (Mathf.PerlinNoise((Time.time * wanderIntensity) + uniqueId * 2, uniqueId) - .5f) * wanderScope
                );
            steer = mine.ValidateFlockDirection(Quaternion.Euler(wanderDirection) * mine.Forward).normalized * mine.activeSettings.maxForce;
        }



#if UNITY_EDITOR
        public override void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            base.DrawPropertyGizmos(agent, drawLabels);
            Vector3 startHoriz = Quaternion.Euler(0, -wanderScope / 2f, 0) * Vector3.forward;
            Vector3 startVert = Quaternion.Euler(-wanderScope / 2f, 0, 0) * Vector3.forward;

            Vector3 endHoriz = Quaternion.Euler(0, wanderScope / 2f, 0) * Vector3.forward;
            Vector3 endVert = Quaternion.Euler(wanderScope / 2f, 0, 0) * Vector3.forward;


            float wanderRadius = agent.shape.radius * 2f;

            Handles.DrawWireArc(Vector3.zero, Vector3.up, startHoriz, wanderScope, wanderRadius);
            Handles.DrawWireArc(Vector3.zero, Vector3.right, startVert, wanderScope, wanderRadius);

            if (wanderScope < 360)
            {
                Handles.DrawLine(Vector3.zero, startHoriz * wanderRadius);
                Handles.DrawLine(Vector3.zero, endHoriz * wanderRadius);
                Handles.DrawLine(Vector3.zero, startVert * wanderRadius);
                Handles.DrawLine(Vector3.zero, endVert * wanderRadius);
            }

            if (drawLabels)
            {
                Handles.Label(startHoriz * wanderRadius, new GUIContent("Wander Scope"));
            }
        }
#endif

    }


}

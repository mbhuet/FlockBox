/*
MIT License

Copyright (c) 2019 Sebastian Lague

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    public enum VisionQuality
    {
        VERY_HIGH = 1, //360 rays
        HIGH = 2, //180 rays
        MEDIUM = 3, //120 rays
        LOW = 4, //90 rays
        VERY_LOW = 6, //60 rays
    }

    [System.Serializable]
    public partial class ColliderAvoidanceBehavior : ForecastSteeringBehavior
    {
        [Tooltip("Extra clearance space to strive for when avoiding obstacles.")]
        public float clearance;
        [Tooltip("Which Physics Layers should be considered obstacles. Used for Raycasting.")]
        public LayerMask mask = -1;
        [Tooltip("Determines how many raycasts an agent will perform when looking for a clear path.")]
        public VisionQuality visionRayDensity = VisionQuality.VERY_HIGH;
        [Tooltip("Draw lines representing Raycasts this behavior is using to detect obstacles ahead and clear routes (Will only appear in Play mode).")]
        public bool drawVisionRays = false;

        RaycastHit hit;

        public override bool CanUseTagFilter { get { return false; } }

        const int numViewDirections = 360;
        public static Vector3[] Directions
        {
            get
            {
                if (_directions == null)
                {
                    _directions = new Vector3[numViewDirections];

                    float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
                    float angleIncrement = Mathf.PI * 2 * goldenRatio;

                    for (int i = 0; i < numViewDirections; i++)
                    {
                        float t = (float)i / numViewDirections;
                        float inclination = Mathf.Acos(1 - 2 * t);
                        float azimuth = angleIncrement * i;

                        float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                        float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                        float z = Mathf.Cos(inclination);
                        _directions[i] = new Vector3(x, y, z);
                    }
                }
                return _directions;
            }
        }
        private static Vector3[] _directions;

        private const string lastClearDirectionKey = "lastClearDirection";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            float rayDist = lookAheadSeconds * mine.Velocity.magnitude;
            Ray myRay = new Ray(mine.WorldPosition, mine.WorldForward);
            if (!ObstacleInPath(myRay, mine.shape.radius + clearance, rayDist, ref hit, mask))
            {
                steer = Vector3.zero;
                mine.SetAgentVector3Property(lastClearDirectionKey, steer);

                return;
            }
            float hitDist = hit.distance;

            Vector3 lastClearWorldDirection = Vector3.zero;
            if (mine.HasAgentVector3Property(lastClearDirectionKey))
            {
                lastClearWorldDirection = mine.GetAgentVector3Property(lastClearDirectionKey);
            }
            if (lastClearWorldDirection == Vector3.zero)
            {
                lastClearWorldDirection = myRay.direction;
            }

            myRay.direction = lastClearWorldDirection;
            
            steer = ObstacleRays(myRay, mine.shape.radius + clearance, rayDist, ref hit, mask, mine);
            mine.SetAgentVector3Property(lastClearDirectionKey, steer.normalized);
            float smooth = rayDist > 0 ? (1f - (hitDist / rayDist)) : 1f;
            steer = mine.WorldToFlockBoxDirection(steer);
            steer = steer.normalized * mine.activeSettings.maxForce - mine.Velocity;
            steer *= smooth;
        }

        bool ObstacleInPath(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit, LayerMask mask)
        {
            if (Physics.SphereCast(ray.origin, rayRadius, ray.direction, out hit, perceptionDistance, mask))
            {
                return true;
            }
            return false;
        }


        Vector3 forward;
        Quaternion rot;

        Vector3 ObstacleRays(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit, LayerMask mask, SteeringAgent agent)
        {
            forward = ray.direction;
            rot = Quaternion.LookRotation(forward);

            for (int i = 0; i < Directions.Length; i+=(int)visionRayDensity)
            {
                Vector3 dir = rot * (Directions[i]);
                dir = agent.ValidateFlockDirection(dir);
                ray.direction = dir;
                if (!Physics.SphereCast(ray, rayRadius, out hit, perceptionDistance, mask))
                {
                    if(drawVisionRays) Debug.DrawLine(ray.origin, ray.origin + ray.direction.normalized * perceptionDistance, Color.yellow);
                    return dir;
                }
                if(drawVisionRays)Debug.DrawLine(ray.origin, ray.origin + ray.direction.normalized * hit.distance, new Color(1,1,1,.3f));

            }

            return forward;
        }


#if UNITY_EDITOR
        protected override void DrawForecastPerceptionGizmo(SteeringAgent agent, float distance)
        {
            DrawCylinderGizmo(agent.shape.radius + clearance, distance);
            Handles.DrawWireDisc(Vector3.forward * distance, Vector3.forward, agent.shape.radius);
            Handles.Label(Vector3.forward * distance + Vector3.up * agent.shape.radius, new GUIContent("Agent Radius"));
            Handles.Label(Vector3.forward * distance + Vector3.up * (agent.shape.radius + clearance), new GUIContent("Clearance"));


        }
#endif
    }
}

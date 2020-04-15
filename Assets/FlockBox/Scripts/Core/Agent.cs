using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class SurroundingsContainer
    {
        public float perceptionRadius { get; private set; }
        public float lookAheadSeconds { get; private set; }
        public HashSet<string> globalSearchTags { get; private set; }
        public HashSet<Agent> allAgents { get; private set; }
        public List<System.Tuple<Shape, Vector3>> perceptionShapes { get; private set; }

        public SurroundingsContainer()
        {
            globalSearchTags = new HashSet<string>();
            allAgents = new HashSet<Agent>();
            perceptionShapes = new List<System.Tuple<Shape, Vector3>>();
        }

        public void Clear()
        {
            perceptionRadius = 0;
            lookAheadSeconds = 0;
            allAgents.Clear();
            perceptionShapes.Clear();
            globalSearchTags.Clear();
        }

        public void SetMinPerceptionRadius(float radius)
        {
            perceptionRadius = Mathf.Max(radius, perceptionRadius);
        }

        public void SetMinLookAheadSeconds(float seconds)
        {
            lookAheadSeconds = Mathf.Max(lookAheadSeconds, seconds);
        }

        public void AddGlobalSearchTag(string tag)
        {
            globalSearchTags.Add(tag);
        }

        public void AddAgent(Agent a)
        {
            allAgents.Add(a);
        }

        public void AddAgents(HashSet<Agent> agents)
        {
            foreach(Agent a in agents)
            {
                AddAgent(a);
            }
        }

        public void AddPerceptionShape(Shape shape, Vector3 position)
        {
            perceptionShapes.Add(new System.Tuple<Shape, Vector3>(shape,position));
        }
    }

    [System.Serializable]
    public class Agent : MonoBehaviour
    {
        private Vector3 m_position = Vector3.zero;
        /// <summary>
        /// Position in local space.
        /// </summary>
        public Vector3 Position
        {
            get { return m_position; }
            protected set { m_position = value; }
        }
        
        /// <summary>
        /// Position in world space. 
        /// </summary>
        public Vector3 WorldPosition
        {
            get { return FlockBoxToWorldPosition(Position);}
        }

        private Vector3 m_velocity = Vector3.zero;
        /// <summary>
        /// Velocity in local space. Magnitude may be zero.
        /// </summary>
        public Vector3 Velocity
        {
            get { return m_velocity; }
            protected set { m_velocity = value; }
        }

        private Vector3 m_forward = Vector3.forward;
        /// <summary>
        /// Forward in local space. Magnitude will never be zero. Guaranteed to be normalized.
        /// </summary>
        public Vector3 Forward
        {
            get { return m_forward; }
            protected set { m_forward = value.normalized; }
        }

        /// <summary>
        /// Forward in world space.
        /// </summary>
        public Vector3 WorldForward
        {
            get { return FlockBoxToWorldDirection(Forward); }
        }


        private Vector3 LineStartPoint
        {
            get { return Position; }
        }
        private Vector3 LineEndPoint
        {
            get { return Position + Forward * shape.length; }
        }



        [SerializeField][HideInInspector]
        private float m_radius = 1f;
        [HideInInspector][SerializeField]
        private int neighborType;


        protected FlockBox myNeighborhood;


        [SerializeField]
        public Shape shape;
        public bool drawDebug = false;

        protected HashSet<int> buckets = new HashSet<int>();
        protected HashSet<Agent> neighbors;

        protected static Dictionary<int, Agent> agentRegistry;
        protected static int agentCount_static = 0;
        public int agentID { get; protected set; }
        public bool isRegistered { get; protected set; }

        public bool isAlive { get; protected set; }
        public bool isCaught { get; protected set; }
        protected bool hasSpawned = false;

        private float spawnTime;
        protected float age { get { return Time.time - spawnTime; } }

        [SerializeField]
        protected Dictionary<string, object> attributes = new Dictionary<string, object>();
        public object GetAttribute(string name)
        {
            object val;
            if (!attributes.TryGetValue(name, out val))
                return false;
            return val;
        }
        public virtual void SetAttribute(string name, object value)
        {
            if (attributes.ContainsKey(name))
                attributes[name] = value;
            else
            {
                attributes.Add(name, value);
            }
        }
        public void RemoveAttribute(string name)
        {
            attributes.Remove(name);
        }
        public bool HasAttribute(string name)
        {
            return attributes.ContainsKey(name);
        }


        public delegate void AgentEvent(Agent agent);
        public AgentEvent OnCaught;
        public AgentEvent OnCatch;
        public AgentEvent OnKill;
        public AgentEvent OnSpawn;



        protected virtual void LateUpdate()
        {
            if (isAlive && transform.hasChanged)
            {
                Position = transform.localPosition;
                ForceUpdatePosition();
                transform.hasChanged = false;
            }
        }

        protected virtual void Awake()
        {
            if (!isRegistered) RegisterNewAgent();
        }

        protected void Start()
        {
            if (!hasSpawned)
            {
                myNeighborhood = GetComponentInParent<FlockBox>();
                if (myNeighborhood)
                {
                    Spawn(myNeighborhood, myNeighborhood.transform.InverseTransformPoint(transform.position));
                }
                else
                {
                    Debug.LogWarning("Agent " + this.name + " is not a child of a FlockBox object.", this);
                }
            }

            MigrateData();
        }

        private void OnValidate()
        {
            MigrateData();
        }

        private void MigrateData()
        {
            if (shape == null) return;
            //MIGRATION
            if (m_radius != 0)
            {
                shape.radius = m_radius;
                m_radius = 0;
            }
            if (neighborType != 0 && shape.type == 0)
            {
                shape.type = (Shape.ShapeType)neighborType;
                neighborType = 0;
            }
        }

        protected void RegisterNewAgent()
        {
            agentCount_static++;
            agentID = agentCount_static;
            if (agentRegistry == null) agentRegistry = new Dictionary<int, Agent>();
            agentRegistry.Add(agentID, this);
            string name = this.name;
            name = name.Replace("(Clone)", "");
            int underscoreIndex = name.IndexOf('_');
            if (underscoreIndex > -1) name = name.Remove(underscoreIndex);
            name += "_" + agentID;
            this.name = name;

        }

        /// <summary>
        /// Validate Position with current FlockBox. If the FlockBox wraps edges, Position will be wrapped. Otherwise it will be clamped to inside the FlockBox.
        /// </summary>
        /// <returns>False if Position was adjusted.</returns>
        protected bool ValidatePosition()
        {
            if(myNeighborhood)
                return myNeighborhood.ValidatePosition(ref m_position);
            return true;
        }

        /// <summary>
        /// Validate Velocity with current FlockBox. Will zero out any dimension which the FlockBox does not have.
        /// </summary>
        /// <returns>False if Velocity was adjusted.</returns>
        protected bool ValidateVelocity()
        {
            if (myNeighborhood)
                return myNeighborhood.ValidateVelocity(ref m_velocity);
            return true;
        }

        protected virtual void FindNeighborhoodBuckets()
        {
            if(myNeighborhood)
            myNeighborhood.UpdateAgentBuckets(this, buckets, true);
        }

        protected void RemoveFromAllNeighborhoods()
        {
            if(myNeighborhood)
            myNeighborhood.RemoveAgentFromBuckets(this, buckets);
        }

        public virtual void Kill()
        {
            if (OnKill != null) OnKill.Invoke(this);
            isAlive = false;
            hasSpawned = false;
            RemoveFromAllNeighborhoods();
            this.gameObject.SetActive(false);
        }

        public virtual void Spawn(FlockBox neighborhood, Vector3 position)
        {
            if (OnSpawn != null) OnSpawn.Invoke(this);
            gameObject.SetActive(true);
            spawnTime = Time.time;
            isAlive = true;
            hasSpawned = true;
            isCaught = false;
            myNeighborhood = neighborhood;
            transform.SetParent(myNeighborhood.transform);
            this.Position = position;
            ForceUpdatePosition();
        }

        public void Spawn(FlockBox neighborhood)
        {
            Spawn(neighborhood, neighborhood.RandomPosition());
        }

        protected virtual void ForceUpdatePosition()
        {
            ValidatePosition();
            UpdateTransform();
            FindNeighborhoodBuckets();
        }


        public Vector3 FlockBoxToWorldPosition(Vector3 localPos)
        {
            if (myNeighborhood)
            {
                return myNeighborhood.transform.TransformPoint(localPos);
            }
            return localPos;
        }

        public Vector3 FlockBoxToWorldDirection(Vector3 localDir)
        {
            if (myNeighborhood)
            {
                return myNeighborhood.transform.TransformDirection(localDir);
            }
            return localDir;
        }

        public Vector3 WorldToFlockBoxDirection(Vector3 worldDir)
        {
            if (myNeighborhood)
            {
                return myNeighborhood.transform.InverseTransformDirection(worldDir);
            }
            return worldDir;
        }

        public Vector3 WorldToFlockBoxPosition(Vector3 worldPos)
        {
            if (myNeighborhood)
            {
                return myNeighborhood.transform.InverseTransformPoint(worldPos);
            }
            return worldPos;
        }


        /// <summary>
        /// Updates transform to match current Position and Velocity.
        /// </summary>
        protected virtual void UpdateTransform()
        {
            this.transform.localPosition = Position;
            if (Velocity.magnitude > 0)
            {
                transform.localRotation = Quaternion.LookRotation(Velocity.normalized, Vector3.up);
                Forward = Velocity;
            }
            else
            {
                Forward = transform.localRotation * Vector3.forward;
            }
        }

        public virtual void CatchAgent(Agent other)
        {
            if (OnCatch != null) OnCatch.Invoke(this);
            other.CaughtBy(this);
        }


        public virtual void CaughtBy(Agent other)
        {
            isCaught = true;
            if (OnCaught != null) OnCaught.Invoke(this);
        }

        public virtual bool CanBeCaughtBy(Agent agent)
        {
            return isAlive;
        }




        #region ShapeUtils
        public bool Overlaps(Agent other)
        {
            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return other.OverlapsSphere(Position, shape.radius);
                case Shape.ShapeType.SPHERE:
                    return other.OverlapsSphere(Position, shape.radius);
                case Shape.ShapeType.LINE:
                    return other.OverlapsLine(LineStartPoint, LineEndPoint, shape.radius);
                case Shape.ShapeType.CYLINDER:
                    return other.OverlapsLine(LineStartPoint, LineEndPoint, shape.radius);
                default:
                    return other.OverlapsSphere(Position, shape.radius);
            }
        }

        public bool OverlapsSphere(Vector3 center, float radius)
        {
            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return GeometryUtility.SphereOverlap(center, radius, Position, shape.radius);
                case Shape.ShapeType.SPHERE:
                    return GeometryUtility.SphereOverlap(center, radius, Position, shape.radius);
                case Shape.ShapeType.LINE:
                    return GeometryUtility.SphereLineOverlap(center, radius + shape.radius, LineStartPoint, LineEndPoint, ref p1);
                case Shape.ShapeType.CYLINDER:
                    return GeometryUtility.SphereLineOverlap(center, radius + shape.radius, LineStartPoint, LineEndPoint, ref p1);
                default:
                    return GeometryUtility.SphereOverlap(center, radius, Position, shape.radius);
            }
        }
        public bool OverlapsLine(Vector3 start, Vector3 end, float thickness)
        {
            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return GeometryUtility.SphereLineOverlap(Position, shape.radius + thickness, start, end, ref p1);
                case Shape.ShapeType.SPHERE:
                    return GeometryUtility.SphereLineOverlap(Position, shape.radius + thickness, start, end, ref p1);
                case Shape.ShapeType.LINE:
                    return GeometryUtility.LineSegementsIntersect(start, end, LineStartPoint, LineEndPoint, shape.radius + thickness, ref p1, ref p2);
                case Shape.ShapeType.CYLINDER:
                    return GeometryUtility.LineSegementsIntersect(start, end, LineStartPoint, LineEndPoint, shape.radius + thickness, ref p1, ref p2);
                default:
                    return false;
            }
        }


        




        public bool RaycastToShape(Ray ray, float rayRadius, float perceptionDistance, out RaycastHit hit)
        {
            hit = new RaycastHit();

            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return RaycastToSphereShape(ray, rayRadius, perceptionDistance, ref hit);
                case Shape.ShapeType.LINE:
                    return RaycastToLineShape(ray, rayRadius, perceptionDistance, ref hit);
                case Shape.ShapeType.CYLINDER:
                    return RaycastToLineShape(ray, rayRadius, perceptionDistance, ref hit);
                case Shape.ShapeType.SPHERE:
                    return RaycastToSphereShape(ray, rayRadius, perceptionDistance, ref hit);
            }
            return false;
        }

        float mu1, mu2;
        Vector3 p1, p2;
        private float t;
        private Vector3 norm;

        private bool RaycastToSphereShape(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit)
        {

            if (GeometryUtility.RaySphereIntersection(ray, Position, shape.radius + rayRadius, ref t))
            {
                if (t < 0 || t > perceptionDistance) return false;
                hit.point = ray.GetPoint(t);
                
                hit.normal = (hit.point - Position).normalized;
                hit.distance = t;
                return true;
            }
            return false;
        }

        private bool RaycastToLineShape(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit)
        {
            if (GeometryUtility.RayCylinderIntersection(ray, LineStartPoint, LineEndPoint, shape.radius + rayRadius, ref t, ref norm))
            {
                hit.normal = norm;
                hit.point = ray.GetPoint(t);
                hit.distance = t;
                return true;
            }
            
            return false;
        }

        public void FindNormalToSteerAwayFromShape(Ray ray, RaycastHit hit, float clearanceRadius, ref Vector3 normal)
        {
            if (shape.type == Shape.ShapeType.POINT || shape.type == Shape.ShapeType.SPHERE)
            {
                //if inside the sphere, steer out
                if (hit.distance == 0)
                {
                    normal = (ray.origin - Position).normalized;
                    return;
                }
                // approaching the sphere, find perpendicular
                GeometryUtility.SphereLineOverlap(Position, shape.radius, ray.origin, ray.origin + ray.direction, ref p1);
                normal = (p1 - Position).normalized;
            }
            else if (shape.type == Shape.ShapeType.LINE || shape.type == Shape.ShapeType.CYLINDER)
            {
                //inside of cylinder, steer out
                if (hit.distance == 0)
                {
                    normal = hit.normal;
                    return;
                }
                if(GeometryUtility.LinesIntersect(ray.origin, ray.origin + ray.direction, LineStartPoint, LineEndPoint, ref mu1, ref mu2)){
                    p1 = ray.origin + ray.direction * mu1;
                    p2 = Vector3.LerpUnclamped(LineStartPoint, LineEndPoint, mu2);
                    //this is likely a 2d simulation, use hit normal
                    if((p1-p2).sqrMagnitude < .01f)
                    {
                        normal = hit.normal;
                        return;
                    }
                    else if (Vector3.Cross(hit.normal, Forward).sqrMagnitude < 1) //hit a cap in 3D, use hit normal
                    {
                        normal = hit.normal;
                        return;
                    }
                    // approaching the cylinder from the side, find perpendicular
                    normal  = (p1-p2).normalized;

                    return;
                }
                normal = hit.normal;

            }
        }





        #endregion





#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (drawDebug)
            {
                Gizmos.color = Color.grey;
                Gizmos.matrix = this.transform.localToWorldMatrix;
                UnityEditor.Handles.matrix = this.transform.localToWorldMatrix;
                UnityEditor.Handles.color = Color.grey;
                shape.DrawGizmo();
            }
        }
#endif
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace CloudFine
{
    [System.Serializable]
    public class SurroundingsContainer
    {
        public float perceptionRadius = 0;
        public float lookAheadSeconds = 0;
        public List<Agent> allAgents = new List<Agent>();

        public SurroundingsContainer() { }
    }

    [System.Serializable]
    public class Agent : MonoBehaviour
    {

        

        private Vector3 m_position = Vector3.zero;
        public Vector3 Position
        {
            get { return m_position; }
            protected set { m_position = value; }
        }

        private Vector3 m_velocity = Vector3.zero;
        public Vector3 Velocity
        {
            get { return m_velocity; }
            protected set { m_velocity = value; }
        }

        public Vector3 Forward
        {
            get { return transform.localRotation * Vector3.forward; }
        }

        private Vector3 LineStartPoint
        {
            get { return Position; }
        }
        private Vector3 LineEndPoint
        {
            get { return Position + Forward * shape.length; }
        }

        private Vector3 m_acceleration = Vector3.zero;
        public Vector3 Acceleration
        {
            get { return m_acceleration; }
            protected set { m_acceleration = value; }
        }

        [SerializeField][HideInInspector]
        private float m_radius = 1f;
        [HideInInspector][SerializeField]
        private int neighborType;


        protected FlockBox myNeighborhood;


        [SerializeField]
        public Shape shape;
        public bool drawDebug = false;

        protected List<int> buckets;
        protected List<Agent> neighbors;

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

        protected void Start()
        {
            if (!isRegistered) RegisterNewAgent();
            if (!hasSpawned)
            {
                myNeighborhood = GetComponentInParent<FlockBox>();
                if (myNeighborhood)
                {
                    Spawn(myNeighborhood, myNeighborhood.transform.InverseTransformPoint(transform.position));
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
            //MIGRATION
            if (m_radius != default)
            {
                shape.radius = m_radius;
                m_radius = default;
            }
            if (neighborType != default && shape.type == default)
            {
                shape.type = (Shape.ShapeType)neighborType;
                neighborType = default;
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

        protected void ValidatePosition()
        {
            if(myNeighborhood)
            myNeighborhood.ValidatePosition(ref m_position);
        }

        protected void ValidateVelocity()
        {
            if (myNeighborhood)
                myNeighborhood.ValidateVelocity(ref m_velocity);
        }

        protected void FindNeighborhoodBuckets()
        {
            if(myNeighborhood)
            myNeighborhood.UpdateAgentBuckets(this, buckets);
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

        public virtual void Spawn(FlockBox neighborhood)
        {
            Spawn(neighborhood, neighborhood.RandomPosition());
        }

        protected virtual void ForceUpdatePosition()
        {
            ValidatePosition();
            UpdateTransform();
            FindNeighborhoodBuckets();
        }

        protected virtual void UpdateTransform()
        {
            this.transform.localPosition = Position;
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
                    return other.OverlapsLine(Position, LineEndPoint, shape.radius);
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
                    return GeometryUtility.SphereLineOverlap(center, radius + shape.radius, LineStartPoint, LineEndPoint, out mu1, out mu2);
                default:
                    return GeometryUtility.SphereOverlap(center, radius, Position, shape.radius);
            }
        }
        public bool OverlapsLine(Vector3 start, Vector3 end, float thickness)
        {
            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return GeometryUtility.SphereLineOverlap(Position, shape.radius + thickness, start, end, out mu1, out mu2);
                case Shape.ShapeType.SPHERE:
                    return GeometryUtility.SphereLineOverlap(Position, shape.radius + thickness, start, end, out mu1, out mu2);
                case Shape.ShapeType.LINE:
                    return GeometryUtility.LineSegementsIntersect(start, end, LineStartPoint, LineEndPoint, shape.radius + thickness, ref p1, ref p2);
                default:
                    return false;
            }
        }

        


        Vector3 ClosestPointPathToObstacle(SteeringAgent mine, Agent obstacle)
        {
            Vector3 agentPos = mine.Position;
            Vector3 agentToObstacle = obstacle.Position - agentPos;
            Vector3 projection = Vector3.Project(agentToObstacle, mine.Velocity.normalized);
            if (projection.normalized == mine.Velocity.normalized)
                return agentPos + projection;
            else return agentPos;
        }

        public bool RaycastToShape(Ray ray, float perceptionDistance, out RaycastHit hit)
        {
            hit = new RaycastHit();

            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return RaycastToSphereShape(ray, perceptionDistance, ref hit);
                case Shape.ShapeType.LINE:
                    return RaycastToLineShape(ray, perceptionDistance, ref hit);
                case Shape.ShapeType.SPHERE:
                    return RaycastToSphereShape(ray, perceptionDistance, ref hit);
            }
            return false;
        }

        float mu1, mu2;
        Vector3 p1, p2;

        private bool RaycastToSphereShape(Ray ray, float perceptionDistance, ref RaycastHit hit)
        {
            p1 = ray.origin;
            p2 = ray.origin + ray.direction * perceptionDistance;

            if (GeometryUtility.SphereLineOverlap(Position, shape.radius, ray.origin, ray.origin + ray.direction * perceptionDistance, out mu1, out mu2))
            {
                hit.point = Vector3.Lerp(p1, p2, Mathf.Min(mu1, mu2));
                hit.normal = hit.point - Position;
                return true;
            }
            return false;
        }

        private bool RaycastToLineShape(Ray ray, float perceptionDistance, ref RaycastHit hit)
        {
            if (GeometryUtility.LineSegementsIntersect(ray.origin, ray.origin + ray.direction * perceptionDistance, LineStartPoint, LineEndPoint, shape.radius, ref p1, ref p2))
            {
                hit.normal = p2 - p1;
                hit.point = p2 + hit.normal.normalized * shape.radius;
                hit.normal = p2 - p1;
                return true;
            }
            
            else return false;
        }


        #endregion





#if UNITY_EDITOR
        private void OnDrawGizmos()
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
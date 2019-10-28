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

        public enum NeighborType
        {
            POINT, //occupy only one point, one neighborhood
            SPHERE, //occupy all neighborhoods within radius
            LINE, //occupy all neighborhoods along line
            BOX,
        }

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

        private Vector3 m_forward = Vector3.zero;
        public Vector3 Forward
        {
            get { return m_forward; }
            protected set { m_forward = value; }
        }

        private Vector3 m_acceleration = Vector3.zero;
        public Vector3 Acceleration
        {
            get { return m_acceleration; }
            protected set { m_acceleration = value; }
        }

        [SerializeField][HideInInspector]
        private float m_radius = 1f;
        public float Radius
        {
            get { return shape.radius; }
            protected set { shape.radius = value; }
        }


        protected FlockBox myNeighborhood;

        [HideInInspector][SerializeField][FormerlySerializedAs("neighborType")]
        private NeighborType m_neighborType;
        public NeighborType neighborType
        {
            get { return shape.shape; }
        }
        [SerializeField]
        protected Shape shape;
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
            if (m_radius != default && shape.radius == default)
            {
                shape.radius = m_radius;
                m_radius = default;
            }
            if (m_neighborType != default && shape.shape == default)
            {
                shape.shape = m_neighborType;
                m_neighborType = default;
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





#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawDebug)
            {
                Gizmos.color = Color.grey;
                Gizmos.matrix = this.transform.localToWorldMatrix;
                UnityEditor.Handles.matrix = this.transform.localToWorldMatrix;
                switch (neighborType)
                {
                    case NeighborType.BOX:
                        Gizmos.DrawWireCube(Vector3.zero, shape.dimensions);
                        break;
                    case NeighborType.LINE:
                        Gizmos.DrawLine(Vector3.zero, Vector3.forward * shape.length);
                        UnityEditor.Handles.DrawWireDisc(Vector3.forward * shape.length, Vector3.forward, Radius);
                        UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, Radius);

                        break;
                    case NeighborType.POINT:
                        break;
                    case NeighborType.SPHERE:
                        Gizmos.DrawWireSphere(Vector3.zero, Radius);
                        break;

                }
            }
        }
#endif
    }
}
#pragma warning disable 0649
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Serialization;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class Agent : MonoBehaviour, IConvertGameObjectToEntity
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

        
        public FlockBox FlockBox
        {
            get { return _flockBox; }
            protected set { _flockBox = value; }
        }
        
        [SerializeField]
        private FlockBox _flockBox;

        [SerializeField]
        public Shape shape;
        [FormerlySerializedAs("drawDebug")]
        public bool debugDrawShape = false;

        protected HashSet<int> cells = new HashSet<int>();
        protected HashSet<Agent> neighbors;

        protected static Dictionary<int, Agent> agentRegistry;
        protected static int agentCount_static = 0;
        public int agentID { get; protected set; }
        public bool isRegistered { get; protected set; }

        public bool isAlive { get; protected set; }
        public bool isCaught { get; protected set; }
        protected bool hasSpawned = false;

        protected float spawnTime;
        protected float age { get { return Time.time - spawnTime; } }

        #region AgentProperties
        [SerializeField]
        protected Dictionary<string, object> agentProperties = new Dictionary<string, object>();
        public T GetAgentProperty<T>(string name)
        {
            object val;
            if (!agentProperties.TryGetValue(name, out val))
                return default;
            return (T)val;
        }
        public void SetAgentProperty<T>(string name, T value)
        {
            if (agentProperties.ContainsKey(name))
                agentProperties[name] = value;
            else
            {
                agentProperties.Add(name, value);
            }
        }
        public void RemoveAgentProperty(string name)
        {
            agentProperties.Remove(name);
        }
        public bool HasAgentProperty(string name)
        {
            return agentProperties.ContainsKey(name);
        }
    
    #endregion


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
                _flockBox = AutoFindFlockBox();
                if (_flockBox)
                {
                    Spawn(_flockBox, _flockBox.transform.InverseTransformPoint(transform.position));
                }
                else
                {
                    Debug.LogWarning("Agent " + this.name + " is not a child of a FlockBox object.", this);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (hasSpawned || isAlive) Kill();
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

        public static Agent GetAgentById(int id)
        {
            if (agentRegistry == null) return null;
            Agent output;
            if(agentRegistry.TryGetValue(id, out output)){
                return output;
            }
            return null;
        }

        /// <summary>
        /// Validate Position with current FlockBox. If the FlockBox wraps edges, Position will be wrapped. Otherwise it will be clamped to inside the FlockBox.
        /// </summary>
        /// <returns>False if Position was adjusted.</returns>
        protected bool ValidatePosition()
        {
            if(_flockBox)
                return _flockBox.ValidatePosition(ref m_position);
            return true;
        }

        /// <summary>
        /// Validate Velocity with current FlockBox. Will zero out any dimension which the FlockBox does not have.
        /// </summary>
        /// <returns>False if Velocity was adjusted.</returns>
        protected bool ValidateVelocity()
        {
            if (_flockBox)
                return _flockBox.ValidateDirection(ref m_velocity);
            return true;
        }

        public Vector3 ValidateFlockDirection(Vector3 direction)
        {
            if (_flockBox)
            {
                _flockBox.ValidateDirection(ref direction);
            }
            return direction;
        }

        protected virtual FlockBox AutoFindFlockBox()
        {
            return GetComponentInParent<FlockBox>();
        }

        protected virtual void JoinFlockBox(FlockBox flockBox)
        {
            _flockBox = flockBox;
            transform.SetParent(_flockBox.transform);
        }

        protected virtual void FindOccupyingCells()
        {
            if(_flockBox)
            _flockBox.UpdateAgentCells(this, cells, true);
        }

        protected void RemoveFromAllCells()
        {
            if(_flockBox)
            _flockBox.RemoveAgentFromCells(this, cells);
        }

        public virtual void Kill()
        {
            if (OnKill != null) OnKill.Invoke(this);
            isAlive = false;
            hasSpawned = false;
            RemoveFromAllCells();
            this.gameObject.SetActive(false);
        }

        public virtual void Spawn(FlockBox flockBox, Vector3 position)
        {
            if (OnSpawn != null) OnSpawn.Invoke(this);
            gameObject.SetActive(true);
            spawnTime = Time.time;
            isAlive = true;
            hasSpawned = true;
            isCaught = false;
            JoinFlockBox(flockBox);
            this.Position = position;
            ForceUpdatePosition();
        }

        public void Spawn(FlockBox flockBox)
        {
            Spawn(flockBox, flockBox.RandomPosition());
        }

        protected virtual void ForceUpdatePosition()
        {
            ValidatePosition();
            UpdateTransform();
            FindOccupyingCells();
        }


        #region Space Transformation Utils
        public Vector3 FlockBoxToWorldPosition(Vector3 localPos)
        {
            if (_flockBox)
            {
                return _flockBox.transform.TransformPoint(localPos);
            }
            return localPos;
        }

        public Vector3 FlockBoxToWorldDirection(Vector3 localDir)
        {
            if (_flockBox)
            {
                return _flockBox.transform.TransformDirection(localDir);
            }
            return localDir;
        }

        public Vector3 WorldToFlockBoxDirection(Vector3 worldDir)
        {
            if (_flockBox)
            {
                return _flockBox.transform.InverseTransformDirection(worldDir);
            }
            return worldDir;
        }

        public Vector3 WorldToFlockBoxPosition(Vector3 worldPos)
        {
            if (_flockBox)
            {
                return _flockBox.transform.InverseTransformPoint(worldPos);
            }
            return worldPos;
        }
        #endregion


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
            return other.OverlapsSphere(Position, shape.radius);
        }

        public bool OverlapsSphere(Vector3 center, float radius)
        {
            return GeometryUtility.SphereOverlap(center, radius, Position, shape.radius);
        }

        public bool OverlapsLine(Vector3 start, Vector3 end, float thickness)
        {
            switch (shape.type)
            {
                case Shape.ShapeType.POINT:
                    return GeometryUtility.SphereLineOverlap(Position, shape.radius + thickness, start, end, ref p1);
                case Shape.ShapeType.SPHERE:
                    return GeometryUtility.SphereLineOverlap(Position, shape.radius + thickness, start, end, ref p1);
                default:
                    return false;
            }
        }


        




        public bool RaycastToShape(Ray ray, float rayRadius, float perceptionDistance, out RaycastHit hit)
        {
            hit = new RaycastHit();
            return RaycastToSphereShape(ray, rayRadius, perceptionDistance, ref hit);
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

        private bool RaycastToLineShape(Ray ray, float rayRadius, float perceptionDistance, Vector3 lineStart, Vector3 lineEnd, ref RaycastHit hit)
        {
            if (GeometryUtility.RayCylinderIntersection(ray, lineStart, lineEnd, shape.radius + rayRadius, ref t, ref norm))
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
        #endregion


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (debugDrawShape)
            {
                Gizmos.color = Color.grey;
                Gizmos.matrix = this.transform.localToWorldMatrix;
                UnityEditor.Handles.matrix = this.transform.localToWorldMatrix;
                UnityEditor.Handles.color = Color.grey;
                shape.DrawGizmo();
            }
        }
#endif


        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AgentData
            {
                Position = Position,
                Velocity = Velocity,
                Forward = Forward,
                Tag = TagMaskUtility.TagToInt(tag),
                Radius = shape.radius,
                Fill = shape.type == Shape.ShapeType.SPHERE,
            });
        }
    }
}
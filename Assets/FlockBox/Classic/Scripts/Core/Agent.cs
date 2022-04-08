#pragma warning disable 0649
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if FLOCKBOX_DOTS
using Unity.Entities;
using CloudFine.FlockBox.DOTS;
#endif

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public partial class Agent : MonoBehaviour
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

        private Vector3 _lastWorldPosition;

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
        protected Dictionary<string, float> agentFloatProperties;
        protected Dictionary<string, int> agentIntProperties;
        protected Dictionary<string, bool> agentBoolProperties;
        protected Dictionary<string, Vector3> agentVector3Properties;

        //FLOAT
        public float GetAgentFloatProperty(string name)
        {
            if (agentFloatProperties == null) return 0;
            float val;
            if (!agentFloatProperties.TryGetValue(name, out val))
                return 0;
            return val;
        }
        public void SetAgentFloatProperty(string name, float value)
        {
            if (agentFloatProperties == null) agentFloatProperties = new Dictionary<string, float>();
            if (agentFloatProperties.ContainsKey(name)) agentFloatProperties[name] = value;
            else agentFloatProperties.Add(name, value);           
        }
        public void RemoveAgentFloatProperty(string name)
        {
            if(agentFloatProperties != null) agentFloatProperties.Remove(name);
        }
        public bool HasAgentFloatProperty(string name)
        {
            if (agentFloatProperties == null) return false;
            return agentFloatProperties.ContainsKey(name);
        }

        //BOOL
        public bool GetAgentBoolProperty(string name)
        {
            if (agentBoolProperties == null) return false;
            bool val;
            if (!agentBoolProperties.TryGetValue(name, out val))
                return false;
            return val;
        }
        public void SetAgentBoolProperty(string name, bool value)
        {
            if (agentBoolProperties == null) agentBoolProperties = new Dictionary<string, bool>();
            if (agentBoolProperties.ContainsKey(name)) agentBoolProperties[name] = value;
            else agentBoolProperties.Add(name, value);
        }
        public void RemoveAgentBoolProperty(string name)
        {
            if (agentBoolProperties != null) agentBoolProperties.Remove(name);
        }
        public bool HasAgentBoolProperty(string name)
        {
            if (agentBoolProperties == null) return false;
            return agentBoolProperties.ContainsKey(name);
        }

        //INT
        public int GetAgentIntProperty(string name)
        {
            if (agentIntProperties == null) return 0;
            int val;
            if (!agentIntProperties.TryGetValue(name, out val))
                return 0;
            return val;
        }
        public void SetAgentIntProperty(string name, int value)
        {
            if (agentIntProperties == null) agentIntProperties = new Dictionary<string, int>();
            if (agentIntProperties.ContainsKey(name)) agentIntProperties[name] = value;
            else agentIntProperties.Add(name, value);
        }
        public void RemoveAgentIntProperty(string name)
        {
            if (agentIntProperties != null) agentIntProperties.Remove(name);
        }
        public bool HasAgentIntProperty(string name)
        {
            if (agentIntProperties == null) return false;
            return agentIntProperties.ContainsKey(name);
        }

        //VECTOR3
        public Vector3 GetAgentVector3Property(string name)
        {
            if (agentVector3Properties == null) return Vector3.zero;
            Vector3 val;
            if (!agentVector3Properties.TryGetValue(name, out val))
                return Vector3.zero;
            return val;
        }
        public void SetAgentVector3Property(string name, Vector3 value)
        {
            if (agentVector3Properties == null) agentVector3Properties = new Dictionary<string, Vector3>();
            if (agentVector3Properties.ContainsKey(name)) agentVector3Properties[name] = value;
            else agentVector3Properties.Add(name, value);
        }
        public void RemoveAgentVector3Property(string name)
        {
            if (agentVector3Properties != null) agentVector3Properties.Remove(name);
        }
        public bool HasAgentVector3Property(string name)
        {
            if (agentVector3Properties == null) return false;
            return agentVector3Properties.ContainsKey(name);
        }

        #endregion


        public delegate void AgentEvent(Agent agent);
        public AgentEvent OnCaught;
        public AgentEvent OnCatch;
        public AgentEvent OnKill;
        public AgentEvent OnSpawn;

        protected virtual void Awake()
        {
            if (!isRegistered) RegisterNewAgent();
        }

        protected void Start()
        {
            if (!hasSpawned)
            {
                if (!_flockBox)
                {
                    _flockBox = AutoFindFlockBox();
                }
                if (_flockBox)
                {
                    Spawn(_flockBox, transform.position, true);
                }
                else
                {
                    Debug.LogWarning("Agent " + this.name + " is not a child of a FlockBox object.", this);
                }
            }
        }

        public virtual void FlockingUpdate()
        {

        }

        public virtual void FlockingLateUpdate()
        {
            if (isAlive && transform.hasChanged)
            {
                if (FlockBox != null)
                {
                    Position = WorldToFlockBoxPosition(transform.position);
                    Velocity = WorldToFlockBoxDirection((transform.position - _lastWorldPosition) / Time.deltaTime);
                    ValidateVelocity();
                    if (Velocity != Vector3.zero)
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
            _lastWorldPosition = transform.position;

        }  

        public void CompensateForFlockBoxMovement(Matrix4x4 prevFlockBoxLTW)
        {
            Position = WorldToFlockBoxPosition(prevFlockBoxLTW.MultiplyPoint(Position));
            Velocity = WorldToFlockBoxVector(prevFlockBoxLTW.MultiplyVector(Velocity));
            Forward = WorldToFlockBoxVector(prevFlockBoxLTW.MultiplyVector(Forward));
        }


        private void OnDisable()
        {
            RemoveFromAllCells();
        }

        private void OnDestroy()
        {
            if (hasSpawned || isAlive) Kill();
            UnregisterAgent();
        }


        private void RegisterNewAgent()
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

        private void UnregisterAgent()
        {
            if (agentRegistry.ContainsKey(agentID))
            {
                agentRegistry.Remove(agentID);
            }
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

        private void JoinFlockBox(FlockBox flockBox)
        {
            if(flockBox == null)
            {
                Debug.LogWarning("cannot join null flockbox");
                return;
            }
            LeaveFlockBox();
            FlockBox = flockBox;
            FlockBox.RegisterAgentUpdates(this);
            _lastWorldPosition = transform.position;
            OnJoinFlockBox(flockBox);
        }

        protected virtual void OnJoinFlockBox(FlockBox flockBox)
        {

        }

        private void LeaveFlockBox()
        {
            if (FlockBox)
            {
                FlockBox.UnregisterAgentUpdates(this);
                RemoveFromAllCells();
                FlockBox = null;
            }
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
            LeaveFlockBox();
            this.gameObject.SetActive(false);
        }

        public virtual void Spawn(FlockBox flockBox, Vector3 position, bool useWorldSpace = false)
        {
            LeaveFlockBox();
            gameObject.SetActive(true);
            spawnTime = Time.time;
            isAlive = true;
            hasSpawned = true;
            isCaught = false;
            JoinFlockBox(flockBox);
            this.Position = useWorldSpace ? WorldToFlockBoxPosition(position) : position;
            ForceUpdatePosition();
            if (OnSpawn != null) OnSpawn.Invoke(this);
        }

        public void Spawn(FlockBox flockBox)
        {
            Spawn(flockBox, transform.position, true);
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

        public Vector3 FlockBoxToWorldVector(Vector3 localVector)
        {
            if (_flockBox)
            {
                return _flockBox.transform.TransformVector(localVector);
            }
            return localVector;
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

        public Vector3 WorldToFlockBoxVector(Vector3 worldVector)
        {
            if (_flockBox)
            {
                return _flockBox.transform.InverseTransformVector(worldVector);
            }
            return worldVector;
        }
#endregion


        /// <summary>
        /// Updates transform to match current Position and Velocity.
        /// </summary>
        protected virtual void UpdateTransform()
        {
            this.transform.position = FlockBoxToWorldPosition(Position);
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


        
    }

#if FLOCKBOX_DOTS
    public partial class Agent : IConvertGameObjectToEntity
    {
        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, ConvertToAgentData());
        }

        protected AgentData ConvertToAgentData()
        {
            return new AgentData
            {
                Position = Position,
                Velocity = Velocity,
                Forward = Forward,
                Tag = TagMaskUtility.TagToInt(tag),
                Radius = shape.radius,
                Fill = shape.type == Shape.ShapeType.SPHERE,
                UniqueID = (int)(UnityEngine.Random.value * 100000),
            };
        }
    }
#endif
}
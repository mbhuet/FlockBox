using UnityEngine;
using System;
using Unity.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    [CreateAssetMenu(menuName = "BehaviorSettings")]
    public class BehaviorSettings : ScriptableObject
    {
        public static Action<BehaviorSettings> OnSteeringValuesModified;
        public static Action<BehaviorSettings, SteeringBehavior> OnBehaviorAdded;
        public static Action<BehaviorSettings, SteeringBehavior> OnBehaviorValuesModified;
        public static Action<BehaviorSettings, SteeringBehavior> OnBehaviorRemoved;

        public float maxForce = 20;    // Maximum steering force
        public float maxSpeed = 30;    // Maximum speed 

        [SerializeField]
        private ContainmentBehavior containmentBehavior = null;
        public ContainmentBehavior Containment { get { return containmentBehavior; } }

        [SerializeField]
        private SteeringBehavior[] behaviors = new SteeringBehavior[0];
        public SteeringBehavior[] Behaviors { get { return behaviors; } }
        public int NumBehaviors
        {
            get { return behaviors.Length; }
        }

        private void OnEnable()
        {
            OnBehaviorAdded += BehaviorAddDetected;
            //Subscribe to value changes in behaviors
            foreach(SteeringBehavior behavior in Behaviors)
            {
                ListenForBehaviorChanges(behavior);
            }
            if(Containment != null) ListenForBehaviorChanges(Containment);
        }

        private void OnDisable()
        {
            OnBehaviorAdded -= BehaviorAddDetected;
        }

        private void OnValidate()
        {
            if (OnSteeringValuesModified != null) OnSteeringValuesModified.Invoke(this);
        }

        public void BehaviorChangeDetected(SteeringBehavior modBehavior)
        {
            if(OnBehaviorValuesModified != null) OnBehaviorValuesModified.Invoke(this, modBehavior);
        }

        private void BehaviorAddDetected(BehaviorSettings settings, SteeringBehavior behavior)
        {
            if (settings == this)
            {
                ListenForBehaviorChanges(behavior);
            }
        }

        private void ListenForBehaviorChanges(SteeringBehavior behavior)
        {
            behavior.OnValueChanged += BehaviorChangeDetected;
        }


        public SteeringBehavior GetBehavior(int index)
        {
            if (index < 0 || index >= behaviors.Length) return null;
            return behaviors[index];
        }

        public T GetBehavior<T>() where T : SteeringBehavior
        {
            foreach (SteeringBehavior behavior in Behaviors)
            {
                if (behavior.GetType() == typeof(T))
                {
                    return (T)behavior;
                }
            }
            return null;
        }

        public void AddPerceptions(SteeringAgent agent, SurroundingsContainer surroundings)
        {

            surroundings.Clear();
            for (int i = 0; i < behaviors.Length; i++)
            {
                behaviors[i].AddPerception(agent, surroundings);
            }
        }

        #region DOTS

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            foreach (IConvertToComponentData behavior in Behaviors)
            {
                behavior.AddEntityData(entity, dstManager);
            }
            Containment.AddEntityData(entity, dstManager);

            dstManager.AddSharedComponentData(entity, new BehaviorSettingsData { Settings = this });
            dstManager.AddComponentData(entity, new SteeringData { MaxSpeed = maxSpeed, MaxForce = maxForce });
        }

        #endregion

#if UNITY_EDITOR
        public void DrawPropertyGizmos(SteeringAgent agent)
        {

            foreach (SteeringBehavior behavior in behaviors)
            {
                if (behavior.DrawProperties)
                {
                    behavior.DrawPropertyGizmos(agent, !Application.isPlaying);
                }
            }
        }


#endif

    }
}
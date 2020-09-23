using UnityEngine;
using System;
using Unity.Entities;
using CloudFine.FlockBox.DOTS;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
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
            MarkAsChanged();
        }

        public void BehaviorChangeDetected(SteeringBehavior modBehavior)
        {
            if(OnBehaviorValuesModified != null) OnBehaviorValuesModified.Invoke(this, modBehavior);
        }

        public void MarkAsChanged()
        {
            if (OnSteeringValuesModified != null) OnSteeringValuesModified.Invoke(this);
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

        /// <summary>
        /// Used to set the BehaviorSettings of an Entity. Will clean up old ComponentData.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dstManager"></param>
        public void ApplyToEntity(Entity entity, EntityManager dstManager)
        {
            //if this entity already has a behaviorSettingsData, clean it up
            if (dstManager.HasComponent<BehaviorSettingsData>(entity))
            {
                BehaviorSettings oldSettings = dstManager.GetSharedComponentData<BehaviorSettingsData>(entity).Settings;
                if(oldSettings != null)
                {
                    oldSettings.CleanupBehaviorsOnEntity(entity, dstManager);
                }
                dstManager.SetSharedComponentData<BehaviorSettingsData>(entity, new BehaviorSettingsData { Settings = this });
            }
            //otherwise add new component data
            else
            {
                dstManager.AddSharedComponentData(entity, new BehaviorSettingsData { Settings = this });
            }

            if (dstManager.HasComponent<SteeringData>(entity))
            {
                dstManager.SetComponentData(entity, ConvertToComponentData());
            }
            else
            {
                dstManager.AddComponentData(entity, ConvertToComponentData());
            }

            ApplyBehaviorsToEntity(entity, dstManager);   
        }

        private void CleanupBehaviorsOnEntity(Entity entity, EntityManager dstManager)
        {
            return;
            foreach (SteeringBehavior behavior in Behaviors)
            {
                if (behavior is IConvertToComponentData)
                {
                    (behavior as IConvertToComponentData).RemoveEntityData(entity, dstManager);

                }
            }
            Containment.RemoveEntityData(entity, dstManager);
        }

        private void ApplyBehaviorsToEntity(Entity entity, EntityManager dstManager)
        {
            return;
            foreach (SteeringBehavior behavior in Behaviors)
            {
                if (behavior is IConvertToComponentData)
                {
                    (behavior as IConvertToComponentData).AddEntityData(entity, dstManager);

                }
            }
            Containment.AddEntityData(entity, dstManager);
        }

        public bool RequiresComponentData<T>() where T : struct, IComponentData
        {
            return behaviors.Any(x => (x as IConvertToSteeringBehaviorComponentData<T>) != null);
        }

        public SteeringData ConvertToComponentData()
        {
            return new SteeringData { MaxForce = maxForce, MaxSpeed = maxSpeed };
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
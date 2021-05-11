using UnityEngine;
using System;
#if FLOCKBOX_DOTS
using Unity.Entities;
using CloudFine.FlockBox.DOTS;
#endif

namespace CloudFine.FlockBox
{
    [CreateAssetMenu(menuName = "FlockBox/BehaviorSettings")]
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
#if FLOCKBOX_DOTS
#region DOTS

        /// <summary>
        /// Used to apply necessary behavior ComponentData to an Entity
        /// Will reference current BehaviorSettingsData to clean up ComponentData that is no longer needed.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dstManager"></param>
        public void ApplyToEntity(Entity entity, EntityManager dstManager)
        {
            //there could be existing componentdata that needs to be removed from this entity first
            BehaviorSettingsData toClean = dstManager.GetSharedComponentData<BehaviorSettingsData>(entity);
            if(toClean.Settings != null)
            {
                foreach (SteeringBehavior behavior in toClean.Settings.behaviors)
                {
                    IConvertToComponentData convert = (behavior as IConvertToComponentData);
                    if (convert != null)
                    {
                        convert.RemoveEntityData(entity, dstManager);
                    }
                }
            }

            dstManager.SetSharedComponentData(entity, new BehaviorSettingsData { Settings = this });
            dstManager.SetComponentData(entity, new SteeringData { MaxForce = maxForce, MaxSpeed = maxSpeed });
         
            foreach (SteeringBehavior behavior in Behaviors)
            {
                IConvertToComponentData convert = (behavior as IConvertToComponentData);
                if (convert != null)
                {                  
                    convert.AddEntityData(entity, dstManager);                    
                }
            }

            if (Containment.HasEntityData(entity, dstManager))
            {
                Containment.SetEntityData(entity, dstManager);
            }
            else
            {
                Containment.AddEntityData(entity, dstManager);
            }
        }
#endregion
#endif

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
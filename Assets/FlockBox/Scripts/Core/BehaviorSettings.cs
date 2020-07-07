using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using Unity.Entities;
using System.CodeDom;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    [CreateAssetMenu(menuName = "BehaviorSettings")]
    public class BehaviorSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        public Action<BehaviorSettings> OnChanged;
        public Action<BehaviorSettings, SteeringBehavior> OnBehaviorAdded;
        public Action<BehaviorSettings, SteeringBehavior> OnBehaviorModified;
        public Action<BehaviorSettings, SteeringBehavior> OnBehaviorRemoved;

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

        public void OnBeforeSerialize()
        {
            if (OnChanged != null) OnChanged.Invoke(this);
        }

        public void OnAfterDeserialize()
        {
            //throw new NotImplementedException();
        }
#endif

    }
}
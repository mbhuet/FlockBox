using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    [CreateAssetMenu(menuName = "BehaviorSettings")]
    public class BehaviorSettings : ScriptableObject
    {
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
            foreach(SteeringBehavior behavior in Behaviors)
            {
                if(behavior.GetType() == typeof(T))
                {
                    return (T)behavior;
                }
            }
            return null;
        }

        public void AddPerceptions(SurroundingsContainer surroundings)
        {

            surroundings.Clear();
            for(int i=0; i<behaviors.Length; i++)
            {
                behaviors[i].AddPerception(surroundings);
            }
        }

    }
}
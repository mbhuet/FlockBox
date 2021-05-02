using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class SurroundingsContainer
    {
        public float perceptionRadius { get; private set; }
        public float lookAheadSeconds { get; private set; }
        public HashSet<string> globalSearchTags { get; private set; }
        public HashSet<Agent> allAgents { get; private set; }
        public List<System.Tuple<float, Vector3>> perceptionSpheres { get; private set; }

        public SurroundingsContainer()
        {
            globalSearchTags = new HashSet<string>();
            allAgents = new HashSet<Agent>();
            perceptionSpheres = new List<System.Tuple<float, Vector3>>();
        }

        public void Clear()
        {
            perceptionRadius = 0;
            lookAheadSeconds = 0;
            allAgents.Clear();
            perceptionSpheres.Clear();
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
            foreach (Agent a in agents)
            {
                AddAgent(a);
            }
        }

        public void AddPerceptionSphere(float radius, Vector3 position)
        {
            perceptionSpheres.Add(new System.Tuple<float, Vector3>(radius, position));
        }
    }
}
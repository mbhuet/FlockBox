using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CloudFine
{
    [Serializable]
    public struct AgentPopulation
    {
        public Agent prefab;
        public int population;
    }

    [RequireComponent(typeof(FlockBox))]
    public class FlockSpawner : MonoBehaviour
    {
        private FlockBox neighborhood;
        public List<AgentPopulation> startingPopulations;

        private void Awake()
        {
            neighborhood = GetComponent<FlockBox>();
        }
        // Use this for initialization
        void Start()
        {
            foreach(AgentPopulation pop in startingPopulations)
            {
                Spawn(pop);
            }
        }


        void Spawn(AgentPopulation pop)
        {
            if (pop.prefab == null)
            {
                Debug.LogWarning("prefab is null");
                return;
            }
            if(neighborhood == null)
            {
                return;
            }
            for (int i = 0; i < pop.population; i++)
            {
                Agent agent = GameObject.Instantiate<Agent>(pop.prefab);
                agent.Spawn(neighborhood);
            }
        }

    }
}
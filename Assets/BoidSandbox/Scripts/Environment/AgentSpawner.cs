using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class AgentSpawner : MonoBehaviour
    {
        public NeighborhoodCoordinator neighborhood;
        public Agent prefab;
        public int numStartSpawns;

        // Use this for initialization
        void Start()
        {
            Spawn(numStartSpawns);
        }


        void Spawn(int numBoids)
        {
            if (prefab == null)
            {
                Debug.LogWarning("AgentSpawner.prefab is null");
                return;
            }
            if(neighborhood == null)
            {
                return;
            }
            for (int i = 0; i < numBoids; i++)
            {
                Agent agent = GameObject.Instantiate<Agent>(prefab);
                agent.Spawn(neighborhood);
            }
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class FoodPellet : Agent
    {
        private void Awake()
        {
            OnCaught += Relocate;
        }

        void Relocate(Agent other)
        {
            transform.position = myNeighborhood.RandomPosition();
        }

    }
}
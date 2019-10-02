using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class Food : Agent
    {
        private void Awake()
        {
            OnCaught += Relocate;
        }

        void Relocate(Agent other)
        {
            Position = myNeighborhood.RandomPosition();
            ForceUpdatePosition();
        }

    }
}
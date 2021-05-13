using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public class Food : Agent
    {
        protected override void Awake()
        {
            base.Awake();
            OnCaught += Relocate;
        }

        void Relocate(Agent other)
        {
            Position = FlockBox.RandomPosition();
            ForceUpdatePosition();
        }

    }
}
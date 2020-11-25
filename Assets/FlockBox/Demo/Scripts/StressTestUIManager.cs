using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public class StressTestUIManager : MonoBehaviour
    {
        public FlockBox testBox;

        public void SetFidelity(float fidelity)
        {
            if (testBox)
            {
                testBox.sleepChance = 1f - fidelity;
            }
        }


    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine {
    public class StressTestManager : MonoBehaviour
    {
        public FlockBox testBox;

        public void SetSleep(float sleep)
        {
            testBox.sleepChance = sleep;
        }
    }
}
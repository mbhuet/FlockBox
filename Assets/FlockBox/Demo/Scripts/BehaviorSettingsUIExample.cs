using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine
{
    public class BehaviorSettingsUIExample : MonoBehaviour
    {
        public BehaviorSettings mySettings;

        private AlignmentBehavior alignment;
        private CohesionBehavior cohesion;
        private SeparationBehavior separation;

        public Slider alignmentSlider;
        public Slider cohesionSlider;
        public Slider separationSlider;


        // Start is called before the first frame update
        void Start()
        {
            if (mySettings)
            {
                alignment = mySettings.GetBehavior<AlignmentBehavior>();
                cohesion = mySettings.GetBehavior<CohesionBehavior>();
                separation = mySettings.GetBehavior<SeparationBehavior>();

                if (alignment && alignmentSlider)
                {
                    alignmentSlider.SetValueWithoutNotify(alignment.weight);
                }
                if(cohesion && cohesionSlider)
                {
                    cohesionSlider.SetValueWithoutNotify(cohesion.weight);
                }
                if(separation && separationSlider)
                {
                    separationSlider.SetValueWithoutNotify(separation.weight);
                }
            }
        }

        public void SetAlignment(System.Single t)
        {
            if (alignment)
            {
                alignment.weight = t;
            }
        }

        public void SetCohesion(System.Single t)
        {
            if (cohesion)
            {
                cohesion.weight = t;
            }
        }

        public void SetSeparation(System.Single t)
        {
            if (separation)
            {
                separation.weight = t;
            }
        }
    }
}

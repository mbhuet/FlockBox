using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.FlockBox
{
    public class BehaviorSettingsUIExample : MonoBehaviour
    {
        public BehaviorSettings mySettings;
        public float maxWeight = 2f;

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
                    alignmentSlider.value = (alignment.weight / maxWeight);
                }
                if (cohesion && cohesionSlider)
                {
                    cohesionSlider.value = (cohesion.weight / maxWeight);
                }
                if (separation && separationSlider)
                {
                    separationSlider.value = (separation.weight / maxWeight);
                }
            }
        }

        public void SetAlignment(System.Single t)
        {
            if (alignment)
            {
                alignment.weight = t*maxWeight;
                alignment.MarkAsChanged();
                MarkSettingsDirty();
            }
        }

        public void SetCohesion(System.Single t)
        {
            if (cohesion)
            {
                cohesion.weight = t*maxWeight;
                cohesion.MarkAsChanged();
                MarkSettingsDirty();
            }
        }

        public void SetSeparation(System.Single t)
        {
            if (separation)
            {
                separation.weight = t*maxWeight;
                separation.MarkAsChanged();
                MarkSettingsDirty();
            }
        }


        /// <summary>
        /// Will cause settings changes to be saved by "File/Save Project"
        /// </summary>
        private void MarkSettingsDirty()
        {
#if UNITY_EDITOR
            if (mySettings)
            {
                UnityEditor.EditorUtility.SetDirty(mySettings);
            }
#endif
        }
    }
}

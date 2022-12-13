#if FLOCKBOX_DOTS
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS {
    public class DOTSAgentStateDemo : MonoBehaviour
    {

    }

    public class DOTSAgentStateDemoBaker : Baker<DOTSAgentStateDemo>
    {
        public override void Bake(DOTSAgentStateDemo authoring)
        {
            AddComponent(new AgentStateData { State = 0 });
        }
    }
}
#endif
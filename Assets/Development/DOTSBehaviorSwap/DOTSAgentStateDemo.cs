using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS {
    public class DOTSAgentStateDemo : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AgentStateData{ State = 0});
        }
    }
}

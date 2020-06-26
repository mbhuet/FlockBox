using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public interface IConvertToComponentData
{
    void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
}

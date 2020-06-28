using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public interface IConvertToComponentData<T> where T : struct, IComponentData
{
    //void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
    //void UpdateEntityComponentData(Entity entity, EntityManager dstManager);

    T Convert();
}

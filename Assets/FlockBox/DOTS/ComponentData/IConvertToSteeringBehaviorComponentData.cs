using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public interface IConvertToComponentData
{
    void AddEntityData(Entity entity, EntityManager entityManager);
    void SetEntityData(Entity entity, EntityManager entityManager);
    void RemoveEntityData(Entity entity, EntityManager entityManager);
}

public interface IConvertToSteeringBehaviorComponentData<T> : IConvertToComponentData where T : struct, IComponentData, ISteeringBehaviorComponentData
{
    T Convert();
}
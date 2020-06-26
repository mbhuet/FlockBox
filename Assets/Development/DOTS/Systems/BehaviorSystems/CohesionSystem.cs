using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public class CohesionSystem : JobComponentSystem
{
    [BurstCompile]
    struct CohesionJob : IJobForEach<Acceleration, SurroundingsData, CohesionData>
    {
        public float dt;

        public void Execute(ref Acceleration c1, ref SurroundingsData c0, ref CohesionData c2)
        {
            throw new System.NotImplementedException();
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        CohesionJob job = new CohesionJob
        {
            //pass input data into the job
            dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}

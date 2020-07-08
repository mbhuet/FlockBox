using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public class CohesionSystem : JobComponentSystem
{
    [BurstCompile]
    struct CohesionJob : IJobForEach_BCC<NeighborData, Acceleration, CohesionData>
    {
        public float dt;

        public void Execute(DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref CohesionData c2)
        {
            //c1.Value += new float3(0, dt * b0.Length * 100, 0);

            //if (TagMaskUtility.TagInMask(b0[0].Value.Tag, c2.TagMask))
            {

            }
        }
    }

    struct CohesionPerceptionJob : IJobForEach<CohesionData, PerceptionData>
    {
        public void Execute(ref CohesionData c0, ref PerceptionData c1)
        {
            //add perceptions
            c1.perceptionRadius = Mathf.Max(c1.perceptionRadius, c0.Radius);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        CohesionJob job = new CohesionJob
        {
            dt = Time.deltaTime

        };
        return job.Schedule(this, inputDeps);
    }
}

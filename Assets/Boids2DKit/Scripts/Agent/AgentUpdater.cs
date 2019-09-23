using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;

public class AgentUpdater : MonoBehaviour {

    JobHandle flockJobHandle;
    FlockJobStruct job;

    NativeArray<int> nativeAgentIDs;
    TransformAccessArray transAccess = new TransformAccessArray();

    private void Update()
    {
        transAccess = new TransformAccessArray(Agent.Registry.Count);
        nativeAgentIDs = new NativeArray<int>(Agent.Registry.Count,Allocator.TempJob);

        int i = 0;
        foreach(var agent in Agent.Registry)
        {
            transAccess.Add(agent.Value.transform);
            nativeAgentIDs[i] = agent.Key;
            i++;
        }

        job = new FlockJobStruct() {
            ids = nativeAgentIDs,
            deltaTime = Time.deltaTime
        };

        flockJobHandle = job.Schedule(transAccess);
        flockJobHandle.Complete();

        transAccess.Dispose();
        nativeAgentIDs.Dispose();
    }


    [BurstCompile]
    protected struct FlockJobStruct : IJobParallelForTransform
    {
        public NativeArray<int> ids;
        public float deltaTime;


        Vector3 Flock(SurroundingsInfo surroundings, Agent agent)
        {
            Vector3 acceleration = Vector3.zero;
            Vector3 steer = Vector3.zero;
            foreach (SteeringBehavior behavior in agent.activeSettings.Behaviors)
            {
                if (!behavior.IsActive) continue;
                behavior.GetSteeringBehaviorVector(out steer, agent, surroundings);
                steer *= behavior.weight;
                if (behavior.drawVectorLine) Debug.DrawRay(agent.Position, steer, behavior.vectorColor);

                acceleration += (steer);
            }
            return acceleration;
        }

        void IJobParallelForTransform.Execute(int index, TransformAccess transform)
        {
                    Agent agent;

            if (!Agent.Registry.TryGetValue(ids[index], out agent)) return;
            if (!agent.isAlive) return;
            if (agent.activeSettings == null) return;
            if (agent.Frozen) return;

            SurroundingsInfo mySurroundings = new SurroundingsInfo();

        NeighborhoodCoordinator.GetSurroundings(ref mySurroundings, agent.Position, agent.activeSettings.PerceptionDistance);

            Vector3 Velocity = agent.Velocity;
            Velocity += Flock(mySurroundings, agent) * deltaTime;
            Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, agent.activeSettings.maxSpeed * agent.Throttle);

            Vector3 Position = transform.position;

            Position += (Velocity * deltaTime);
            Position = NeighborhoodCoordinator.WrapPosition(Position);

            transform.position = Position;
            float z_rot = transform.rotation.eulerAngles.z;
            if (Velocity.magnitude > 0)
            {
                transform.rotation = (Quaternion.Euler(0, 0, (Mathf.Atan2(Velocity.normalized.y, Velocity.normalized.x) - Mathf.PI * .5f) * Mathf.Rad2Deg));
            }

        }
    }
}

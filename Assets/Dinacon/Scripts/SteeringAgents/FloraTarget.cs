using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraTarget: Target {
    const float energyProductionRate = 1;
    protected Vector3 spawnDirection = Vector3.zero;
    private int numChildren;
    public float propagationInterval = 1;
    public float energy = 1;
    protected int spawnedChildren = 0;
    private int generation = 0;

    public int rapidPropogationToGen = 0;

    public AnimationCurve propagationCurve;

    public override void Spawn(Vector2 position)
    {
        name = "Flora_" + targetID;
        numChildren = randomNumChildren();
        if (spawnDirection == Vector3.zero) spawnDirection = Random.insideUnitCircle.normalized;

        if (rapidPropogationToGen > generation)
        {
            base.Spawn(position);
            InstantPropogation();
        }
        else
        {
            StartCoroutine(GrowToFullSize(position));
        }
    }

    private int randomNumChildren()
    {
        float rand = Random.Range(0f, 1f);
        float eval = propagationCurve.Evaluate(rand);
        int floor = Mathf.FloorToInt(eval);
        Debug.Log(rand + " " + eval + " " + floor);
        return 1;
    }

    public void SetSpawnDirection(Vector3 direction, int generation)
    {
        spawnDirection = direction;
        this.generation = generation;
    }

    public void InstantPropogationToGeneration(int stopGeneration)
    {
        rapidPropogationToGen = stopGeneration;
    }

    protected IEnumerator GrowToFullSize(Vector2 position)
    {
        for(float t = 0; t<1; t+= Time.deltaTime)
        {
            
            visual.transform.localScale = Vector3.one * t;
            yield return null;

        }
        visual.transform.localScale = Vector3.one;
        
        base.Spawn(position);
        StartCoroutine(PropagationRoutine());
    }

    protected void InstantPropogation()
    {
        for(int i = 0; i<numChildren; i++)
        {
            SpawnChild(spawnDirection);
        }
    }

    protected IEnumerator PropagationRoutine()
    {
        yield return new WaitForSeconds(propagationInterval);
        while(spawnedChildren<numChildren && !isCaught)
        {

            SpawnChild(spawnDirection);
            yield return new WaitForSeconds(propagationInterval);

        }
    }

    protected void SpawnChild(Vector3 direction)
    {
        FloraTarget child = GameObject.Instantiate(this) as FloraTarget;
        child.transform.position = this.transform.position + direction.normalized * radius * 2;
        child.SetSpawnDirection(Vector3.zero, generation + 1);
        if (rapidPropogationToGen > generation)
        {
            child.InstantPropogationToGeneration(rapidPropogationToGen);
        }

            spawnedChildren++;

    }

    public override void CaughtBy(SteeringAgent other)
    {
        base.CaughtBy(other);
        float last_nourishment = 0;
        if (other.HasAttribute(EcosystemAgent.energyAttributeName))
        {
            last_nourishment = (float)other.GetAttribute(EcosystemAgent.energyAttributeName);
        }
        other.SetAttribute(EcosystemAgent.energyAttributeName, last_nourishment + energy);
    }

    protected IEnumerator EatenRoutine()
    {
        yield return null;
    }
}

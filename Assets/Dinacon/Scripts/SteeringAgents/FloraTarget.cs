using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraTarget: Target {
    const float energyProductionRate = 1;
    protected Vector3 spawnDirection = Vector3.zero;
    private int numChildren;
    public float propagationInterval = 1;
    public float energy = 1;

    public AnimationCurve propagationCurve;

    public override void Spawn(Vector2 position)
    {
        StartCoroutine(GrowToFullSize(position));
        numChildren = randomNumChildren();
        if (spawnDirection == Vector3.zero) spawnDirection = Random.insideUnitCircle.normalized;
    }

    private int randomNumChildren()
    {
        return 1;
        return Mathf.FloorToInt(propagationCurve.Evaluate(Random.Range(0f, 1f)));
    }

    public void SetSpawnDirection(Vector3 direction)
    {
        spawnDirection = direction;
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
        StartCoroutine(Propagate());
    }

    protected IEnumerator Propagate()
    {
        int spawnedChildren = 0;
        yield return new WaitForSeconds(propagationInterval);
        while(spawnedChildren<numChildren && !isCaught)
        {

            SpawnChild(spawnDirection);
            spawnedChildren++;
            yield return new WaitForSeconds(propagationInterval);

        }
    }

    protected void SpawnChild(Vector3 direction)
    {
        FloraTarget child = GameObject.Instantiate(this) as FloraTarget;
        child.transform.position = this.transform.position + direction.normalized * radius * 2;
        child.SetSpawnDirection(Vector3.zero);
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

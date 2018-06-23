using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Once all of a floraTarget's children have been caught, it is available to be eaten. It will respawn children over time if allowed
/// </summary>
public class FloraTarget: Target {
    const float energyProductionRate = 1;
    protected Vector3 spawnDirection = Vector3.zero;
    private int numChildren;
    public float propagationInterval { get { if (generation == 0) return numChildren; return generation; } }
    public float energy = 1;
    private int generation = 0;
    private const float spawnTurnScope = 90;

    private List<Target> children;
    private List<Vector3> openChildPositions;

    protected bool readyToEat = false;


    public int rapidPropogationToGen = 0;

    public AnimationCurve propagationCurve;

    protected static List<FloraTarget> floraCache;

    public override void Spawn(Vector2 position)
    {
        base.Spawn(position);

        name = "Flora_" + targetID;

        PrepareForChildren();

        if (rapidPropogationToGen > generation)
        {
            InstantPropogation();
            rapidPropogationToGen = 0;
        }
        else
        {
            StartCoroutine(GrowToFullSize(position));
        }
    }

    private int randomNumChildren()
    {
        if (generation == 0) return 4;
        float rand = Random.Range(0f, 1f);
        float eval = propagationCurve.Evaluate(rand);
        int floor = Mathf.FloorToInt(eval);
        return floor;
    }

    public void SetSpawnDirection(Vector3 direction, int generation)
    {
        this.generation = generation;
        float turn = (Mathf.PerlinNoise(generation, 0) - .5f) * spawnTurnScope;
        spawnDirection = Quaternion.AngleAxis(turn, Vector3.forward) * direction;
    }

    public void InstantPropogationToGeneration(int stopGeneration)
    {
        rapidPropogationToGen = stopGeneration;
    }

    protected IEnumerator GrowToFullSize(Vector2 position)
    {
        readyToEat = false;
        for(float t = 0; t<1; t+= Time.deltaTime)
        {
            
            visual.transform.localScale = Vector3.one * t;
            yield return null;

        }
        visual.transform.localScale = Vector3.one;
        readyToEat = true;

        StartCoroutine("DelayedPropagationRoutine");
    }

    protected void InstantPropogation()
    {
        for(int i = 0; i<numChildren; i++)
        {
            SpawnChild();
        }
        readyToEat = true;

    }

    protected IEnumerator DelayedPropagationRoutine()
    {
        yield return new WaitForSeconds(propagationInterval);
        if (!isCaught)
        {
            SpawnChild();

            if (openChildPositions.Count > 0) StartCoroutine("DelayedPropagationRoutine");
        }

    }

    protected void PrepareForChildren()
    {
        if (spawnDirection == Vector3.zero) spawnDirection = Random.insideUnitCircle.normalized;

        numChildren = randomNumChildren();
        children = new List<Target>();
        openChildPositions = new List<Vector3>();

        for (int i = 0; i < numChildren; i++)
        {

            float angleBetweenChildren = spawnTurnScope;
            float childAngle = -angleBetweenChildren * (numChildren - 1) / 2f + angleBetweenChildren * i;
            Quaternion childTurn = Quaternion.AngleAxis(childAngle, Vector3.forward);

            Vector3 childDirection = childTurn * spawnDirection.normalized;
            Vector3 childPosition = this.transform.position + childDirection.normalized * radius * 2;

            openChildPositions.Add(childPosition);
        }
    }

    protected void ChildWasCaught(Target child)
    {
        children.Remove(child);
        openChildPositions.Add(child.position);
        child.OnCaught -= ChildWasCaught;
        StartCoroutine("DelayedPropagationRoutine");
    }

    protected void SpawnChild()
    {
        if (openChildPositions.Count == 0) return;

        Vector3 childPos = openChildPositions[0];
        openChildPositions.RemoveAt(0);

        FloraTarget child = GetFlora();
        child.SetSpawnDirection((childPos - position).normalized, generation + 1);
        child.InstantPropogationToGeneration(rapidPropogationToGen);
        
        child.Spawn(childPos);
        child.transform.position = childPos;

        children.Add(child);
        child.OnCaught += ChildWasCaught;
        
    }

    public override bool CanBePursuedBy(SteeringAgent agent)
    {

        //if not ready to eat, return false
        if (!readyToEat || isCaught) return false;

        //if there are no children, return true
        if (children.Count == 0) return true;

        //if there are edible children, this target should not be eaten
        foreach(Target child in children)
        {
            if (child.CanBePursuedBy(agent)) return false;
        }

        //if there are children and none of them are edible yet, return false
        if (children.Count > 0) return false;

        //default
        return true;
    }

    public override void CaughtBy(SteeringAgent other)
    {
        base.CaughtBy(other);
        NourishAgent(other);
        StopCoroutine("DelayedPropagationRoutine");
        CacheSelf();
    }

    protected void NourishAgent(SteeringAgent agent)
    {
        float last_nourishment = 0;
        if (agent.HasAttribute(EcosystemAgent.energyAttributeName))
        {
            last_nourishment = (float)agent.GetAttribute(EcosystemAgent.energyAttributeName);
        }
        agent.SetAttribute(EcosystemAgent.energyAttributeName, last_nourishment + energy);
    }

    protected FloraTarget GetFlora()
    {
        if (floraCache == null) floraCache = new List<FloraTarget>();
        if (floraCache.Count > 0)
        {
            FloraTarget flora = floraCache[0];
            floraCache.RemoveAt(0);
            flora.gameObject.SetActive(true);
            return flora;
        }
        else return GameObject.Instantiate(this) as FloraTarget;
    }

    protected void CacheSelf()
    {
        if (floraCache == null) floraCache = new List<FloraTarget>();
        gameObject.SetActive(false);
        floraCache.Add(this);
    }

    protected IEnumerator EatenRoutine()
    {
        yield return null;
    }
}

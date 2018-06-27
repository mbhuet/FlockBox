using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Once all of a floraTarget's children have been caught, it is available to be eaten. It will respawn children over time if allowed
/// </summary>
public class FloraTarget: Agent {

    const float energyProductionRate = 1;
    private int numChildren;
    public float propagationInterval = 3;// { get { if (generation == 0) return numChildren; return generation; } }
    public float energy = 1;
    private int generation = 0;
    private const float spawnTurnScope = 90;
    public float lifespan = 10;


    private List<FloraTarget> children;
    private List<Vector3> openChildPositions;

    protected bool readyToEat = false;


    public int rapidPropogationToGen = 0;
    public int cutoffGeneration = 10;
    public float seedChance = .1f;

    public AnimationCurve propagationCurve;

    public Sprite leafSprite;
    public Sprite rootSprite;
    public Sprite seedSprite;
    public FloraSeed seedPrefab;

    private bool hasSeed = false;


    public void Spawn(Vector3 position, int generation, Vector3 forwardDirection, int rapidPropogationCutoff)
    {
        base.Spawn(position);
        this.generation = generation;
        rapidPropogationToGen = rapidPropogationCutoff;
        float turn = (Mathf.PerlinNoise(generation, 0) - .5f) * spawnTurnScope;
        forward = Quaternion.AngleAxis(turn, Vector3.forward) * forwardDirection;

        hasSeed = randomSeedChance();

        PrepareForChildren();
        if (generation == 0)
        {
            visual.SetSprite(rootSprite);
            BeginLifeCountdown();
        }
        else if (hasSeed)
        {
            visual.SetSprite(seedSprite);
        }
        else visual.SetSprite(leafSprite);

        if (rapidPropogationToGen > generation)
        {
            InstantPropogation();
        }
        else
        {
            StartCoroutine(GrowToFullSize(position));
        }
        rapidPropogationToGen = 0;

    }

    public override void Spawn(Vector3 position)
    {
        Spawn(position, 0, Random.insideUnitCircle.normalized, 0);     
    }

    private int randomNumChildren()
    {
        if (hasSeed) return 0;
        if (generation == 0) return 4;
        if (generation == cutoffGeneration) return 0;
        float rand = Random.Range(0f, 1f);
        float eval = propagationCurve.Evaluate(rand);
        int floor = Mathf.FloorToInt(eval);
        return floor;
    }

    private bool randomSeedChance()
    {
        if (generation == 0) return false;
        return Random.Range(0, 1f) < seedChance;
    }

    protected IEnumerator GrowToFullSize(Vector2 position)
    {
        readyToEat = false;
        for(float t = 0; t<1; t+= Time.deltaTime)
        {
            
            visual.SetRootSize(t);
            yield return null;

        }
        visual.SetRootSize(1);
        readyToEat = true;

        for (int i = 0; i < numChildren; i++)
        {
            StartCoroutine("DelayedPropagationRoutine");
        }
    }


    protected void InstantPropogation()
    {
        for(int i = 0; i<numChildren; i++)
        {
            AttemptToSpawnChild();
        }
        visual.SetRootSize(1);
        readyToEat = true;

    }

    protected IEnumerator DelayedPropagationRoutine()
    {
        yield return new WaitForSeconds(propagationInterval + Random.Range(-1f, 1f));
        if (!isCaught)
        {
            AttemptToSpawnChild();
        }

    }

    protected void AttachSeedToAgent(Agent agent)
    {
        FloraSeed seed = GameObject.Instantiate(seedPrefab);
        seed.LatchOntoAgent(agent);

        
    }

    protected void PrepareForChildren()
    {

        numChildren = randomNumChildren();
        children = new List<FloraTarget>();
        openChildPositions = new List<Vector3>();

        for (int i = 0; i < numChildren; i++)
        {

            float angleBetweenChildren = spawnTurnScope;
            float childAngle = -angleBetweenChildren * (numChildren - 1) / 2f + angleBetweenChildren * i;
            Quaternion childTurn = Quaternion.AngleAxis(childAngle, Vector3.forward);

            Vector3 childDirection = childTurn * forward.normalized;
            Vector3 childPosition = this.transform.position + childDirection.normalized * radius * 2;

            openChildPositions.Add(childPosition);
        }
    }

    protected void ChildWasCaught(Agent child)
    {
        FloraTarget floraChild = child as FloraTarget;
        children.Remove(floraChild);
        openChildPositions.Add(floraChild.position);
        child.OnCaught -= ChildWasCaught;
        StartCoroutine("DelayedPropagationRoutine");
    }

    protected void AttemptToSpawnChild()
    {
        if (openChildPositions.Count == 0) return;

        Vector3 childPos = openChildPositions[0];
        openChildPositions.RemoveAt(0);

        FloraTarget child = GetInstance() as FloraTarget;
        
        child.Spawn(childPos, generation + 1, (childPos - position).normalized, rapidPropogationToGen);

        children.Add(child);
        child.OnCaught += ChildWasCaught;
        
    }

    public override bool CanBePursuedBy(Agent agent)
    {
        //roots cannot be eaten
        if (generation == 0) return false;

        //if not ready to eat, return false
        if (!readyToEat || isCaught) return false;

        //if there are no children, return true
        if (children.Count == 0) return true;

        //if there are edible children, this target should not be eaten
        foreach(FloraTarget child in children)
        {
            if (child.CanBePursuedBy(agent)) return false;
        }

        //if there are children and none of them are edible yet, return false
        if (children.Count > 0) return false;

        //default
        return true;
    }

    public override void CaughtBy(Agent other)
    {
        base.CaughtBy(other);
        NourishAgent(other);
        if (hasSeed) AttachSeedToAgent(other);
        StartCoroutine(BlinkDieRoutine());
    }

    public override void Kill()
    {
        DisconnectFromChildren();
        StopAllCoroutines();
        base.Kill();

    }

    protected void DisconnectFromChildren()
    {
        foreach(FloraTarget child in children)
        {
            child.OnCaught -= ChildWasCaught;
            if(child.isActiveAndEnabled)
                child.BeginLifeCountdown();
        }
        children.Clear();

    }

    protected void NourishAgent(Agent agent)
    {
        float last_nourishment = 0;
        if (agent.HasAttribute(FaunaAgent.energyAttributeName))
        {
            last_nourishment = (float)agent.GetAttribute(FaunaAgent.energyAttributeName);
        }
        agent.SetAttribute(FaunaAgent.energyAttributeName, last_nourishment + energy);
    }

    public void BeginLifeCountdown()
    {
        StartCoroutine("LifeCountdownRoutine");
    }

    protected IEnumerator LifeCountdownRoutine()
    {
        yield return new WaitForSeconds(lifespan + Random.Range(-1f, 1f));
        if (!isCaught)
        {
            StartCoroutine(ShrinkDieRoutine());
        }
    }

    protected IEnumerator BlinkDieRoutine()
    {
        RemoveFromLastNeighborhood();
        readyToEat = false;
        visual.Blink(true);
        yield return new WaitForSeconds(1);
        visual.Blink(false);
        Kill();

    }

    protected IEnumerator ShrinkDieRoutine()
    {
        RemoveFromLastNeighborhood();
        visual.Show();
        readyToEat = false;
        for (float t = 1; t > 0; t -= Time.deltaTime)
        {

            visual.SetRootSize(t);
            yield return null;

        }
        visual.SetRootSize(0);
        Kill();
    }
}

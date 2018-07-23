using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Once all of a floraTarget's children have been caught, it is available to be eaten. It will respawn children over time if allowed
/// </summary>
public class FloraTarget: Agent {

    [System.Serializable]
    protected class FloraChildSlot
    {
        public Vector3 position;
        public FloraTarget parent;
        public FloraTarget child;
        public LineRenderer stem;

        public bool IsOccupied { get { return child != null; } }
        public bool IsObstructed { get { return FloraTarget.ChildPositionOverlapsExistingFlora(parent, position); } }
        public bool IsAvailable { get { return !IsOccupied && !IsObstructed; } }

        public FloraChildSlot(FloraTarget parent, Vector3 position)// LineRenderer stem)
        {
            this.parent = parent; this.position = position;
            //this.stem = stem;
            child = null;
        }
    }

    const float energyProductionRate = 1;
    private int numChildren;
    public float propagationInterval = 3;// { get { if (generation == 0) return numChildren; return generation; } }
    public float energy = 1;
    private int generation = 0;
    private const float spawnTurnScope = 90;
    public float lifespan = 10;

    public LineRenderer stemPrefab;


    private List<FloraChildSlot> children;

    protected bool readyToEat = false;

    public int initial_rapidPropogationToGen = 0;
    protected int rapidPropogationToGen = 0;

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
        float turn = (Mathf.PerlinNoise(generation, 0) - .5f) * 180;
        forward = Quaternion.AngleAxis(turn, Vector3.forward) * forwardDirection;

        hasSeed = randomSeedChance();

        PrepareForChildren();
        if (generation == 0)
        {
            visual.SetSprite(rootSprite);
            BeginLifeCountdown();
            RemoveFromLastNeighborhood();
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

    public override void Spawn(Vector3 position, params string[] args)
    {
        bool rapidSpawn = false;
        foreach(string arg in args) {
            if (arg == "initial")
            {
                rapidSpawn = true;
            }
        }
        Spawn(position, 0, Random.insideUnitCircle.normalized, rapidSpawn? initial_rapidPropogationToGen : 0);
    }

    public override void Spawn(Vector3 position)
    {
        Spawn(position, 0, Random.insideUnitCircle.normalized, 0);     
    }

    public override void ForceWrapPosition()
    {
        base.ForceWrapPosition();
        if (children != null)
        {
            foreach (FloraChildSlot slot in children)
            {
                slot.position = NeighborhoodCoordinator.WrapPosition(slot.position);
                //slot.stem.SetPositions(new Vector3[] { position, NeighborhoodCoordinator.ClosestPositionWithWrap(position, slot.position) });
            }
        }
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
        if (generation == 0 || generation == 1) return false;
        return Random.Range(0, 1f) < seedChance;
    }

    protected IEnumerator GrowToFullSize(Vector2 position)
    {
        //readyToEat = false;
        readyToEat = true;
        for(float t = 0; t<1; t+= Time.deltaTime)
        {
            
            visual.SetRootSize(t);
            yield return null;

        }
        visual.SetRootSize(1);
        //readyToEat = true;

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
        children = new List<FloraChildSlot>();

        for (int i = 0; i < numChildren; i++)
        {

            float angleBetweenChildren = spawnTurnScope;
            float childAngle = -angleBetweenChildren * (numChildren - 1) / 2f + angleBetweenChildren * i;
            Quaternion childTurn = Quaternion.AngleAxis(childAngle, Vector3.forward);

            Vector3 childDirection = childTurn * forward.normalized;
            Vector3 childPosition = position + childDirection.normalized * radius * 2;

            //LineRenderer stem = GameObject.Instantiate(stemPrefab) as LineRenderer;
            //stem.SetPositions(new Vector3[] { position, childPosition});

            //stem.SetPositions(new Vector3[] { NeighborhoodCoordinator.ClosestPositionWithWrap(this.position, childPosition), this.position });
            //stem.enabled = false;
            children.Add(new FloraChildSlot(this, childPosition));//, stem));
        }
    }

    protected void ChildWasCaught(Agent child)
    {
        FloraTarget floraChild = child as FloraTarget;
        bool edibleChild = false;
        foreach(FloraChildSlot slot in children)
        {
            if (slot.IsOccupied)
            {
                if (slot.child == child) EmptyChildSlot(slot);
                else edibleChild = true;
            }
        }
        if (!edibleChild) FindNeighborhood();
        StartCoroutine("DelayedPropagationRoutine");
    }

    protected void FillChildSlot(FloraChildSlot slot)
    {
        FloraTarget floraChild = GetInstance() as FloraTarget;
        Vector3 childPos = slot.position;
        floraChild.Spawn(childPos, generation + 1, (childPos - position).normalized, rapidPropogationToGen);
        floraChild.OnCaught += ChildWasCaught;
        //slot.stem.enabled = true;
        slot.child = floraChild;
        RemoveFromLastNeighborhood();
    }

    protected void EmptyChildSlot(FloraChildSlot slot)
    {
        if (!slot.IsOccupied) return;
        slot.child.OnCaught -= ChildWasCaught;
        slot.child = null;
    }

    protected void AttemptToSpawnChild()
    {
        foreach (FloraChildSlot slot in children)
        {
            if (slot.IsAvailable)
            {
                FillChildSlot(slot);
                return;
            }
        }
        
    }

    protected static bool ChildPositionOverlapsExistingFlora(FloraTarget parent, Vector3 childPosition)
    {
        LinkedList<AgentWrapped> neighbors = SteeringBehavior.GetFilteredAgents(
            NeighborhoodCoordinator.GetSurroundings(
                  NeighborhoodCoordinator.WorldPosToNeighborhoodCoordinates(childPosition), parent.radius*2), parent.tag);
        foreach(AgentWrapped flora in neighbors)
        {
            if (Vector3.Distance(flora.wrappedPosition, childPosition) < parent.radius * 2 && flora.agent != parent)
            {
//                Debug.Log(parent.name + " Child Overlap at " + childPosition + " with " + flora.agent.name);
                return true;
            }
        }
        return false;
    }

    public override bool CanBePursuedBy(Agent agent)
    {
        //roots cannot be eaten
        if (generation == 0)
        {
            return false;
        }

        //if not ready to eat, return false
        if (!readyToEat || isCaught)
        {
            return false;
        }

        //if there are no children, return true
        if (numChildren == 0)
        {
            return base.CanBePursuedBy(agent);
        }

        //if there are active children, this target should not be eaten
        foreach(FloraChildSlot childSlot in children)
        {

            if (childSlot.IsOccupied)
            {
  //              Debug.Log(this.name + " has child");
                return false;
            }
        }

        //default
//        Debug.Log(this.name + " default case");

        return base.CanBePursuedBy(agent);
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
        if (children == null) return;
        foreach(FloraChildSlot childSlot in children)
        {
            //GameObject.Destroy(childSlot.stem.gameObject);

            if (!childSlot.IsOccupied) continue;
            if(childSlot.child.isActiveAndEnabled)
                childSlot.child.BeginLifeCountdown();
            EmptyChildSlot(childSlot);
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
//        Debug.Log(this.name + " Life Countdown");
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
        yield return new WaitForSeconds(FaunaAgent.eatTime);
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

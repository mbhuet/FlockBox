using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPellet : Target {

    public float minStatus;
    public float nourishAmount = 1;

    public delegate void FoodPelletEvent(FoodPellet pellet);
    public FoodPelletEvent OnEaten;


    protected new void Start()
    {
        Vector3 pos = transform.position;
        transform.position = ZLayering.GetZLayeredPosition(pos);
        SetMinimumStatusRequirement(minStatus);
        OnCaught += InvokeEatenEvent;
        base.Start();
    }

    public override bool CanBePursuedBy(Agent agent)
    {
        bool sufficientStatus = true;
        if (agent.HasAttribute(SocialStatusBehavior.statusAttributeName))
        {
            float status = (float)agent.GetAttribute(SocialStatusBehavior.statusAttributeName);
            sufficientStatus = status >= minStatus;
        }
        return sufficientStatus && base.CanBePursuedBy(agent) ;
    }

    public override void CaughtBy(Agent agent)
    {
        base.CaughtBy(agent);
        Nourish(agent);
    }

    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        StartCoroutine(GrowRoutine());
        //SetMinimumStatusRequirement(Random.Range(0, 100));
    }

    private IEnumerator GrowRoutine()
    {
        float size = 0;
        while (size < .9f)
        {
            size = Mathf.Lerp(size, 1, Time.deltaTime * 3f);
            visual.transform.localScale = Vector3.one * size;
            yield return null;
        }

        visual.transform.localScale = Vector3.one;


    }

    public void SetMinimumStatusRequirement(float status)
    {
        minStatus = status;
        float height = (TallVisual.StatusToHeight(minStatus) - 1);
        visual.transform.localPosition = Vector3.up * height;
    }

    void Nourish(Agent agent)
    {
        if (agent.HasAttribute(SocialStatusBehavior.statusAttributeName))
        {
            float status = (float)agent.GetAttribute(SocialStatusBehavior.statusAttributeName);
            status += nourishAmount;
            agent.SetAttribute(SocialStatusBehavior.statusAttributeName, status);
        }
    }

    private void InvokeEatenEvent(Agent target)
    {
        if (OnEaten != null) OnEaten.Invoke(this);
    }

}

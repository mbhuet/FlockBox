using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPlant : MonoBehaviour {
    public FoodPellet pelletPrefab;
    public Transform stalkTransform;

    List<FoodPellet> activePellets = new List<FoodPellet>();
    List<FoodPellet> cachedPellets = new List<FoodPellet>();

    

    protected float minStatus = 0;

    public delegate void PlantEvent(FoodPlant plant);
    public PlantEvent OnExhausted;


    public void Grow(Vector2 position, float status)
    {
        transform.position = ZLayering.GetZLayeredPosition(position);
        minStatus = status;
        float height = (TallVisual.StatusToHeight(minStatus) - 1);
        StartCoroutine(GrowToHeight(height));
    }

    protected void Whither()
    {
        StartCoroutine(WhitherRoutine());
    }

    protected void SetPlantHeight(float height)
    {
        stalkTransform.localPosition = Vector3.up * height / 2;
        stalkTransform.localScale = new Vector3(.1f, height, 1);
    }

    protected void AddPelletToCache(FoodPellet pellet)
    {
        activePellets.Remove(pellet);
        cachedPellets.Add(pellet);

        if(activePellets.Count == 0 && !currentlySpawningPellets)
        {
            Whither();
        }
    }

    protected void SpawnPellet(Vector2 position)
    {
        FoodPellet pellet = GetFreshFoodPellet();
        pellet.Spawn(position);
        pellet.SetMinimumStatusRequirement(minStatus);
        activePellets.Add(pellet);

    }

    protected FoodPellet CreateNewPellet()
    {
        FoodPellet pellet = GameObject.Instantiate(pelletPrefab);
        pellet.OnEaten += AddPelletToCache;
        return pellet;
    }

    protected FoodPellet GetFreshFoodPellet()
    {
        if (cachedPellets.Count == 0) return CreateNewPellet();

        FoodPellet pellet = cachedPellets[0];
        cachedPellets.Remove(pellet);
        return pellet;
    }

    protected IEnumerator GrowToHeight(float height)
    {
         
        float curHeight = 0;
        float targetHeight = height;
        float t = 0;
        while (curHeight<targetHeight-.2f)
        {
            t += Time.deltaTime;
            //curHeight = Mathf.SmoothDamp(curHeight, targetHeight, ref yVelocity, smoothTime);
            curHeight = Mathf.Lerp(curHeight, targetHeight, Time.deltaTime * 2);
            SetPlantHeight(curHeight);
            yield return null;
        }
        SetPlantHeight(targetHeight);
       
        StartCoroutine(SpawnPelletsRoutine(Mathf.FloorToInt(minStatus/10)+1));
    }

    private bool currentlySpawningPellets = false;
    protected IEnumerator SpawnPelletsRoutine(int numPellets)
    {
        currentlySpawningPellets = true;
        float polarAngle = 0;
        float arcLength_per_pellet = .75f;
        float desiredCircumference = arcLength_per_pellet* numPellets;
        float polarDist = desiredCircumference / (2 * Mathf.PI);
        for(int i = 0; i<numPellets; i++)
        {
            yield return new WaitForSeconds(.1f);

            polarAngle = (2 * Mathf.PI) / numPellets * i;
            Vector3 pelletPos = transform.position + new Vector3(Mathf.Cos(polarAngle) * polarDist, Mathf.Sin(polarAngle) * polarDist, 0);
            SpawnPellet(pelletPos);

        }

        currentlySpawningPellets = false;
    }

    protected IEnumerator WhitherRoutine()
    {

        float curHeight = stalkTransform.localScale.y;
        float targetHeight = 0;
        float t = 0;
        while (curHeight>.2f)
        {
            t += Time.deltaTime;
            //curHeight = Mathf.SmoothDamp(curHeight, targetHeight, ref yVelocity, smoothTime);
            curHeight = Mathf.Lerp(curHeight, targetHeight, Time.deltaTime * 2);

            SetPlantHeight(curHeight);
            yield return null;
        }
        SetPlantHeight(targetHeight);

        yield return null;
        if (OnExhausted != null) OnExhausted.Invoke(this);
    }
}

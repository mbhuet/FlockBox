using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraSeed : MonoBehaviour {

    public Agent rootPrefab;
    protected Vector3 latchPoint;
    public float fallOffDistance = 10;

    private Agent seedHost;

    public void LatchOntoAgent(Agent host)
    {
        //this host already has a seed attached
        if(host.GetComponentInChildren<FloraSeed>() != null)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        transform.parent = host.transform;
        transform.localPosition = Vector3.zero;
        latchPoint = host.transform.position;
        host.OnKill += OnHostKill;
        seedHost = host;
    }

    private void Update()
    {
        if(Vector3.Distance(latchPoint, transform.position) > fallOffDistance)
        {
            Plant();
        }
    }

    void OnHostKill(Agent host)
    {
        Plant();
    }

    void Plant()
    {
        if(seedHost!=null)seedHost.OnKill -= OnHostKill;
        Agent root = rootPrefab.GetInstance();
        root.Spawn(transform.position);
        GameObject.Destroy(this.gameObject);
    }
}

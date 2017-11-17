using UnityEngine;
using System.Collections;

public class ZLayer : MonoBehaviour
{
    public Transform rootTransform; //alays at height 0
    private const float z_spread = 3;
    // Update is called once per frame

    private void Start()
    {
        if (rootTransform == null) rootTransform = transform.root;
    }

    void Update()
    {
        UpdateSpriteZPosition();
    }

    void UpdateSpriteZPosition()
    {
        float z_pos = (rootTransform.position.y) * z_spread;
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, z_pos);
        this.transform.position = newPos;
        Debug.Log(transform.position + " " + newPos);
    }
}

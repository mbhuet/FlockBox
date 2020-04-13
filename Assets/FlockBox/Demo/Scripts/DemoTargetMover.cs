using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTargetMover : MonoBehaviour
{
    public Transform[] positions;
    public float moveTime = 5f;

    private float time;
    private int positionIndex;

    private void Start()
    {
        if (positions.Length == 0)
        {
            enabled = false;
            return;
        }
        MoveToPosition(0);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time >= moveTime)
        {
            MoveToPosition(positionIndex + 1);
            time = 0;
        }
    }

    void MoveToPosition(int posIndex)
    {
        if (posIndex >= positions.Length) posIndex = 0;
        if (posIndex < 0) posIndex = positions.Length - 1;
        positionIndex = posIndex;
        if (positions[posIndex] != null)
        {
            transform.position = positions[posIndex].position;
        }
    }
}

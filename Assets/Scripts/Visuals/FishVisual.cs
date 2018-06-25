using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishVisual : AgentVisual {

    public Animator animator;

    protected void Start()
    {
        base.Start();
        animator.Play(0, -1, Random.Range(0f, 1f));
    }
}

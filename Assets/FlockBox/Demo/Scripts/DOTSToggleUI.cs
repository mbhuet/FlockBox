using CloudFine.FlockBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTSToggleUI : MonoBehaviour
{
    public GameObject DOTSBundle;
    public GameObject BasicBundle;

    private void Awake()
    {
        SetDOTSEnabled(true);
    }

    public void SetDOTSEnabled(bool dots)
    {
        DOTSBundle.SetActive(dots);
        BasicBundle.SetActive(!dots);
    }
}

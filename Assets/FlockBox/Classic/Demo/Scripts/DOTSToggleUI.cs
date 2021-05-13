using UnityEngine;
using UnityEngine.UI;

public class DOTSToggleUI : MonoBehaviour
{
    public GameObject DOTSBundle;
    public GameObject BasicBundle;
    public Toggle dotsToggle;

    private void Awake()
    {
#if FLOCKBOX_DOTS
        SetDOTSEnabled(true);
#else
        SetDOTSEnabled(false);
        dotsToggle.interactable = false;
        dotsToggle.isOn = (false);
#endif

    }

    public void SetDOTSEnabled(bool dots)
    {
        DOTSBundle.SetActive(dots);
        BasicBundle.SetActive(!dots);
    }
}

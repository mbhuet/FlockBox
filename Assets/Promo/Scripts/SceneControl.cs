using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControl : MonoBehaviour
{
    private static SceneControl Instance;

    private void Awake()
    {
        if (!Instance || Instance == this)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadScene(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadScene(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            LoadScene(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            LoadScene(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            LoadScene(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            LoadScene(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            LoadScene(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            LoadScene(9);
        }
    }

    public void LoadScene(int scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}

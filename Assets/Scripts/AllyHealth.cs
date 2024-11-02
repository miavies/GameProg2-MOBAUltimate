using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllyHealth : MonoBehaviour
{
    public Slider targetSlider3D;
    Stats statsScript;

    // Start is called before the first frame update
    void Start()
    {
        statsScript = GetComponentInParent<Stats>();
        if (statsScript != null)
        {
            targetSlider3D.maxValue = statsScript.maxHealth;
            statsScript.health = statsScript.maxHealth;
        }
        else
        {
            Debug.LogWarning("Stats component not found on parent.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (statsScript != null)
        {
            targetSlider3D.value = statsScript.health;
        }
    }
}

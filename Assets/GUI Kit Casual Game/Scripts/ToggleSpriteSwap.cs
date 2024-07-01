using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSpriteSwap : MonoBehaviour
{
    [SerializeField] Image ToggleOn;
    [SerializeField] Image ToggleOff;

    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        OnToggle();
    }

    public void OnToggle()
    {
        if(toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle.isOn)
        {
            ToggleOn.enabled = true;
            ToggleOff.enabled = false;
        }
        else
        {
            ToggleOn.enabled = false;
            ToggleOff.enabled = true;
        }
    }
}

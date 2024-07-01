using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirUnit : MonoBehaviour
{
    public float FillAmount;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Tube"))
        {
            GetComponentInParent<AirUnitsManager>().SpawnAirUnits(1);
            GetComponentInParent<AirUnitsManager>().Remove(gameObject);
        }
    }
}

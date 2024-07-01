using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirUnitsManager : MonoBehaviour
{
    public static AirUnitsManager Instance;

    public float SafeAreaSizeX;
    public float SafeAreaSizeY;
    public float SafeAreaSizeZ;
    public float distanceFromNearestAirUnit;
    public int maxTries;
    public int NumberOfAirUnits;
    public GameObject AirUnitPrefab;

    private List<GameObject> CurrentAirUnits;

    private void Awake()
    {
        Instance = this;
        CurrentAirUnits = new List<GameObject>();
    }

    private void Start()
    {
        SpawnAirUnits(NumberOfAirUnits);
    }

    public void ChangeAirUnitPosition(GameObject airUnit)
    {
        Vector3 newSpawnLocation = Vector3.zero;
        int tryCount = 0;

        while (tryCount < maxTries)
        {
            tryCount++;

            bool positionGood = true;
            newSpawnLocation = new Vector3(Random.Range(-SafeAreaSizeX, SafeAreaSizeX), Random.Range(-SafeAreaSizeY, SafeAreaSizeY), Random.Range(-SafeAreaSizeZ, SafeAreaSizeZ)) + transform.position;

            Collider[] colliders = Physics.OverlapSphere(newSpawnLocation, distanceFromNearestAirUnit);

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Tube") || collider.CompareTag("Player") || collider.CompareTag("Wall"))
                {
                    positionGood = false;
                    break;
                }
            }

            if (positionGood)
                break;
        }

        airUnit.transform.position = newSpawnLocation;
    }

    public void SpawnAirUnits(int numberOfUnits)
    {
        while (numberOfUnits > 0)
        {
            numberOfUnits--;

            Vector3 newSpawnLocation = Vector3.zero;
            int tryCount = 0;

            while (tryCount < maxTries)
            {
                tryCount++;

                bool positionGood = true;
                newSpawnLocation = new Vector3(Random.Range(-SafeAreaSizeX, SafeAreaSizeX), Random.Range(-SafeAreaSizeY, SafeAreaSizeY), Random.Range(-SafeAreaSizeZ, SafeAreaSizeZ));

                Collider[] colliders = Physics.OverlapSphere(newSpawnLocation, distanceFromNearestAirUnit);

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Air") || collider.CompareTag("Player") || collider.CompareTag("Bot") || collider.CompareTag("Tube") || collider.CompareTag("Wall"))
                    {
                        positionGood = false;
                        break;
                    }
                }

                if (positionGood)
                    break;
            }

            CurrentAirUnits.Add(Instantiate(AirUnitPrefab, newSpawnLocation, Quaternion.identity, transform));
        }
    }

    public void Remove(GameObject airUnit)
    {
        CurrentAirUnits.Remove(airUnit);
        Destroy(airUnit);
    }

    public GameObject GetRandomAirUnit()
    {
        return CurrentAirUnits[Random.Range(0, CurrentAirUnits.Count)];
    }
}

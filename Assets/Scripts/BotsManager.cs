using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotsManager : MonoBehaviour
{
    public static BotsManager Instance;

    public float SafeAreaSizeX;
    public float SafeAreaSizeY;
    public float SafeAreaSizeZ;
    public float distanceFromNearestPlayer;
    public int maxTries;
    public int MaxConcurrentBots;
    public int TotalNumOfBots;
    public GameObject BotPrefab;

    public List<Material> BotMaterials;
    public List<GameObject> ActiveBots;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ActiveBots = new List<GameObject>();
        SpawnBot(MaxConcurrentBots);
    }

    public void SpawnBot(int numberOfUnits)
    {
        TotalNumOfBots -= numberOfUnits;
        if (TotalNumOfBots < 0)
            return;

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

                Collider[] colliders = Physics.OverlapSphere(newSpawnLocation, distanceFromNearestPlayer);

                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Bot") || collider.CompareTag("Player"))
                    {
                        positionGood = false;
                        break;
                    }
                }

                if (positionGood)
                    break;
            }

            GameObject obj = Instantiate(BotPrefab, newSpawnLocation, Quaternion.identity, transform);
            ActiveBots.Add(obj);
            obj.GetComponentInChildren<AgentController>().tube.GetComponent<MeshRenderer>().material = BotMaterials[UnityEngine.Random.Range(0, BotMaterials.Count)];
        }
    }

    public void RemoveBotFromActiveList(GameObject bot)
    {
        ActiveBots.Remove(bot);
    }
}

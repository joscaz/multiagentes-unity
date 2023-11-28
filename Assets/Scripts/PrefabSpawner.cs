using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{

    public List<GameObject> prefabsToSpawn; // Assign your prefabs in the Inspector
    public Vector3 spawnPosition; // Position where prefabs will spawn
    public float movementLimit = 685f; // M limit for prefab movement
    public float movementSpeed = 15f;
    public float minSpawnTime = 1f; // Minimum time interval between spawns (a)
    public float maxSpawnTime = 5f; // Maximum time interval between spawns (b)

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPrefabs());
    }

    private IEnumerator SpawnPrefabs()
    {
        while (true) // Infinite loop to keep spawning prefabs
        {
            GameObject prefab = Instantiate(GetRandomPrefab(), spawnPosition, Quaternion.identity);
            StartCoroutine(MoveAndDestroyPrefab(prefab));

            float spawnTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(spawnTime);
        }
    }


    private GameObject GetRandomPrefab()
    {
        int randomIndex = Random.Range(0, prefabsToSpawn.Count);
        return prefabsToSpawn[randomIndex];
    }

    private IEnumerator MoveAndDestroyPrefab(GameObject prefab)
    {
        while (prefab.transform.position.z < movementLimit)
        {
            // Move the prefab. Modify this line to change movement behavior
            prefab.transform.position += Vector3.forward * movementSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(prefab); // Destroy the prefab when it reaches the limit
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

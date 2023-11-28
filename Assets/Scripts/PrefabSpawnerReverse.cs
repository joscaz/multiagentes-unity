using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PrefabSpawnerReverse : MonoBehaviour
{
    public List<GameObject> prefabsToSpawn; // Assign your prefabs in the Inspector
    public float minSpawnTime = 1f; // Minimum time interval between spawns
    public float maxSpawnTime = 5f; // Maximum time interval between spawns
    public Vector3 spawnPosition; // Original position where prefabs would spawn
    public float movementLimit = 10f; // M limit for prefab movement on Z-axis (new spawn position)
    public float movementSpeed = 5f; // Speed at which prefabs move
    public float destructionZPosition = 5f; // Z position where prefabs should be destroyed

    private void Start()
    {
        StartCoroutine(SpawnPrefabs());
    }

    private IEnumerator SpawnPrefabs()
    {
        while (true) // Infinite loop to keep spawning prefabs
        {
            Vector3 spawnAtLimitPosition = new Vector3(spawnPosition.x, spawnPosition.y, movementLimit);
            Quaternion rotation = Quaternion.Euler(0, 180, 0); // Rotation 180 degrees around Y-axis
            GameObject prefab = Instantiate(GetRandomPrefab(), spawnAtLimitPosition, rotation);
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
        while (prefab.transform.position.z > destructionZPosition)
        {
            // Move the prefab towards the original spawn position
            prefab.transform.position += Vector3.back * movementSpeed * Time.deltaTime;
            yield return null;
        }

        Destroy(prefab); // Destroy the prefab when it reaches Z position 5
    }
}

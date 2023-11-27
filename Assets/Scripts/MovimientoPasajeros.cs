using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PositionData
{
    public string id;
    public int[] position;
}

[System.Serializable]
public class Step
{
    public List<PositionData> positions;
}

[System.Serializable]
public class Steps
{
    public List<Step> steps;
}

public class MovimientoPasajeros : MonoBehaviour
{
    public GameObject prefabPasajero;
    public GameObject prefabMetro;
    public GameObject prefabEstacion;

    private Steps allSteps;
    private Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("transformed_sample");
        if (jsonFile != null)
        {
            allSteps = JsonUtility.FromJson<Steps>(jsonFile.text);
            if (allSteps != null && allSteps.steps != null)
            {
                StartCoroutine(UpdatePositions());
            }
            else
            {
                Debug.LogError("Error al deserializar el JSON");
            }
        }
        else
        {
            Debug.LogError("No se encontró el archivo JSON");
        }
    }


    IEnumerator UpdatePositions()
    {
        foreach (Step step in allSteps.steps)
        {
            foreach (PositionData posData in step.positions)
            {
                // Cambiar la segunda coordenada para que sea en el eje Z en lugar de Y
                Vector3 position = new Vector3(posData.position[0], 1/2, posData.position[1]);
                if (!instantiatedObjects.ContainsKey(posData.id))
                {
                    GameObject prefab = GetPrefab(posData.id);
                    instantiatedObjects[posData.id] = Instantiate(prefab, position, Quaternion.identity);
                }
                else
                {
                    instantiatedObjects[posData.id].transform.position = position;
                }
            }
            yield return new WaitForSeconds(1f); // Espera 1 segundo antes de pasar al siguiente paso
        }
    }



    private GameObject GetPrefab(string id)
    {
        if (id.StartsWith("pasajero"))
        {
            return prefabPasajero;
        }
        else if (id.StartsWith("brt"))
        {
            return prefabMetro;
        }
        else if (id.StartsWith("estacion"))
        {
            return prefabEstacion;
        }
        return null;
    }
}

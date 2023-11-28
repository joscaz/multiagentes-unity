using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PositionData
{
    public bool a_bordo;
    public string id;
    public int[] pos;
}

[System.Serializable]
public class Step
{
    public List<PositionData> positions;
}

[System.Serializable]
public class StepsContainer
{
    public int capacity;
    public List<Step> steps;

}

public class MovimientoPasajeros : MonoBehaviour
{
    public GameObject prefabPasajero;
    public GameObject prefabMetro;
    public GameObject prefabEstacion;
    public float speed = 5f;
    public TMPro.TextMeshPro capacityText;
    public CameraFollow cameraFollowScript;

    private Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        StartCoroutine(GetStepData(0)); // Iniciar con el paso 0
    }

    IEnumerator GetStepData(int stepNumber)
    {
        string url = "http://127.0.0.1:5000/getSteps/" + stepNumber.ToString();
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            if (request.responseCode == 500)
            {
                Debug.Log("Fin de los pasos.");
            }
            else
            {
                Debug.LogError("Error al obtener datos: " + request.error);
            }
        }
        else
        {
            StepsContainer stepsContainer = JsonUtility.FromJson<StepsContainer>(request.downloadHandler.text);
            if (stepsContainer.steps != null && stepsContainer.steps.Count > 0)
            {
                capacityText.text = "Capacidad: " + stepsContainer.capacity; // Actualizar TextMeshPro
                Debug.Log("Capacidad en el paso " + stepNumber + ": " + stepsContainer.capacity);
                yield return StartCoroutine(UpdatePositions(stepsContainer.steps[0]));
                StartCoroutine(GetStepData(stepNumber + 1)); // Siguiente paso
            }
            else
            {
                Debug.LogError("No se encontraron posiciones en el paso: " + stepNumber);
            }
        }
    }

    IEnumerator UpdatePositions(Step step)
    {
        foreach (PositionData posData in step.positions)
        {
            Vector3 newPosition = new Vector3(posData.pos[0], 2, posData.pos[1]);

            if (!instantiatedObjects.ContainsKey(posData.id))
            {
                GameObject prefab = GetPrefab(posData.id);
                if (prefab != null)
                {
                    GameObject instantiatedObject = Instantiate(prefab, newPosition, Quaternion.identity);
                    instantiatedObjects.Add(posData.id, instantiatedObject);
                    instantiatedObject.SetActive(true);
                    if (posData.id.StartsWith("Brt") && cameraFollowScript != null)
                    {
                        cameraFollowScript.SetTarget(instantiatedObject.transform);
                    }
                }
                else
                {
                    Debug.LogError("Prefab no encontrado para ID: " + posData.id);
                }
            }
            else
            {
                GameObject obj = instantiatedObjects[posData.id];

                // Activar el objeto antes de moverlo a la nueva posición
                if (posData.id.StartsWith("pasajero"))
                {
                    obj.SetActive(!posData.a_bordo);
                }

                if (obj.activeSelf)
                {
                    obj.transform.position = newPosition;
                }

                StartCoroutine(MoveToPosition(posData.id, newPosition));
            }
        }
        yield return new WaitForSeconds(1 / 4f); // Tiempo entre pasos
    }


    IEnumerator MoveToPosition(string id, Vector3 newPosition)
    {
        if (instantiatedObjects.ContainsKey(id))
        {
            GameObject obj = instantiatedObjects[id];

            while (Vector3.Distance(obj.transform.position, newPosition) > 0.01f)
            {
                // Mover el objeto hacia la nueva posición a una velocidad constante
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, newPosition, speed * Time.deltaTime);

                yield return null; // Espera hasta el siguiente frame antes de continuar
            }
        }
    }


    private GameObject GetPrefab(string id)
    {
        if (id.StartsWith("pasajero") && prefabPasajero != null)
        {
            return prefabPasajero;
        }
        else if (id.StartsWith("Brt") && prefabMetro != null)
        {
            return prefabMetro;
        }
        else if (id.StartsWith("estacion") && prefabEstacion != null)
        {
            return prefabEstacion;
        }
        Debug.LogError("Prefab no encontrado o no asignado para el ID: " + id);
        return null;
    }
}

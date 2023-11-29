using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Define la estructura de datos para cada posición de un objeto en el juego.
[System.Serializable]
public class PositionData
{
    public bool a_bordo;
    public string id;
    public int[] pos;
}

// Define un paso, que consiste en una lista de posiciones.
[System.Serializable]
public class Step
{
    public List<PositionData> positions;
}

// Contiene la capacidad total y una lista de pasos.
[System.Serializable]
public class StepsContainer
{
    public int capacity;
    public List<Step> steps;

}

public class MovimientoPasajeros : MonoBehaviour
{
    // Referencias a los prefabs utilizados en el juego.
    public GameObject prefabPasajero;
    public GameObject prefabMetro;
    public GameObject prefabEstacion;
    public float speed = 5f;
    public TMPro.TextMeshPro capacityText;
    public CameraFollow cameraFollowScript;

    // Diccionario para mantener un registro de todos los objetos instanciados.
    private Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        // Inicia la corutina para obtener datos del primer paso.
        StartCoroutine(GetStepData(0)); // Iniciar con el paso 0
    }

    // Corutina para obtener datos de cada paso de la API.
    IEnumerator GetStepData(int stepNumber)
    {
        string url = "http://127.0.0.1:5000/getSteps/" + stepNumber.ToString();
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        // Maneja la respuesta de la API.
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
            // Deserializa y procesa los datos recibidos.
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

    // Actualiza las posiciones de los objetos en cada paso.
    IEnumerator UpdatePositions(Step step)
    {
        foreach (PositionData posData in step.positions)
        {
            Vector3 newPosition = new Vector3(posData.pos[0], 2, posData.pos[1]);

            if (!instantiatedObjects.ContainsKey(posData.id))
            {
                // Instanciación y configuración inicial de objetos.
                GameObject prefab = GetPrefab(posData.id);
                if (prefab != null)
                {
                    GameObject instantiatedObject = Instantiate(prefab, newPosition, Quaternion.identity);
                    instantiatedObjects.Add(posData.id, instantiatedObject);
                    instantiatedObject.SetActive(true);

                    // Si es un metro, establece como objetivo de la cámara.
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
                // Actualizar la posición y visibilidad de objetos existentes.
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
        yield return new WaitForSeconds(1/32f); // Tiempo entre pasos
    }

    // Mueve los objetos hacia una nueva posición de forma suave.
    IEnumerator MoveToPosition(string id, Vector3 newPosition)
    {
        if (instantiatedObjects.ContainsKey(id))
        {
            GameObject obj = instantiatedObjects[id];
            float journeyLength = Vector3.Distance(obj.transform.position, newPosition);
            float startTime = Time.time;

            // Animación suave hacia la nueva posición.
            while (Time.time < startTime + (journeyLength / speed))
            {
                float distanceCovered = (Time.time - startTime) * speed;
                float fractionOfJourney = distanceCovered / journeyLength;
                obj.transform.position = Vector3.Lerp(obj.transform.position, newPosition, fractionOfJourney);
                yield return null;
            }

            obj.transform.position = newPosition;
        }
    }


    // Devuelve el prefab correspondiente según el ID.
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

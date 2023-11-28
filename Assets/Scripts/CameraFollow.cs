using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // El objeto a seguir
    public Vector3 offset; // Desplazamiento de la cámara respecto al objeto

    void Update()
    {
        if (target != null)
        {
            // Actualizar la posición de la cámara basada en la posición del target
            transform.position = new Vector3(target.position.x, target.position.y + offset.y, target.position.z + offset.z);
            transform.LookAt(target);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

using UnityEngine;

public class SwordGuide : MonoBehaviour
{
    public Transform target;          // El objetivo hacia el que la espada debe apuntar
    public Transform cameraTransform; // La referencia a la cámara
    public Vector3 offset;            // Offset para ajustar la posición relativa a la cámara

    private void LateUpdate()  // Usamos LateUpdate para que el movimiento sea más fluido
    {
        // 1. Mover la espada para que esté siempre frente a la cámara
        if (cameraTransform != null)
        {
            // Posicionamos la espada frente a la cámara con un offset opcional
            transform.position = cameraTransform.position + cameraTransform.forward * offset.z
                                + cameraTransform.right * offset.x
                                + cameraTransform.up * offset.y;

            // Alineamos la espada con la rotación de la cámara
            transform.rotation = cameraTransform.rotation;
        }

        // 2. Apuntar la espada hacia el objetivo con los ejes corregidos (si hay un objetivo)
        if (target != null)
        {
            // Apuntar hacia el objetivo
            Vector3 directionToTarget = target.position - transform.position;

            // Ajuste manual de los ejes invertidos de la espada
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(
                directionToTarget.z,  // El eje z de la espada se convierte en x
                directionToTarget.x,  // El eje x se convierte en y
                directionToTarget.y   // El eje y se convierte en z
            ));

            // Aplica la rotación de forma gradual para que sea más suave
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}

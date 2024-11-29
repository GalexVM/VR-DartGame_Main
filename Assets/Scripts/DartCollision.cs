using UnityEngine;

public class DartCollision : MonoBehaviour
{
    [Header("Configuración del Detector")]
    [Tooltip("Tag del objeto con el que se desea detectar la colisión")]
    public string targetTag = "Target";

    private Rigidbody rb;

    void Start()
    {
        // Asegúrate de que el objeto tenga un Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("El objeto no tiene un componente Rigidbody.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verifica si el objeto con el que colisionó tiene el Tag específico
        if (collision.gameObject.CompareTag(targetTag))
        {
            // Configura el Rigidbody como kinemático
            rb.isKinematic = true;

            // Haz que el dardo sea hijo del objeto objetivo
            transform.SetParent(collision.transform);

            // Opcional: Ajusta la posición o rotación del dardo si es necesario
            Debug.Log($"El dardo se ha pegado al objetivo: {collision.gameObject.name}");
        }
    }
}

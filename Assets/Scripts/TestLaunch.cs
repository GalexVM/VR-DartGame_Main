using UnityEngine;

public class TestLaunch : MonoBehaviour
{
    [Header("Parámetros del Tiro")]
    [Tooltip("Fuerza inicial del lanzamiento")]
    public float launchForce = 10f;

    [Tooltip("Ángulo de lanzamiento en grados")]
    public float launchAngle = 45f;

    [Tooltip("Gravedad personalizada (opcional)")]
    public float customGravity = -9.81f;

    private Rigidbody rb;
    private bool isLaunched = false;

    void Start()
    {
        // Asegúrate de que el objeto tenga un Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("El objeto no tiene un componente Rigidbody.");
        }
    }

    public void Launch()
    {
        if (isLaunched) return;

        // Calcula la dirección inicial del lanzamiento
        float angleInRadians = launchAngle * Mathf.Deg2Rad;
        Vector3 launchDirection = new Vector3(0, Mathf.Sin(angleInRadians), Mathf.Cos(angleInRadians)).normalized;

        // Aplica la fuerza inicial
        rb.useGravity = false; // Desactiva la gravedad para usar la personalizada
        rb.velocity = launchDirection * launchForce;

        // Activa el seguimiento de gravedad personalizada
        isLaunched = true;
        InvokeRepeating(nameof(ApplyCustomGravity), 0f, Time.fixedDeltaTime);
    }

    void ApplyCustomGravity()
    {
        if (isLaunched)
        {
            // Aplica la gravedad personalizada
            rb.velocity += Vector3.up * customGravity * Time.fixedDeltaTime;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Detiene el movimiento y la gravedad al colisionar
        if (isLaunched)
        {
            isLaunched = false;
            CancelInvoke(nameof(ApplyCustomGravity));
            rb.useGravity = true; // Reactiva la gravedad estándar si es necesario
        }
    }
}

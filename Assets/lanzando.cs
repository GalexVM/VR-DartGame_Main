using UnityEngine;

public class OculusGrabTest : MonoBehaviour
{
    private OVRGrabbable grabbable;
    private Rigidbody rb;

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();
        rb = GetComponent<Rigidbody>();

        if (grabbable == null)
        {
            Debug.LogError("OVRGrabbable no está asignado al objeto.");
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody no está asignado al objeto.");
        }
    }

    void Update()
    {
        if (grabbable.isGrabbed)
        {
            Debug.Log("El objeto está siendo agarrado.");
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
    }
}

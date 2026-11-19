// Copies the camera's forward direction onto this object's forward every frame.
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cameraTransform;

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        transform.forward = cameraTransform.forward;
    }
}

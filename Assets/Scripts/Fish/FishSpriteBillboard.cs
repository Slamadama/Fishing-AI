// Keeps a fish sprite square to the screen while FishMotor steers the body underneath it.
using UnityEngine;

public class FishSpriteBillboard : MonoBehaviour
{
    private Transform cameraTransform;

    private void LateUpdate()
    {
        // Camera.main searches the scene by tag, so it is resolved once and then cached.
        if (cameraTransform == null)
        {
            Camera main = Camera.main;

            if (main == null)
                return;

            cameraTransform = main.transform;
        }

        transform.rotation = cameraTransform.rotation;
    }
}

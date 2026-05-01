using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cameraTransform;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 pos = transform.position;
        pos.y = cameraTransform.position.y;
        transform.position = pos;
    }
}
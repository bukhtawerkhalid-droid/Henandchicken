using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private float shakeTimer = 0f;
    private float shakeDuration = 0.15f;
    private float shakeMagnitude = 0.35f;

    void Awake()
    {
        Instance = this;
    }

    public void Shake()
    {
        shakeTimer = shakeDuration;
    }

    public void StopShake()
    {
        shakeTimer = 0f;
    }

    public Vector3 GetShakeOffset()
    {
        if (shakeTimer <= 0f)
            return Vector3.zero;

        shakeTimer -= Time.deltaTime;

        float x = Random.Range(-1f, 1f) * shakeMagnitude;
        float y = Random.Range(-1f, 1f) * shakeMagnitude;

        return new Vector3(x, y, 0f);
    }
}
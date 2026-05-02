using UnityEngine;
using System.Collections;

public class ChickFollow : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 10.0f;
    public float followDistance = 0.8f;
    public float stopThreshold = 0.05f;

    public bool isFollowing = false;

    void Update()
    {
        if (!isFollowing || target == null) return;

        Vector3 dir = target.position - transform.position;
        float distance = dir.magnitude;

        // --- FLIP LOGIC ---
        // Flip based on movement direction relative to target
        if (dir.x > 0.05f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (dir.x < -0.05f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // --- SCREEN WRAP SNAP ---
        float screenWidth = Camera.main != null
            ? Camera.main.orthographicSize * Camera.main.aspect * 2f
            : 20f;

        if (distance > screenWidth * 0.5f)
        {
            Vector3 snapPos = target.position - (dir.normalized * followDistance);
            transform.position = snapPos;
            return;
        }

        // --- SMOOTH FOLLOW ---
        Vector3 desiredPosition = target.position - (dir.normalized * followDistance);

        if (distance > stopThreshold)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                Time.deltaTime * followSpeed
            );
        }

        // --- WRAP LOGIC ---
        if (Camera.main != null)
        {
            float leftBound = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            float rightBound = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;

            Vector3 pos = transform.position;

            if (pos.x < leftBound)
                pos.x = rightBound;
            else if (pos.x > rightBound)
                pos.x = leftBound;

            transform.position = pos;
        }
    }

    public void OnDetached()
    {
        target = null;
        isFollowing = false;
    }

    // -----------------------------
    // PICKUP POP EFFECT
    // -----------------------------
    public void PlayPickupEffect()
    {
        StopAllCoroutines();
        StartCoroutine(PickupPop());
    }

    private IEnumerator PickupPop()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        float t = 0f;
        float duration = 0.1f;

        // Scale UP
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            yield return null;
        }

        t = 0f;

        // Scale DOWN
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
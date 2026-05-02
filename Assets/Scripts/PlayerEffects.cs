using UnityEngine;
using System.Collections;

public class PlayerEffects : MonoBehaviour
{
    private Rigidbody2D rb;
    private Transform visual;

    private bool isGrounded = false;
    private bool wasFalling = false;
    private bool isBouncing = false;

    private float fallTimer = 0f;

    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        visual = transform.Find("Visual");

        if (visual != null)
            originalScale = visual.localScale;
    }

    void Update()
    {
        if (rb == null || visual == null) return;

        // Detect REAL falling (not small movements)
        if (rb.linearVelocity.y < -1.5f)
        {
            wasFalling = true;
            fallTimer += Time.deltaTime;
        }
        else
        {
            fallTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Floor")) return;

        // Only bounce if:
        // - we were falling
        // - we were falling long enough (prevents fake bounce on edge)
        // - not already grounded
        // - not already bouncing
        if (wasFalling && fallTimer > 0.1f && !isGrounded && !isBouncing)
        {
            StartCoroutine(LandingBounce());
        }

        isGrounded = true;
        wasFalling = false;
        fallTimer = 0f;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Floor")) return;

        isGrounded = false;
    }

    IEnumerator LandingBounce()
    {
        isBouncing = true;

        // ALWAYS reset scale (prevents stacking bug)
        visual.localScale = originalScale;

        float duration = 0.06f;
        float t = 0f;

        // Squash
        while (t < duration)
        {
            t += Time.deltaTime;
            float currentSign = Mathf.Sign(visual.localScale.x);
            Vector3 squash = new Vector3(Mathf.Abs(originalScale.x) * 1.15f * currentSign, originalScale.y * 0.85f, originalScale.z);
            visual.localScale = Vector3.Lerp(visual.localScale, squash, t / duration);
            yield return null;
        }

        t = 0f;

        // Stretch
        while (t < duration)
        {
            t += Time.deltaTime;
            float currentSign = Mathf.Sign(visual.localScale.x);
            Vector3 squash = new Vector3(Mathf.Abs(originalScale.x) * 1.15f * currentSign, originalScale.y * 0.85f, originalScale.z);
            Vector3 stretch = new Vector3(Mathf.Abs(originalScale.x) * 0.9f * currentSign, originalScale.y * 1.1f, originalScale.z);
            visual.localScale = Vector3.Lerp(squash, stretch, t / duration);
            yield return null;
        }

        t = 0f;

        // Return to original
        while (t < duration)
        {
            t += Time.deltaTime;
            float currentSign = Mathf.Sign(visual.localScale.x);
            Vector3 stretch = new Vector3(Mathf.Abs(originalScale.x) * 0.9f * currentSign, originalScale.y * 1.1f, originalScale.z);
            Vector3 targetOriginal = new Vector3(Mathf.Abs(originalScale.x) * currentSign, originalScale.y, originalScale.z);
            visual.localScale = Vector3.Lerp(stretch, targetOriginal, t / duration);
            yield return null;
        }

        float finalSign = Mathf.Sign(visual.localScale.x);
        visual.localScale = new Vector3(Mathf.Abs(originalScale.x) * finalSign, originalScale.y, originalScale.z);

        isBouncing = false;
    }
}
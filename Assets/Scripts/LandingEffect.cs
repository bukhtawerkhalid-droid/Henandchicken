using UnityEngine;
using System.Collections;

public class LandingEffect : MonoBehaviour
{
    public float squashAmount = 0.7f;
    public float stretchAmount = 1.2f;
    public float duration = 0.08f;

    private bool isSquashing = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only trigger on floor
        if (!collision.gameObject.CompareTag("Floor")) return;

        // Prevent spam
        if (isSquashing) return;

        StartCoroutine(DoSquash());
    }

    IEnumerator DoSquash()
    {
        isSquashing = true;

        Vector3 originalScale = transform.localScale;
        Vector3 squash = new Vector3(originalScale.x * stretchAmount, originalScale.y * squashAmount, 1f);

        float t = 0;

        // squash
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, squash, t / duration);
            yield return null;
        }

        t = 0;

        // return
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squash, originalScale, t / duration);
            yield return null;
        }

        transform.localScale = originalScale;
        isSquashing = false;
    }
}
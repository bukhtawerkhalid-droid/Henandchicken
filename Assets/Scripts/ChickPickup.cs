using UnityEngine;
using System.Collections;

public class ChickPickup : MonoBehaviour
{
    public ParticleSystem pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                ChickFollow follow = GetComponent<ChickFollow>();

                if (follow != null)
                {
                    follow.isFollowing = true;

                    // NEW: Smooth Pop Effect
                    StartCoroutine(PopEffect());

                    // Particle Effect
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }

                    if (player.chicks.Count == 0)
                    {
                        follow.target = player.transform;
                    }
                    else
                    {
                        follow.target = player.chicks[player.chicks.Count - 1];
                    }

                    player.chicks.Add(this.transform);
                }
            }

            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    // -----------------------------
    // IMPROVED POP EFFECT
    // -----------------------------
    private IEnumerator PopEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        float t = 0f;
        float duration = 0.08f;

        // Scale UP (smooth)
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
            yield return null;
        }

        t = 0f;

        // Scale DOWN (smooth)
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
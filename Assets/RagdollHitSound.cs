using UnityEngine;

public class RagdollHitSound : MonoBehaviour
{
    // [HideInInspector] ukrywa te pola w Inspectorze, bo będą ustawiane przez RagdollSpawner.
    [HideInInspector]
    public AudioClip hitGroundSound;
    [HideInInspector]
    public float soundVolume;

    private bool hasHitGround = false;
    
    // Minimalna prędkość, aby kolizja została uznana za "uderzenie" (unikamy drobnych drgań)
    private const float MIN_IMPACT_VELOCITY = 1.5f; 

    // Ta funkcja jest wywoływana, gdy Collider, do którego jest przypisany ten skrypt,
    // wejdzie w kontakt z innym Colliderem (np. z podłogą).
    private void OnCollisionEnter(Collision collision)
    {
        // 1. Sprawdzenie warunków
        // Jeśli już uderzył lub nie ma dźwięku, przerywamy.
        if (hasHitGround || hitGroundSound == null)
        {
            return;
        }
        
        // 2. Weryfikacja siły uderzenia
        // collision.relativeVelocity.magnitude to siła/prędkość, z jaką nastąpiła kolizja.
        if (collision.relativeVelocity.magnitude > MIN_IMPACT_VELOCITY)
        {
            // 3. Odtwarzanie dźwięku
            // AudioSource.PlayClipAtPoint tworzy tymczasowy AudioSource w miejscu uderzenia.
            // collision.contacts[0].point to punkt w świecie, w którym doszło do kolizji.
            AudioSource.PlayClipAtPoint(hitGroundSound, collision.contacts[0].point, soundVolume);
            
            hasHitGround = true; 
            
            // Opcjonalnie: Zniszcz skrypt po użyciu, aby nie odtwarzał dźwięku ponownie.
            Destroy(this);
        }
    }
}
using UnityEngine;

public class RagdollSpawner : MonoBehaviour
{
    // Upewnij się, że to pole jest przypisane w Edytorze do Prefabu Ragdoll
    public GameObject ragdollPrefab; 
    public Transform spawnPoint;

    // --- POLA DŹWIĘKOWE ---
    public AudioClip spawnSound;
    // DODANE: Pole dla dźwięku uderzenia
    [Tooltip("Klip odtwarzany w chwili uderzenia o ziemię (np. głuchy łomot).")]
    public AudioClip hitGroundSound; 
    
    public float soundVolume = 0.8f;
    private bool hasSpawned = false;
    // ------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            SpawnRagdoll();
            hasSpawned = true;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void SpawnRagdoll()
    {
        if (ragdollPrefab == null || spawnPoint == null) return;

        // 1. Spawnowanie Ragdoll
        GameObject spawnedRagdoll = Instantiate(
            ragdollPrefab, 
            spawnPoint.position, 
            spawnPoint.rotation
        );
        
        // 2. Odtwarzanie dźwięku spawnu
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, spawnPoint.position, soundVolume);
        }

        // 3. Dodanie kontrolera dźwięku upadku (uzupełniony kod)
        // Znajdujemy Rigidbody na kości bioder, która uderzy o ziemię
        Rigidbody hipsRb = spawnedRagdoll.GetComponentInChildren<Rigidbody>();
        
        // Sprawdzamy, czy znaleźliśmy Rigidbody i czy mamy przypisany klip dźwiękowy uderzenia
        if (hipsRb != null && hitGroundSound != null)
        {
             // Dodajemy skrypt RagdollHitSound do obiektu kości Hips
             RagdollHitSound hipHitController = hipsRb.gameObject.AddComponent<RagdollHitSound>();
             
             // Przypisujemy do nowego komponentu klip i głośność
             hipHitController.hitGroundSound = hitGroundSound;
             hipHitController.soundVolume = soundVolume;
        }
    }
}
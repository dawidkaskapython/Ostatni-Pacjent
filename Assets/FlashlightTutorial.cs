using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class FlashlightTutorial : MonoBehaviour
{
    [Header("Referencje Skryptów")]
    public FlashlightEffect flashlightEffect; 

    [Header("Ustawienia Tutoriala")]
    public float tutorialChargeTime = 4f; // Teraz możesz to zmieniać w Inspektorze

    [Header("Warunek Aktywacji")]
    public GameObject flashlightObject; 

    [Header("Referencje Scenki")]
    public VampireEnemy targetEnemy;      
    public Animator enemyAnimator;         
    public GameObject tutorialCanvas; 
    public Transform playerCamera;         

    [Header("Audio (Serce)")]
    public AudioSource heartAudioSource;   
    public AudioClip heartbeatClip;         
    public float maxHeartPitch = 2.5f;     

    [Header("Ustawienia Scenki")]
    public float rotationSpeed = 3f;       
    public float animSlowdownDuration = 2f; 
    public float lookAtHeightOffset = 1.3f;

    private bool tutorialTriggered = false;
    private float originalChargeTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tutorialTriggered)
        {
            if (flashlightObject != null && flashlightObject.activeInHierarchy)
            {
                StartCoroutine(TutorialSequence());
            }
        }
    }

    IEnumerator TutorialSequence()
    {
        tutorialTriggered = true;

        NavMeshAgent agent = targetEnemy.GetComponent<NavMeshAgent>();
        float originalAgentSpeed = 0;
        if (agent != null) originalAgentSpeed = agent.speed;

        if (flashlightEffect != null)
        {
            originalChargeTime = flashlightEffect.chargeTime;
            flashlightEffect.chargeTime = tutorialChargeTime; 
        }

        if (heartAudioSource != null && heartbeatClip != null)
        {
            heartAudioSource.clip = heartbeatClip;
            heartAudioSource.loop = true;
            heartAudioSource.pitch = 1f;
            heartAudioSource.ignoreListenerPause = true;
            heartAudioSource.Play();
        }

        if (flashlightObject != null) flashlightObject.SetActive(false);
        if (targetEnemy != null) targetEnemy.gameObject.SetActive(true);
        
        if (UIController.instance != null)
            UIController.instance.SetGameplayState(false); 

        yield return new WaitForSecondsRealtime(0.5f);
        if (flashlightObject != null) flashlightObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < animSlowdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / animSlowdownDuration;
            
            if (enemyAnimator != null) enemyAnimator.speed = Mathf.Lerp(1f, 0f, progress);
            if (agent != null) agent.speed = Mathf.Lerp(originalAgentSpeed, 0f, progress);

            if (targetEnemy != null)
            {
                Vector3 targetLookPos = targetEnemy.transform.position + Vector3.up * lookAtHeightOffset;
                Vector3 direction = (targetLookPos - playerCamera.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, lookRotation, progress * rotationSpeed * Time.unscaledDeltaTime);
            }
            yield return null;
        }

        if (enemyAnimator != null) enemyAnimator.speed = 0; 
        if (agent != null) agent.enabled = false; 
        
        if (tutorialCanvas != null) tutorialCanvas.SetActive(true);

        while (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
        {
            if (Input.GetKey(KeyCode.F))
            {
                if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
                
                if (heartAudioSource != null)
                {
                    // Serce przyspiesza w tempie dostosowanym do tutorialChargeTime
                    heartAudioSource.pitch = Mathf.MoveTowards(heartAudioSource.pitch, maxHeartPitch, Time.unscaledDeltaTime * (maxHeartPitch / tutorialChargeTime));
                }
            }
            else
            {
                if (heartAudioSource != null)
                {
                    heartAudioSource.pitch = Mathf.MoveTowards(heartAudioSource.pitch, 1f, Time.unscaledDeltaTime * 0.5f);
                }
            }
            yield return null;
        }

        if (heartAudioSource != null) heartAudioSource.Stop();
        
        if (flashlightEffect != null)
            flashlightEffect.chargeTime = originalChargeTime; 

        if (UIController.instance != null)
            UIController.instance.SetGameplayState(true);

        Destroy(gameObject);
    }
}
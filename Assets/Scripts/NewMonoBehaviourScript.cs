using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    public GameObject scareObject;          // Twój model potwora
    public Animator scareAnimator;          // Animator potwora
    public string animationName;            // Nazwa animacji, którą chcesz odpalić
    public float hideDelay = 2f;            // Po ilu sekundach model ma zniknąć

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Pokazujemy potwora
            scareObject.SetActive(true);

            // Start animacji
            scareAnimator.Play(animationName);

            // Ukrycie potwora po czasie
            StartCoroutine(HideAfterDelay());
        }
    }

    private System.Collections.IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        scareObject.SetActive(false);
    }
}
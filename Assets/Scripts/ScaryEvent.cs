using UnityEngine;

public class WalkAcrossEvent : MonoBehaviour
{
    public GameObject npc;            // postać, która ma przejść
    public Transform startPoint;      // punkt pojawienia się
    public Transform endPoint;        // punkt, do którego idzie
    public float speed = 2f;          // prędkość chodzenia
    private bool eventStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (eventStarted) return;

        if (other.CompareTag("Player"))
        {
            eventStarted = true;
            StartCoroutine(EventSequence());
        }
    }

    private System.Collections.IEnumerator EventSequence()
    {
        // 1. pokazujemy NPC
        npc.SetActive(true);

        // 2. ustawiamy go na pozycji startowej
        npc.transform.position = startPoint.position;
        npc.transform.rotation = startPoint.rotation;

        // 3. ruch: idzie od startu do końca
        while (Vector3.Distance(npc.transform.position, endPoint.position) > 0.1f)
        {
            npc.transform.position = Vector3.MoveTowards(
                npc.transform.position,
                endPoint.position,
                speed * Time.deltaTime
            );

            // obrót w stronę celu
            Vector3 dir = endPoint.position - npc.transform.position;
            if (dir != Vector3.zero)
                npc.transform.rotation = Quaternion.LookRotation(dir);

            yield return null;
        }

        // 4. po dojściu możesz go ukryć
        npc.SetActive(false);
    }
}
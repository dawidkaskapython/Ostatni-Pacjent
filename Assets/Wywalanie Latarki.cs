using UnityEngine;
using System.Collections;

public class TriggerHideYellowLight : MonoBehaviour
{
    public GameObject yellowLightChild;

    private bool playerInside = false;

    private void Start()
    {
        if (yellowLightChild != null)
            Debug.Log("Znaleziono dziecko: " + yellowLightChild.name);
        else
            Debug.LogWarning("Nie przypisano YellowLight!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && yellowLightChild != null)
        {
            playerInside = true;
            SetYellowLight(false); // wy³¹cz latarkê
            Debug.Log("Ukryto dziecko (model + œwiat³o): " + yellowLightChild.name);

            // Uruchamiamy korutynê, która w³¹czy latarkê po 3 sekundach
            StartCoroutine(ReenableAfterDelay(3f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && yellowLightChild != null)
        {
            playerInside = false;
            SetYellowLight(true); // w³¹cz latarkê
            Debug.Log("Przywrócono dziecko (model + œwiat³o): " + yellowLightChild.name);
        }
    }

    private IEnumerator ReenableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // SprawdŸ, czy gracz wci¹¿ jest w triggerze
        if (playerInside && yellowLightChild != null)
        {
            SetYellowLight(true); // w³¹cz latarkê po 3 sekundach
            Debug.Log("Automatycznie w³¹czono latarkê po 3 sekundach: " + yellowLightChild.name);
        }
    }

    // Funkcja pomocnicza do w³¹czania/wy³¹czania modelu i œwiat³a
    private void SetYellowLight(bool state)
    {
        foreach (Renderer r in yellowLightChild.GetComponentsInChildren<Renderer>())
            r.enabled = state;

        foreach (Light l in yellowLightChild.GetComponentsInChildren<Light>())
            l.enabled = state;
    }
}

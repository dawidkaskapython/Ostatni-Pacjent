using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Ustawienia Drzwi")]
    public float openAngle = 90f;   // K¹t otwarcia (np. 90 stopni)
    public float openSpeed = 2f;    // Szybkoœæ otwierania
    public bool openReverse = false; // Zaznacz, jeœli drzwi otwieraj¹ siê w drug¹ stronê

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;

    void Start()
    {
        // Zapamiêtaj pocz¹tkow¹ rotacjê jako "zamkniête"
        closedRotation = transform.localRotation;
    }

    // Ta funkcja zostanie wywo³ana przez zamek
    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;

            // Oblicz docelow¹ rotacjê
            float angle = openReverse ? -openAngle : openAngle;
            targetRotation = closedRotation * Quaternion.Euler(0, angle, 0);

            // Rozpocznij animacjê
            StartCoroutine(AnimateDoor());
        }
    }

    IEnumerator AnimateDoor()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            // P³ynna interpolacja (Slerp)
            transform.localRotation = Quaternion.Slerp(closedRotation, targetRotation, t);
            yield return null;
        }
        // Doci¹gniêcie do koñca
        transform.localRotation = targetRotation;
    }
}
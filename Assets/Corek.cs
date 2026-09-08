using UnityEngine;

public class Corek : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Temas eden objenin etiketinde Player yazıyor mu?
        if (other.CompareTag("Player"))
        {
            Debug.Log("Çörek toplandı!");
            Destroy(gameObject); // Çöreği sahneden sil
        }
    }
}
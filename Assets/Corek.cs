using UnityEngine;

public class Corek : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Temas eden oyuncuysa
        if (other.CompareTag("Player"))
        {
            // Sayaca 1 adet ekle
            if (CorekSayaci.Instance != null)
            {
                CorekSayaci.Instance.CorekEkle(1);
            }

            Destroy(gameObject); // Çöreği sahneden kaldır
        }
    }
}
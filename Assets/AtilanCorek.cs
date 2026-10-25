using UnityEngine;

public class AtilanCorek : MonoBehaviour
{
    public float donusHizi = 360f;
    public float omur = 4f;

    private Rigidbody2D rb;

    public void Firlat(Vector2 hizVektoru, float yercekimi)
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Obje üzerinde Rigidbody2D yoksa otomatik ekler (havada asılı kalmayı engeller)
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = yercekimi;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Fırlatma hızını uygula
        #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = hizVektoru;
        #else
            rb.velocity = hizVektoru;
        #endif

        Destroy(gameObject, omur);
    }

    void Update()
    {
        // Havada dönerken animasyon hissi
        transform.Rotate(0f, 0f, -donusHizi * Time.deltaTime);
    }

private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name.ToLower().Contains("karakter") || other.name.ToLower().Contains("player")) 
            return;

        string nesneAdi = other.gameObject.name.ToLower();

        // Kurda veya Dusman'a çarparsa
        if (nesneAdi.Contains("kurt") || nesneAdi.Contains("dusman") || other.CompareTag("Dusman"))
        {
            // Kurdun üzerindeki scripti bul
            KurtKontrolcusu kurt = other.GetComponent<KurtKontrolcusu>();
            if (kurt == null) kurt = other.GetComponentInParent<KurtKontrolcusu>();

            if (kurt != null)
            {
                kurt.Sersemle(); // Kurdu sersemlet
            }

            Destroy(gameObject); // Çöreği yok et
            return;
        }

        // Zemine değerse yok ol
        if (nesneAdi.Contains("zemin") || nesneAdi.Contains("cimen") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
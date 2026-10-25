using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DikenTuzagi : MonoBehaviour
{
    [Header("Sprite Görselleri")]
    public Sprite inikSprite;   // tuzak diken 3
    public Sprite kalkikSprite; // tuzak diken 2

    [Header("Süre Ayarları (Saniye)")]
    public float inikBeklemeSuresi = 2f;
    public float kalkikBeklemeSuresi = 1.2f;

    [Header("Hasar ve Tepki")]
    [Tooltip("Diken kalkıkken değildiğinde gidecek can miktarı")]
    public float hasarMiktari = 25f;
    [Tooltip("Karakter dikene basınca yukarı fırlatma gücü")]
    public float ziplatmaGucu = 4f;
    [Tooltip("Art arda hasar almayı önleyen bekleme payı")]
    public float hasarBeklemeSuresi = 0.8f;

    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool tehlikeliMi = false;
    private float sonHasarZamani = -99f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        if (inikSprite == null || kalkikSprite == null)
        {
            Debug.LogError("DİKKAT: DikenTuzagi üzerindeki resim kutuları boş! Inspector'dan resimleri ata.");
            return;
        }

        StartCoroutine(TuzakDongusu());
    }

    IEnumerator TuzakDongusu()
    {
        while (true)
        {
            // 1. İNİK DURUM (Güvenli)
            sr.sprite = inikSprite;
            tehlikeliMi = false;
            col.enabled = false;
            yield return new WaitForSeconds(inikBeklemeSuresi);

            // 2. KALKIK DURUM (Tehlikeli)
            sr.sprite = kalkikSprite;
            tehlikeliMi = true;
            col.enabled = true;
            yield return new WaitForSeconds(kalkikBeklemeSuresi);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HasarUygula(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HasarUygula(other.gameObject);
    }

    private void HasarUygula(GameObject carpanObje)
    {
        if (!tehlikeliMi) return;

        // Çok hızlı aralıklarla birden fazla kez hasar vurmasını engelle
        if (Time.time - sonHasarZamani < hasarBeklemeSuresi) return;

        bool oyuncuMu = carpanObje.CompareTag("Player") ||
                        carpanObje.name.ToLower().Contains("kiz") ||
                        carpanObje.name.ToLower().Contains("karakter") ||
                        carpanObje.name.ToLower().Contains("oyuncu");

        if (!oyuncuMu) return;

        // Can barını bul
        OtomatikCanBari can = carpanObje.GetComponent<OtomatikCanBari>();
        if (can == null) can = carpanObje.GetComponentInParent<OtomatikCanBari>();
        if (can == null) can = carpanObje.GetComponentInChildren<OtomatikCanBari>();

        if (can != null)
        {
            sonHasarZamani = Time.time;

            // Kızı tuzağın içinden kurtarmak için yukarı doğru sektir
            Vector2 itmeYonu = (carpanObje.transform.position - transform.position).normalized;
            if (itmeYonu.y < 0.5f) itmeYonu.y = 0.9f;
            itmeYonu = itmeYonu.normalized;

            // Can barına hasar gönder
            can.HasarAl(hasarMiktari, itmeYonu * ziplatmaGucu);

            // Sadece can 0 veya altına indiyse sahneyi baştan başlat
            if (can.mevcutCan <= 0)
            {
                StartCoroutine(YenidenBaslatGecikmesi());
            }
        }
    }

    private IEnumerator YenidenBaslatGecikmesi()
    {
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
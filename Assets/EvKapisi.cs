using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvKapisi : MonoBehaviour
{
    [Header("Kapı Animasyon Kareleri (5 Kare)")]
    [Tooltip("Kapalıdan açığa doğru 5 kareyi sırayla buraya sürükle")]
    public Sprite[] kapiKareleri;
    [Tooltip("Kareler arası geçiş hızı")]
    public float kareHizi = 0.09f;

    [Header("Bölüm Geçişi")]
    public string sonrakiBolumAdi = "";
    public float bolumGecisGecikmesi = 0.8f;

    private SpriteRenderer sr;
    private bool kapiAciliyorMu = false;
    private bool kapiTamAcik = false;
    private bool kizIceriGirdi = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        // Başlangıçta 1. kareyi (kapalı halini) göster
        if (kapiKareleri != null && kapiKareleri.Length > 0 && kapiKareleri[0] != null && sr != null)
        {
            sr.sprite = kapiKareleri[0];
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => KontrolEt(other.gameObject);
    private void OnTriggerStay2D(Collider2D other) => KontrolEt(other.gameObject);

    private void KontrolEt(GameObject carpan)
    {
        if (kizIceriGirdi) return;

        // Çarpan nesnede karakter hareket scripti veya can barı var mı?
        OyuncuKontrol oyuncu = carpan.GetComponentInParent<OyuncuKontrol>();
        if (oyuncu == null) oyuncu = carpan.GetComponentInChildren<OyuncuKontrol>();

        OtomatikCanBari canBari = carpan.GetComponentInParent<OtomatikCanBari>();
        if (canBari == null) canBari = carpan.GetComponentInChildren<OtomatikCanBari>();

        bool karakterMi = (oyuncu != null) || (canBari != null) || 
                          carpan.CompareTag("Player") || 
                          carpan.name.ToLower().Contains("karakter") || 
                          carpan.name.ToLower().Contains("kiz");

        if (!karakterMi) return;

        // 1. Kapı henüz açılmadıysa açılma animasyonunu başlat
        if (!kapiAciliyorMu)
        {
            Debug.Log("<color=green>[KAPI]</color> Karakter algılandı, kapı açılıyor...");
            StartCoroutine(KapiAcilisAnimasyonu());
        }

        // 2. Kapı tamamen açıksa kızı içeri al
        if (kapiTamAcik && !kizIceriGirdi)
        {
            GameObject anaKarakter = (oyuncu != null) ? oyuncu.gameObject : carpan;
            StartCoroutine(KarakteriEveAl(anaKarakter));
        }
    }

    private IEnumerator KapiAcilisAnimasyonu()
    {
        kapiAciliyorMu = true;

        if (kapiKareleri == null || kapiKareleri.Length == 0)
        {
            Debug.LogError("[KAPI HATASI] Inspector'daki 'Kapi Kareleri' listesi boş! Resimleri sürüklemedin.");
            kapiTamAcik = true;
            yield break;
        }

        for (int i = 0; i < kapiKareleri.Length; i++)
        {
            if (kapiKareleri[i] != null && sr != null)
            {
                sr.sprite = kapiKareleri[i];
            }
            yield return new WaitForSeconds(kareHizi);
        }

        kapiTamAcik = true;
        Debug.Log("<color=cyan>[KAPI]</color> Kapı sonuna kadar açıldı, giriş hazır.");
    }

    private IEnumerator KarakteriEveAl(GameObject kiz)
    {
        kizIceriGirdi = true;

        // Kızın hareketini ve fiziğini durdur
        OyuncuKontrol hareket = kiz.GetComponent<OyuncuKontrol>();
        if (hareket != null) hareket.enabled = false;

        Rigidbody2D rb = kiz.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Kapıya doğru küçülerek içeri süzülme animasyonu
        SpriteRenderer kizSr = kiz.GetComponentInChildren<SpriteRenderer>();
        Vector3 baslangicScale = kiz.transform.localScale;
        float sayac = 0f;

        while (sayac < 0.4f)
        {
            sayac += Time.deltaTime;
            float oran = 1f - (sayac / 0.4f);
            kiz.transform.localScale = baslangicScale * oran;

            if (kizSr != null)
            {
                Color c = kizSr.color;
                c.a = oran;
                kizSr.color = c;
            }
            yield return null;
        }

        kiz.SetActive(false);

        yield return new WaitForSeconds(bolumGecisGecikmesi);

        if (!string.IsNullOrEmpty(sonrakiBolumAdi))
        {
            SceneManager.LoadScene(sonrakiBolumAdi);
        }
        else
        {
            // İsim yazılmadıysa sahneyi baştan yükle
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
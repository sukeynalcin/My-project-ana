using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RayliTestere : MonoBehaviour
{
    [Header("Animasyon Kareleri (6 Kare)")]
    public Sprite[] testereKareleri;
    public float animasyonHizi = 0.05f;

    [Header("Ray Hareketi")]
    public float kaymaHizi = 3.5f;
    public float hareketMesafesi = 5f;
    public float beklemeSuresi = 0.2f;

    [Header("Hasar ve İtme")]
    public float vurmaHasari = 25f;
    [Tooltip("Çarpınca kızı geriye sıçratma mesafesi")]
    public float geriItelemeMiktari = 0.6f;
    public float hasarBeklemeSuresi = 0.6f;

    private SpriteRenderer sr;
    private Vector3 baslangicNoktasi;
    private Vector3 hedefNokta;
    private bool sagaGidiyor = true;
    private bool bekliyorMu = false;
    private float sonHasarZamani = -99f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        baslangicNoktasi = transform.position;
        hedefNokta = baslangicNoktasi + Vector3.right * hareketMesafesi;

        StartCoroutine(TestereAnimasyonu());
    }

    void Update()
    {
        if (bekliyorMu) return;

        Vector3 suankiHedef = sagaGidiyor ? hedefNokta : baslangicNoktasi;
        transform.position = Vector3.MoveTowards(transform.position, suankiHedef, kaymaHizi * Time.deltaTime);

        if (Vector3.Distance(transform.position, suankiHedef) < 0.02f)
        {
            StartCoroutine(YonDegistirBeklemesi());
        }
    }

    private IEnumerator TestereAnimasyonu()
    {
        int kareIndex = 0;
        while (true)
        {
            if (testereKareleri != null && testereKareleri.Length > 0 && sr != null)
            {
                sr.sprite = testereKareleri[kareIndex];
                kareIndex = (kareIndex + 1) % testereKareleri.Length;
            }
            yield return new WaitForSeconds(animasyonHizi);
        }
    }

    private IEnumerator YonDegistirBeklemesi()
    {
        bekliyorMu = true;
        yield return new WaitForSeconds(beklemeSuresi);
        sagaGidiyor = !sagaGidiyor;
        bekliyorMu = false;
    }

    // Hem katı çarpışmaları (Collision) hem de Trigger temaslarını yakala
    private void OnCollisionEnter2D(Collision2D collision) => TemasKontrol(collision.gameObject);
    private void OnCollisionStay2D(Collision2D collision) => TemasKontrol(collision.gameObject);
    private void OnTriggerEnter2D(Collider2D collision) => TemasKontrol(collision.gameObject);
    private void OnTriggerStay2D(Collider2D collision) => TemasKontrol(collision.gameObject);

    private void TemasKontrol(GameObject carpan)
    {
        if (Time.time - sonHasarZamani < hasarBeklemeSuresi) return;

        // Çarpan nesnede can barı var mı?
        OtomatikCanBari can = carpan.GetComponentInParent<OtomatikCanBari>();
        if (can == null) can = carpan.GetComponentInChildren<OtomatikCanBari>();
        if (can == null) can = carpan.GetComponent<OtomatikCanBari>();

        if (can != null)
        {
            sonHasarZamani = Time.time;

            // Kızın testereye göre hangi tarafta olduğunu bul
            GameObject kizObje = can.gameObject;
            Vector2 itmeYonu = (kizObje.transform.position - transform.position).normalized;
            if (itmeYonu.y < 0.25f) itmeYonu.y = 0.5f;
            itmeYonu = itmeYonu.normalized;

            // KARAKTER KODUNA DOKUNMADAN ANINDA DIŞARI FIRLAT:
            // 1. Kızın pozisyonunu anında testerenin dışına kaydır
            kizObje.transform.position += (Vector3)(itmeYonu * geriItelemeMiktari);

            // 2. Fizik hızını zıt yöne savur
            Rigidbody2D kizRb = kizObje.GetComponent<Rigidbody2D>();
            if (kizRb != null)
            {
                #if UNITY_6000_0_OR_NEWER
                kizRb.linearVelocity = itmeYonu * 7f;
                #else
                kizRb.velocity = itmeYonu * 7f;
                #endif
            }

            // Can düşür
            can.HasarAl(vurmaHasari, itmeYonu * 5f);

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

    private void OnDrawGizmosSelected()
    {
        Vector3 basla = Application.isPlaying ? baslangicNoktasi : transform.position;
        Vector3 bitis = basla + Vector3.right * hareketMesafesi;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(basla, bitis);
        Gizmos.DrawWireSphere(basla, 0.2f);
        Gizmos.DrawWireSphere(bitis, 0.2f);
    }
}
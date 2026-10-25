using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EjderhaTuzagi : MonoBehaviour
{
    [Header("Sprite Kareleri (Sırasıyla 1, 2, 3, 4)")]
    public Sprite[] kareler; // tuzakk1, tuzakk2, tuzakk3, tuzakk4

    [Header("Zamanlama Ayarları")]
    public float beklemeSuresi = 2.0f;
    public float atesKalmaSuresi = 1.3f;
    public float kareHizi = 0.08f;
    public float baslangicGecikmesi = 0f;

    [Header("Hasar Ayarları")]
    public float vurmaHasari = 25f;
    public float hasarBeklemeSuresi = 0.8f;
    public float itmeGucu = 4.5f;

    [Header("Ateş Çarpışma Alanı")]
    [Tooltip("tuzakk1 üzerindeki BoxCollider2D")]
    public Collider2D atesCollider;

    private SpriteRenderer sr;
    private float sonHasarZamani = -99f;
    private bool atesAktifMi = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Objenin üzerindeki BoxCollider2D'yi otomatik yakala
        if (atesCollider == null)
        {
            atesCollider = GetComponent<BoxCollider2D>();
        }

        if (atesCollider != null)
        {
            atesCollider.isTrigger = true;
            atesCollider.enabled = false;
        }
    }

    void Start()
    {
        StartCoroutine(TuzakDongusu());
    }

    IEnumerator TuzakDongusu()
    {
        if (baslangicGecikmesi > 0f)
            yield return new WaitForSeconds(baslangicGecikmesi);

        while (true)
        {
            // 1. SÖNÜK FAZ
            atesAktifMi = false;
            if (atesCollider != null) atesCollider.enabled = false;

            if (kareler != null && kareler.Length > 0 && kareler[0] != null)
                sr.sprite = kareler[0];

            yield return new WaitForSeconds(beklemeSuresi);

            // 2. ATEŞ FAZI
            if (kareler != null && kareler.Length >= 4)
            {
                if (kareler[1] != null) sr.sprite = kareler[1];
                yield return new WaitForSeconds(kareHizi);

                if (kareler[2] != null) sr.sprite = kareler[2];
                atesAktifMi = true;
                if (atesCollider != null) atesCollider.enabled = true;
                yield return new WaitForSeconds(kareHizi);

                if (kareler[3] != null) sr.sprite = kareler[3];

                float sayac = 0f;
                bool toggle = false;
                while (sayac < atesKalmaSuresi)
                {
                    yield return new WaitForSeconds(0.1f);
                    sayac += 0.1f;
                    toggle = !toggle;
                    sr.sprite = toggle ? kareler[3] : kareler[2];
                }

                if (kareler[1] != null) sr.sprite = kareler[1];
                yield return new WaitForSeconds(kareHizi);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) => HasarKontrol(collision.gameObject);
    private void OnTriggerStay2D(Collider2D collision) => HasarKontrol(collision.gameObject);

    private void HasarKontrol(GameObject hedef)
    {
        if (!atesAktifMi) return;
        if (Time.time - sonHasarZamani < hasarBeklemeSuresi) return;

        // Çarpan nesnede (veya üst/alt objesinde) can barı var mı kontrol et
        OtomatikCanBari can = hedef.GetComponentInParent<OtomatikCanBari>();
        if (can == null) can = hedef.GetComponentInChildren<OtomatikCanBari>();
        if (can == null) can = hedef.GetComponent<OtomatikCanBari>();

        if (can != null)
        {
            sonHasarZamani = Time.time;

            // Kızı alevden dışarı ve yukarı fırlat
            Vector2 itme = (hedef.transform.position - transform.position).normalized;
            if (itme.y < 0.4f) itme.y = 0.8f;

            can.HasarAl(vurmaHasari, itme.normalized * itmeGucu);

            // Can bittiyse sahneyi baştan başlat
            if (can.mevcutCan <= 0)
            {
                StartCoroutine(YenidenBaslat());
            }
        }
    }

    private IEnumerator YenidenBaslat()
    {
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
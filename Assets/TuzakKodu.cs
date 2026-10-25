using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TuzakKodu : MonoBehaviour
{
    private Animator anim;
    private bool tetiklendi = false;
    private Transform tasinacakKarakter;
    private Rigidbody2D playerRb;

    [Header("Tuzak Ayarları")]
    [SerializeField] private float tuzakHasari = 25f;       // 100 üzerinden 25 can götürür
    [SerializeField] private float yukselmeHizi = 3f;      // Karakterin yukarı çıkma hızı
    [SerializeField] private float askidaKalmaSuresi = 0.6f; // Ne kadar süre havada tutulsun

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !tetiklendi)
        {
            tetiklendi = true;

            // 1. Karakterin fiziğini duraklat
            playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.simulated = false;
            }

            // 2. Karakteri yukarı taşımak için kaydet ve animasyonu başlat
            tasinacakKarakter = other.transform;
            if (anim != null)
            {
                anim.SetTrigger("Acilsin");
            }

            // 3. OtomatikCanBari scriptine hasar gönder
            OtomatikCanBari can = other.GetComponent<OtomatikCanBari>();
            if (can == null) can = other.GetComponentInParent<OtomatikCanBari>();

            if (can != null)
            {
                // Yukarı doğru darbe yönüyle hasar ver
                can.HasarAl(tuzakHasari, Vector2.up);

                // Can bittiyse sahneyi baştan başlat, bitmediyse kızı yere bırak
                StartCoroutine(TuzakSonrasiIslem(can.mevcutCan <= 0));
            }
            else
            {
                StartCoroutine(TuzakSonrasiIslem(false));
            }
        }
    }

    private void Update()
    {
        if (tetiklendi && tasinacakKarakter != null)
        {
            tasinacakKarakter.Translate(Vector3.up * yukselmeHizi * Time.deltaTime);
        }
    }

    private IEnumerator TuzakSonrasiIslem(bool olduMu)
    {
        yield return new WaitForSeconds(askidaKalmaSuresi);

        // Yukarı taşımayı durdur
        tetiklendi = false;
        tasinacakKarakter = null;

        // Karakterin fiziğini tekrar aç (yere düşebilsin)
        if (playerRb != null)
        {
            playerRb.simulated = true;
        }

        // Eğer canı bittiyse bölümü baştan yükle
        if (olduMu)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
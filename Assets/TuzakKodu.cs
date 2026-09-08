using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TuzakKodu : MonoBehaviour
{
    private Animator anim;
    private bool tetiklendi = false;
    private Transform tasinacakKarakter;

    [Header("Tuzak Ayarları")]
    [SerializeField] private float yukselmeHizi = 3f; // Karakterin yukarı çıkma hızı
    [SerializeField] private float isinlanmaGecikmesi = 0.5f;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !tetiklendi)
        {
            tetiklendi = true;

            // 1. Karakterin yerçekimini ve fiziğini tamamen kapat
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.simulated = false;
            }

            // 2. Takip edilecek karakteri kaydet ve animasyonu başlat
            tasinacakKarakter = other.transform;
            if (anim != null)
            {
                anim.SetTrigger("Acilsin");
            }

            // 3. Bölüm yenileme sürecini başlat
            StartCoroutine(KarakteriIsinla(other.gameObject));
        }
    }

    private void Update()
    {
        // Tetiklenme anından itibaren karakteri pürüzsüzce yukarı kaldırır
        if (tetiklendi && tasinacakKarakter != null)
        {
            tasinacakKarakter.Translate(Vector3.up * yukselmeHizi * Time.deltaTime);
        }
    }

    private IEnumerator KarakteriIsinla(GameObject player)
    {
        yield return new WaitForSeconds(isinlanmaGecikmesi);

        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.simulated = true;
            }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
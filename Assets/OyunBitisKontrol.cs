using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Ekrana yazı yazdırmak için
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class OyunBitisKontrol : MonoBehaviour
{
    [Header("Sahne İsimleri")]
    public string anaMenuSahnesi = "AnaMenu";
    public string kazanmaSahnesi = "SonSahne";

    [Header("Çörek Kazanma Şartı")]
    [Tooltip("Kazanmak için toplanması gereken minimum çörek sayısı")]
    public int gerekenCorekSayisi = 5;
    public int toplananCorek = 0;

    [Header("Arayüz / UI (İsteğe Bağlı)")]
    [Tooltip("Ekranda 'Çörek: 3 / 5' yazması için Canvas'taki Text'i buraya sürükleyebilirsin")]
    public TextMeshProUGUI corekSayaciText;
    [Tooltip("Yetersiz olduğunda ekranda uyarı çıkması için")]
    public TextMeshProUGUI uyariText;

    [Header("Hedef Etiketleri")]
    public string evTag = "Ev";
    public string corekTag = "Corek";

    private bool bittiMi = false;

    void Start()
    {
        ArayuzuGuncelle();
        if (uyariText != null) uyariText.gameObject.SetActive(false);
    }

    void Update()
    {
        // ESC ile Ana Menüye dönme
        bool escBasildi = false;
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            escBasildi = true;
        #endif
        try { if (Input.GetKeyDown(KeyCode.Escape)) escBasildi = true; } catch { }

        if (escBasildi)
        {
            SceneManager.LoadScene(anaMenuSahnesi);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 0. FIRLATILAN MERMİYİ KORU: 
        // Eğer temas eden nesne fırlatılan bir kopyaysa (Clone) veya Mermi ise hiç dokunma, uçsun gitsin:
        if (collision.gameObject.name.Contains("Clone") || collision.CompareTag("Mermi"))
        {
            return;
        }

        // 1. ÇÖREK TOPLAMA KONTROLÜ (Yalnızca yerdeki sabit çörekler)
        if (collision.CompareTag(corekTag) || collision.gameObject.name.ToLower().Contains("örek") || collision.gameObject.name.ToLower().Contains("corek"))
        {
            toplananCorek++;
            ArayuzuGuncelle();
            Debug.Log($"<color=yellow>[Çörek Toplandı] Çantadaki: {toplananCorek} / {gerekenCorekSayisi}</color>");

            // Yerden alınan çöreği sahneden yok et
            Destroy(collision.gameObject);
            return;
        }

        // 2. EVE / KAPIYA VARMA KONTROLÜ
        if (!bittiMi && (collision.CompareTag(evTag) || collision.gameObject.name.ToLower().Contains("ev")))
        {
            // Şart kontrolü: Yeterli çörek toplandı mı?
            if (toplananCorek >= gerekenCorekSayisi)
            {
                bittiMi = true;
                Debug.Log("<color=green>Yeterli çörek toplandı, tebrikler!</color>");
                SceneManager.LoadScene(kazanmaSahnesi);
            }
            else
            {
                // Yetersizse kapı açılmaz, uyarı verir
                int eksik = gerekenCorekSayisi - toplananCorek;
                string mesaj = $"Babaannene boş gidemezsin! {eksik} çörek daha toplamalısın ({toplananCorek}/{gerekenCorekSayisi})";
                Debug.LogWarning(mesaj);

                if (uyariText != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(UyariGoster(mesaj));
                }
            }
        }
    }

    void ArayuzuGuncelle()
    {
        if (corekSayaciText != null)
        {
            corekSayaciText.text = $"Çörek: {toplananCorek} / {gerekenCorekSayisi}";
        }
    }

    IEnumerator UyariGoster(string mesaj)
    {
        uyariText.text = mesaj;
        uyariText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        uyariText.gameObject.SetActive(false);
    }
}
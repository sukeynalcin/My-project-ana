using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class OyuncuKontrol : MonoBehaviour
{
    [Header("Yatay Hareket")]
    public float HareketHizi = 5f;

    [Header("Zıplama Ayarları")]
    public float ZiplamaKuvveti = 7f;       // İlk Space (Normal Zıplama)
    public float YuksekZiplamaKuvveti = 11f; // Havada İkinci Space (Çift Zıplama)

    [Header("Tırmanma Ayarları")]
    public float TirmanmaHizi = 4.5f;

    private bool yerdeMi = true;
    private bool yuksekZiplamaYapildi = false;

    // Tırmanma ve Zemin Değişkenleri
    private bool merdivendeMi = false;
    private bool tirmaniyorMu = false;
    private float varsayilanYercekimi;
    private Collider2D sonDurulanPlatform;
    private bool platformdanIniyorMu = false;

    // Bileşenler
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D karakterCollider;

    // Girdiler
    private float yatayGirdi;
    private float dikeyGirdi;
    private bool ziplamaIstegi = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        karakterCollider = GetComponent<Collider2D>();

        varsayilanYercekimi = rb.gravityScale;
    }

    void Update()
    {
        Keyboard klavye = Keyboard.current;
        if (klavye == null) return;

        // 1. YATAY GİRDİ (A - D / Ok Tuşları)
        yatayGirdi = 0f;
        if (klavye.dKey.isPressed || klavye.rightArrowKey.isPressed) yatayGirdi += 1f;
        if (klavye.aKey.isPressed || klavye.leftArrowKey.isPressed) yatayGirdi -= 1f;

        // 2. DİKEY GİRDİ (W - S / Yukarı - Aşağı Ok Tuşları)
        dikeyGirdi = 0f;
        if (klavye.wKey.isPressed || klavye.upArrowKey.isPressed) dikeyGirdi += 1f;
        if (klavye.sKey.isPressed || klavye.downArrowKey.isPressed) dikeyGirdi -= 1f;

        // 3. MERDİVEN VE AŞAĞI İNME TETİĞİ
        if (merdivendeMi)
        {
            // Yukarı basarsa tırman
            if (dikeyGirdi > 0.1f)
            {
                tirmaniyorMu = true;
            }
            // Aşağı basarsa hem tırman hem de altındaki zemini delip in
            else if (dikeyGirdi < -0.1f)
            {
                tirmaniyorMu = true;

                if (sonDurulanPlatform != null && !platformdanIniyorMu)
                {
                    StartCoroutine(PlatformuGeciciKapat(sonDurulanPlatform));
                }
            }
        }

        // 4. ZIPLAMA TUŞU (Space)
        if (klavye.spaceKey.wasPressedThisFrame && !tirmaniyorMu)
        {
            ziplamaIstegi = true;
        }

        // 5. GÖRSEL DÖNME (Sprite Flip)
        if (yatayGirdi > 0) spriteRenderer.flipX = false;
        else if (yatayGirdi < 0) spriteRenderer.flipX = true;

        AnimasyonlariYonet();
    }

    void FixedUpdate()
    {
        // MERDİVENDEYKEN HAREKET
        if (tirmaniyorMu)
        {
            rb.gravityScale = 0f;
            // Hem X yönünde hem Y yönünde tam kontrollü hareket
            rb.linearVelocity = new Vector2(yatayGirdi * HareketHizi, dikeyGirdi * TirmanmaHizi);
        }
        // NORMAL HAREKET
        else
        {
            rb.gravityScale = varsayilanYercekimi;
            rb.linearVelocity = new Vector2(yatayGirdi * HareketHizi, rb.linearVelocity.y);
        }

        if (ziplamaIstegi)
        {
            ZiplamaUygula();
            ziplamaIstegi = false;
        }
    }

    // Tepedeki taş platformu geçici olarak yok sayıp kızın aşağı kaymasını sağlar
    private IEnumerator PlatformuGeciciKapat(Collider2D platformCol)
    {
        platformdanIniyorMu = true;
        Physics2D.IgnoreCollision(karakterCollider, platformCol, true);

        yield return new WaitForSeconds(0.45f);

        if (karakterCollider != null && platformCol != null)
        {
            Physics2D.IgnoreCollision(karakterCollider, platformCol, false);
        }
        platformdanIniyorMu = false;
    }

    private void ZiplamaUygula()
    {
        if (yerdeMi)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, ZiplamaKuvveti);
            yerdeMi = false;
            yuksekZiplamaYapildi = false;
        }
        else if (!yuksekZiplamaYapildi)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, YuksekZiplamaKuvveti);
            yuksekZiplamaYapildi = true;
            if (anim != null) anim.Play("kiz_jump", -1, 0f);
        }
    }

    private void AnimasyonlariYonet()
    {
        if (anim == null) return;

        if (tirmaniyorMu)
        {
            if (dikeyGirdi != 0)
            {
                anim.speed = 1f;
                anim.Play("kiz_climb");
            }
            else
            {
                anim.speed = 0f; // Merdivende dururken kareyi dondur
            }
        }
        else
        {
            anim.speed = 1f;

            if (yerdeMi)
            {
                if (yatayGirdi != 0) anim.Play("kiz_run");
                else anim.Play("kiz_idle");
            }
        }
    }

    // --- MERDİVEN TETİKLEYİCİLERİ ---
    private void OnTriggerEnter2D(Collider2D other) => MerdivenGiris(other);
    private void OnTriggerStay2D(Collider2D other) => MerdivenGiris(other);

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder") || other.name.ToLower().Contains("merdiven"))
        {
            merdivendeMi = false;
            tirmaniyorMu = false;
            rb.gravityScale = varsayilanYercekimi;
            if (anim != null) anim.speed = 1f;
        }
    }

    private void MerdivenGiris(Collider2D other)
    {
        if (other.CompareTag("Ladder") || other.name.ToLower().Contains("merdiven"))
        {
            merdivendeMi = true;
        }
    }

    // --- ZEMİN ÇARPIŞMALARI ---
    private void OnCollisionEnter2D(Collision2D collision) => ZeminTemas(collision);
    private void OnCollisionStay2D(Collision2D collision) => ZeminTemas(collision);

    private void ZeminTemas(Collision2D collision)
    {
        // Çarptığı katı nesne (merdiven trigger'ı değilse) zemindir
        if (!collision.collider.isTrigger)
        {
            yerdeMi = true;
            yuksekZiplamaYapildi = false;
            sonDurulanPlatform = collision.collider;

            // Merdivenden tamamen inip taban zemine bastığında tırmanmayı bitir
            if (tirmaniyorMu && dikeyGirdi <= 0 && !collision.gameObject.name.ToLower().Contains("tahta"))
            {
                tirmaniyorMu = false;
            }
        }
    }
}
using UnityEngine;

public class OyuncuKontrol : MonoBehaviour
{
    public float HareketHizi = 5f;

    [Header("Zıplama Ayarları")]
    public float ZiplamaKuvveti = 7f;       // Tek Space (Normal Zıplama)
    public float YuksekZiplamaKuvveti = 11f; // Havada İkinci Space (Ekstra Yüksek Zıplama)

    [Header("Tırmanma Ayarları")]
    public float TirmanmaHizi = 5f;

    private bool yuksekZiplamaYapildi = false;
    private bool yerdeMi = true;

    // Tırmanma Değişkenleri
    private bool merdivendeMi = false;
    private bool tirmaniyorMu = false;
    private float varsayilanYercekimi;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Fizik ve Girdi Değişkenleri
    private float yatayGirdi;
    private float dikeyGirdi;
    private bool ziplamaIstegi = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Karakterin varsayılan yerçekimi değerini kaydet
        varsayilanYercekimi = rb.gravityScale;
    }

    void Update()
    {
        // 1. GİRDİLERİ AL (Update içinde tuş kaçırmamak için alinir)
        yatayGirdi = Input.GetAxisRaw("Horizontal");
        dikeyGirdi = Input.GetAxisRaw("Vertical");

        // Merdiven kontrolü
        if (merdivendeMi && Mathf.Abs(dikeyGirdi) > 0f)
        {
            tirmaniyorMu = true;
        }

        // Zıplama Tuşuna Basıldı mı?
        if (Input.GetButtonDown("Jump") && !tirmaniyorMu)
        {
            ziplamaIstegi = true;
        }

        // 2. KIZIN YÖNÜNÜ DÖNDÜRME
        if (yatayGirdi > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (yatayGirdi < 0)
        {
            spriteRenderer.flipX = true;
        }

        // 3. ANİMASYONLARI YÖNET
        AnimasyonlariYonet();
    }

    // FİZİK HAREKETLERİ BURADA YAPILIR (Titremeyi Engelleyen Kısım!)
    void FixedUpdate()
    {
        // Yürüme ve Tırmanma Fiziği
        if (tirmaniyorMu)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(yatayGirdi * HareketHizi, dikeyGirdi * TirmanmaHizi);
        }
        else
        {
            rb.gravityScale = varsayilanYercekimi;
            rb.linearVelocity = new Vector2(yatayGirdi * HareketHizi, rb.linearVelocity.y);
        }

        // Zıplama Fiziği
        if (ziplamaIstegi)
        {
            ZiplamaUygula();
            ziplamaIstegi = false; // İsteği sıfırla
        }
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
            anim.Play("kiz_jump", -1, 0f);
        }
    }

    private void AnimasyonlariYonet()
    {
        if (tirmaniyorMu)
        {
            if (dikeyGirdi != 0)
            {
                anim.speed = 1f;
                anim.Play("kiz_climb");
            }
            else
            {
                anim.speed = 0f;
            }
        }
        else
        {
            anim.speed = 1f;

            if (!yerdeMi)
            {
                // Havadayken
            }
            else if (yatayGirdi != 0)
            {
                anim.Play("kiz_run");
            }
            else
            {
                anim.Play("kiz_idle");
            }
        }
    }

    // Merdivene Giriş / Çıkış Kontrolleri
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            merdivendeMi = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            merdivendeMi = false;
            tirmaniyorMu = false;
            anim.speed = 1f;
        }
    }

    // Yere ve Küplere basma kontrolü
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string carptigiNesne = collision.gameObject.name.ToLower();

        // BURAYA "tahta" EKLENDİ! Artık tahtaya basınca da yerde kabul edilir.
        if (carptigiNesne.Contains("zemin") || carptigiNesne.Contains("kup") || carptigiNesne.Contains("cube") || carptigiNesne.Contains("tahta"))
        {
            yerdeMi = true;
            yuksekZiplamaYapildi = false;
        }
    }
}
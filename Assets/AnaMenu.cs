using UnityEngine;
using UnityEngine.SceneManagement;

public class AnaMenu : MonoBehaviour
{
    [Header("Yüklenecek Oyun Sahnesi")]
    [Tooltip("Başla butonuna basınca açılacak sahnenin tam adı")]
    public string oyunSahnesiAdi = "OYUN";

    // Hierarchy'deki START butonuna bağlayacağın fonksiyon
    public void OyunaBasla()
    {
        if (!string.IsNullOrEmpty(oyunSahnesiAdi))
        {
            SceneManager.LoadScene(oyunSahnesiAdi);
        }
        else
        {
            // Eğer sahne adı yazılmadıysa, Build Settings'teki 1 nolu sıradaki sahneyi açar
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Hierarchy'deki EXIT butonuna bağlayacağın fonksiyon
    public void OyundanCik()
    {
        Debug.Log("<color=red>[ÇIKIŞ]</color> Oyun kapatılıyor...");
        Application.Quit();

        // Unity Editörü içinde test ederken Play modunun durması için
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
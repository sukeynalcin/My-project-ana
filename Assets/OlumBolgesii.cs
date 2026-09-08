
using UnityEngine;
using UnityEngine.SceneManagement;

public class OlumBolgesi : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Temas eden objenin Tag'i "Player" ise
        if (other.CompareTag("Player"))
        {
            // Mevcut sahneyi anında yeniden yükle
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
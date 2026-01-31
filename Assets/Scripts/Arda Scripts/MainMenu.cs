using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Full Screen Image Overlay")]
    [SerializeField] private GameObject fullScreenImagePanel;
    [SerializeField] private Image fullScreenImage;
    [SerializeField] private bool closeOnEscape = true;

    private void Awake()
    {
        if (fullScreenImagePanel != null)
        {
            fullScreenImagePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!closeOnEscape)
        {
            return;
        }

        if (fullScreenImagePanel == null || !fullScreenImagePanel.activeSelf)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideFullScreenImage();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowFullScreenImage()
    {
        if (fullScreenImagePanel == null)
        {
            return;
        }

        fullScreenImagePanel.SetActive(true);
    }

    public void ShowFullScreenImage(Sprite sprite)
    {
        if (fullScreenImagePanel == null)
        {
            return;
        }

        if (fullScreenImage != null)
        {
            fullScreenImage.sprite = sprite;
        }

        fullScreenImagePanel.SetActive(true);
    }

    public void HideFullScreenImage()
    {
        if (fullScreenImagePanel == null)
        {
            return;
        }

        fullScreenImagePanel.SetActive(false);
    }
}

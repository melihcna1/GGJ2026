using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    public static bool IsActive { get; private set; }

    private readonly List<GameObject> _disabledUiRoots = new List<GameObject>();

    private void OnEnable()
    {
        IsActive = true;
        _disabledUiRoots.Clear();

        var myCanvas = GetComponentInParent<Canvas>();
        var myRootCanvas = myCanvas != null ? myCanvas.rootCanvas : null;

        var canvases = Object.FindObjectsOfType<Canvas>(true);
        foreach (var canvas in canvases)
        {
            if (canvas == null)
                continue;

            if (myRootCanvas != null && canvas.rootCanvas == myRootCanvas)
                continue;

            var root = canvas.rootCanvas != null ? canvas.rootCanvas.gameObject : canvas.gameObject;
            if (root.activeSelf)
            {
                _disabledUiRoots.Add(root);
                root.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        IsActive = false;

        for (int i = 0; i < _disabledUiRoots.Count; i++)
        {
            if (_disabledUiRoots[i] != null)
                _disabledUiRoots[i].SetActive(true);
        }

        _disabledUiRoots.Clear();
    }

    public void Retry()
    {
        Time.timeScale = 1f;  
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
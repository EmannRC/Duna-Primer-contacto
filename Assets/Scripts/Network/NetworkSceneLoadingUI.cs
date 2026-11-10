using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NetworkSceneLoadingUI : MonoBehaviour
{
    public static NetworkSceneLoadingUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingFill;
    [SerializeField] private TMP_Text loadingText;

    [Header("Animation")]
    [SerializeField] private float loadingSpeed = 0.35f;
    [SerializeField] private float finishDelay = 0.3f;

    [SerializeField] private string gameplaySceneName = "Gameplay";

    private Coroutine loadingCoroutine;
    private Coroutine finishCoroutine;

    //======================================================================//
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        loadingPanel.SetActive(false);
        loadingFill.fillAmount = 0f;
    }

    //======================================================================//
    public void SubscribeToSceneEvents()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompleted;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadEventCompleted;
    }

    //======================================================================//
    private void OnDestroy()
    {
        if (Instance != this)
            return;

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompleted;
        }

        Instance = null;
    }

    //======================================================================//
    public void ShowLoading(string message)
    {
        if (finishCoroutine != null)
        {
            StopCoroutine(finishCoroutine);
            finishCoroutine = null;
        }

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        loadingPanel.SetActive(true);

        loadingText.text = message;
        loadingFill.fillAmount = 0f;

        loadingCoroutine = StartCoroutine(AnimateLoading());
    }

    //======================================================================//
    private IEnumerator AnimateLoading()
    {
        float progress = 0f;

        while (progress < 0.9f)
        {
            progress = Mathf.MoveTowards(
                progress,
                0.9f,
                loadingSpeed * Time.deltaTime
            );

            loadingFill.fillAmount = progress;

            yield return null;
        }

        loadingCoroutine = null;
    }

    //======================================================================//
    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete &&
            sceneEvent.ClientId == NetworkManager.Singleton.LocalClientId)
        {
            CompleteLoading();
        }
    }

    //======================================================================//
    private void HandleLoadEventCompleted(
    string sceneName,
    LoadSceneMode loadSceneMode,
    List<ulong> clientsCompleted,
    List<ulong> clientsTimedOut)
    {
        if (sceneName != gameplaySceneName)
            return;

        CompleteLoading();

        if (clientsTimedOut.Count > 0)
        {
            Debug.LogWarning(
                $"Jugadores que no terminaron de cargar: {clientsTimedOut.Count}"
            );
        }
    }

    //======================================================================//
    private void CompleteLoading()
    {
        if (!loadingPanel.activeSelf)
            return;

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (finishCoroutine != null)
            StopCoroutine(finishCoroutine);

        finishCoroutine = StartCoroutine(FinishLoading());
    }

    //======================================================================//
    private IEnumerator FinishLoading()
    {
        loadingFill.fillAmount = 1f;

        yield return new WaitForSeconds(finishDelay);

        loadingPanel.SetActive(false);

        finishCoroutine = null;
    }

    public void HideLoading()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (finishCoroutine != null)
        {
            StopCoroutine(finishCoroutine);
            finishCoroutine = null;
        }

        loadingPanel.SetActive(false);
    }
}

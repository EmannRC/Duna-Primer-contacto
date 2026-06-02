using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    
    public static event Action OnPlayerDeath;

    public static GameManager Instance;

    public GameState state;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetState(GameState newState)
    {
        state = newState;

        if (state == GameState.PlayerDead)
            OnPlayerDeath?.Invoke();
    }
}

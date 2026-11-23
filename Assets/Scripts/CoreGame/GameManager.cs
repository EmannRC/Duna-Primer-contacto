using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    Playing,
    Victory,
    Defeat,
    Paused,
    InMenu
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static event Action OnVictory;
    public static event Action OnDefeat;

    public GameState State { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetState(GameState newState)
    {
        State = newState;

        switch (State)
        {
            case GameState.Victory:
                OnVictory?.Invoke();
                break;

            case GameState.Defeat:
                OnDefeat?.Invoke();
                break;
        }
    }
}

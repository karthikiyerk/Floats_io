using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;

public enum GameState { PreGame, Game, PostGame, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState GameState { get; private set; }
    public Action<GameState> OnGameStateChanged;
    public bool TrainingMode;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    void Start()
    {        
        if(TrainingMode)
        {
            StartCoroutine(StartTraining());
        }

        Academy.Instance.AutomaticSteppingEnabled = false;
    }

    void Update()
    {
        if(!TrainingMode && GameState == GameState.Game && BotsManager.Instance.ActiveBots.Count == 0)
        {
            UIManager.Instance.ShowGameOverScreenWin();
            ChangeGameState(GameState.PostGame);
        }
    }

    public void StartGame()
    {
        ChangeGameState(GameState.Game);
    }

    IEnumerator StartTraining()
    {
        //ToDo : Something is malfunctioning if i remove this wait.
        //Need to check/set execution order
        //This is a temperory work around
        yield return new WaitForSeconds(1);
        StartGame();
    }

    public void ChangeGameState(GameState newState)
    {
        GameState = newState;
        OnGameStateChanged?.Invoke(GameState);

        if (newState.Equals(GameState.Game))
        {
            Academy.Instance.AutomaticSteppingEnabled = true;
        }
    }
}

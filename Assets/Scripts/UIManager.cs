using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Params")]
    [SerializeField] float GameOverScreenDelay;

    [Space][Header("Buttons")]
    [SerializeField] Button Play;
    [SerializeField] Button RetryFail;
    [SerializeField] Button Collect;

    [Space]
    [Header("Screens / PopUps")]
    [SerializeField] GameObject StartScreen;
    [SerializeField] GameObject GameOverWinScreen;
    [SerializeField] GameObject GameOverFailScreen;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Play.onClick.AddListener(OnPlayButtonClick);
        RetryFail.onClick.AddListener(OnRetryButtonClick);
        Collect.onClick.AddListener(OnRetryButtonClick);
    }

    #region Button Listener Functions

    private void OnPlayButtonClick()
    {
        StartScreen.SetActive(false);
        GameManager.Instance.StartGame();
    }

    private void OnRetryButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
            
    #endregion

    #region Public Functions

    public void HideStartGameScreen()
    {
        StartScreen.SetActive(false);
    }

    public void ShowGameOverScreenWin()
    {
        //sim to Windows Reg key save for Android eg;score save 
        int CurrentLevel = PlayerPrefs.GetInt("Level", 1);
        CurrentLevel++;
        PlayerPrefs.SetInt("Level", CurrentLevel);

        Debug.Log("***** NEW LEVEL " + CurrentLevel + " *****");
        StartCoroutine(IE_ShowGameOverScreenWin());
    }

    public void ShowGameOverScreenFail()
    {
        StartCoroutine(IE_ShowGameOverScreenFail());
    }

    #endregion

    #region Private Coroutines

    private IEnumerator IE_ShowGameOverScreenWin()
    {
        yield return new WaitForSeconds(GameOverScreenDelay);
        GameOverWinScreen.SetActive(true);
    }

    private IEnumerator IE_ShowGameOverScreenFail()
    {
        yield return new WaitForSeconds(GameOverScreenDelay);
        GameOverFailScreen.SetActive(true);
    }

    #endregion
}

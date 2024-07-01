using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CounterText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private RectTransform DisplayUnit;
    [SerializeField] private Image FillImage;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float SmoothSpeed;

    private Camera mainCamera;
    private Vector3 displayUnitTargetPosition;

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        mainCamera = Camera.main;

        if (GameManager.Instance.GameState == GameState.Game)
        {
            DisplayUnit.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        displayUnitTargetPosition = mainCamera.WorldToScreenPoint(player.position) + offset;
        DisplayUnit.position = Vector3.Lerp(DisplayUnit.position, displayUnitTargetPosition, Time.deltaTime * SmoothSpeed);     
    }

    public void UpdateCount(float count)
    {
        countText.text = count.ToString("F0") + "%";

        FillImage.fillAmount = count / 100f;
        if (count > 50)
            FillImage.color = Color.green;
        else if (count < 25)
            FillImage.color = Color.red;
        else
            FillImage.color = Color.yellow;

        if(count <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void OnGameStateChanged(GameState currentState)
    {
        if(currentState == GameState.Game)
        {
            DisplayUnit.gameObject.SetActive(true);
        }
        else
        {
            DisplayUnit.gameObject.SetActive(false);
        }
    }

    public void DisableCounter()
    {
        gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [Header("PublicReferences")]
    [SerializeField] private Text scoreText =null;
    [SerializeField] private Text movementText = null;
    [SerializeField] private GameObject gameOverUI = null;
    [SerializeField] private Text finalScoreText = null;

    public void ShowGameOver()
    {
        gameOverUI.SetActive(true);
        finalScoreText.text = GameManager.Instance.gameState.Score.ToString();
    }

    public void ScoreChanged()
    {
        scoreText.text = "" + GameManager.Instance.gameState.Score.ToString();
    }

    public void MovementCountChanged()
    {
        movementText.text = GameManager.Instance.gameState.MovementCount.ToString();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(1);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}

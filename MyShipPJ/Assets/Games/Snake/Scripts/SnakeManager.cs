using System.Collections;
using System.Collections.Generic;
using GameData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class SnakeManager : MonoBehaviour
{
    Game game;

    [Header("TopBar")]
    public GameObject topBar;

    [Header("GameTopBar")]
    public GameObject gameTopBar;
    public TextMeshProUGUI scoreTxt, coinTxt;
    public int coin = 0;
    public int score = 0;

    [Header("Player")]
    public GameObject player;
    public Transform playerContainer;

    [Header("Pause & GameOver")]
    public GameObject gameOverPanel, reStartBtn, returnBtn;
    public TextMeshProUGUI overTxt, overScoreTxt, HighScoreTxt,overCoinTxt;
    public bool pause = false;
    
    public GameObject foodPrefab;
    public Rect spawnArea = new Rect(-5, -5, 10, 10);
    public static SnakeManager instance;

    /*[Header("GameTopBar & GameOverPanel")]
    public GameObject gameTopBar, topBar, gameOverPanel, reStartBtn, returnBtn;
    public TextMeshProUGUI scoreTxt, coinTxt, overCoinTxt, overTxt, overScoreTxt, overHighScoreTxt;
    */
   


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GameManager.instance.returnFromGame = true;
        game = DataManager.instance.userData.games[PlayerPrefs.GetInt("CurGame")];
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SpawnFood();
    }

    void Update()
    {

    }


    public void SpawnFood()
    {
        Vector2 spawnPos2D = new Vector2(
            Mathf.Round(UnityEngine.Random.Range(spawnArea.xMin, spawnArea.xMax)),
            Mathf.Round(UnityEngine.Random.Range(spawnArea.yMin, spawnArea.yMax))
        );

        Vector3 spawnPos = new Vector3(spawnPos2D.x, spawnPos2D.y, 0);
        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    // 게임 오버 및 일시정지 처리
    // 게임 오버
    public void Fail(bool isFail)
    {
        pause = true;
        Time.timeScale = 0;

        topBar.SetActive(true);
        // gameTopBar.SetActive(true); // 필요시 켜기

        Debug.Log($"[FAIL] score: {score}, coin: {coin}");

        if (score > game.high_score)
        {
            game.high_score = score;
        }

        overTxt.text = isFail ? "게임오버" : "일시정지";
        reStartBtn.SetActive(isFail);
        returnBtn.SetActive(!isFail);

        overScoreTxt.text = "점수: " + score;
        HighScoreTxt.text = "최고: " + game.high_score;
        overCoinTxt.text = "" + coin;

        gameOverPanel.SetActive(true);
        DataManager.instance.saveData();
    }



    // [1-3] 게임 오버 패널 버튼 액션들
    public void ReStartAction()
     {
         Time.timeScale = 1;
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
     }

     public void ReturnAction()
     {

         gameOverPanel.SetActive(false);
         Time.timeScale = 1;

         topBar.SetActive(false);
     }

     public void ExitAction()
     {
         Time.timeScale = 1;
         SceneManager.LoadScene("MainScene");
     }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateTopBar();
    }

    public void AddCoin(int amount)
    {
        coin += amount;
        UpdateTopBar();
    }

    void UpdateTopBar()
    {
        if (scoreTxt != null)
            scoreTxt.text = "점수: " + score;

        if (coinTxt != null)
            coinTxt.text = "" + coin;
    }





    // 게임 오버
    /*public void GameOver(bool isOver)
    {
        DataManager.instance.saveData();
        Time.timeScale = 0;

        overTxt.text = isOver ? "게임오버" : "일시정지";

        topBar.SetActive(true);

        returnBtn.SetActive(!isOver);
        reStartBtn.SetActive(isOver);
        overCoinTxt.text = "+ " + coins;
        overScoreTxt.text = "점수 : " + (int)score;
        if (score > game.high_score)
            game.high_score = (int)score;

        overHighScoreTxt.text = "최고 : " + game.high_score;
        gameOverPanel.SetActive(true);

    }

    // [1-3] 게임오버 패널 버튼 액션들
    // SnakeManager.cs
    public void ReStartAction()
    {
        // 게임을 리셋할 때 필요한 상태 초기화
        Time.timeScale = 1;

        // 게임 상태 초기화
        score = 0;
        coins = 0;


        // 새로운 씬 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Start();

    }


    public void ReturnAction()
    {

        gameOverPanel.SetActive(false);
        Time.timeScale = 1;

        topBar.SetActive(false);
    }

    public void ExitAction()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainScene");
    }*/
}
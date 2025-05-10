using System;
using System.Collections;
using System.Collections.Generic;
using GameData;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CMManager : MonoBehaviour
{
    public static CMManager instance;

    Game game;

    int coin = 0;
    public int score = 0;
    public GameObject player;
    public Transform playerContainer;
    public GameObject coinPrefab;


    [Header("GameTopBar & GameOverPanel")]
    public GameObject gameTopBar, topBar, gameOverPanel, reStartBtn, returnBtn;
    public TextMeshProUGUI scoreTxt, coinTxt, overCoinTxt, overTxt, overScoreTxt, overHighScoreTxt;


    void Awake()
    {
        instance = this;
    }
    public void AddCoin(int amount)
    {
        coin += amount;
        coinTxt.text = coin.ToString();
    }

    void Start()
    {
        Time.timeScale = 1; // 혹시 Time.timeScale이 0으로 남아 있을 경우 대비
        coin = 0;
        score = 0;

        coinTxt.text = "0";
        scoreTxt.text = "0";

        GameManager.instance.returnFromGame = true;
        game = DataManager.instance.userData.games[PlayerPrefs.GetInt("CurGame")];
        // 캐릭터 생성
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/" + GameManager.instance.curCharacter.name);
        player = Instantiate(playerPrefab, new Vector3(-2.3f, 3.3f, 0), quaternion.identity);
        player.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        player.transform.SetParent(playerContainer, false);
        player.SetActive(true);

       
    }



    // 게임 오버
    public void GameOver(bool isOver)
    {
        DataManager.instance.saveData();
        Time.timeScale = 0;

        overTxt.text = isOver ? "게임오버" : "일시정지";

        topBar.SetActive(true);

        returnBtn.SetActive(!isOver);
        reStartBtn.SetActive(isOver);
        overCoinTxt.text = "+ " + coin;
        overScoreTxt.text = "점수 : " + (int)score;
        if (score > game.high_score)
            game.high_score = (int)score;

        overHighScoreTxt.text = "최고 : " + game.high_score;
        gameOverPanel.SetActive(true);

    }

    // [1-3] 게임오버 패널 버튼 액션들
    public void ReStartAction()
    {

        Time.timeScale = 1;
        gameOverPanel.SetActive(false);
        topBar.SetActive(false);
        coin = 0;
        score = 0;
        coinTxt.text = "0";
        scoreTxt.text = "0";

        GameManagerTwo.instance.timeLimit = 60f;  //////////재시작시 시간 설정  
        GameManagerTwo.instance.Num = 2;
        GameManagerTwo.instance.consecutiveMatches = 0;
        GameManagerTwo.instance.onepunch();

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


}

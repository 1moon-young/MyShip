using System.Collections;
using System.Collections.Generic;
using GameData;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Asteroid_Manager : MonoBehaviour
{
    public static Asteroid_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    Game game;
    public float score = 0;
    public int coin = 0;

    public float speed = 6f;
    public int speedUpInterval = 5;

    public Transform playerContainer;

    public GameObject asteroidsPrefab;
    public GameObject coinPrefab;
    public Camera mainCamera; // ȭ�� ��� ����� ���� ī�޶� ����

    [Header("GameTopBar & GameOverPanel")]
    public GameObject gameTopBar, topBar, gameOverPanel, reStartBtn, returnBtn;
    public TextMeshProUGUI scoreTxt, coinTxt, overCoinTxt, overTxt, overScoreTxt, overHighScoreTxt;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        GameManager.instance.returnFromGame = true;
        game = DataManager.instance.userData.games[PlayerPrefs.GetInt("CurGame")];

        // �÷��̾� ����
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/" + GameManager.instance.curCharacter.name);
        GameObject player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Unity.Mathematics.quaternion.identity);
        player.transform.SetParent(playerContainer, false);

        // ���� ī�޶� ����
        mainCamera = Camera.main;

        Time.timeScale = 1;

        StartCoroutine("CreateAsteroidsAndCoinRoutine");
        StartCoroutine("SpeedUpRoutine");
        StartCoroutine("ScoreUpRoutine");
    }

    // ȭ�� ��� ���� ���� ��ġ ���� (4�� �� �ϳ�)
    public Vector3 GetRandomSpawnPosition()
    {
        float screenWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float screenHeight = mainCamera.orthographicSize;

        // 0: ��, 1: ��, 2: ��, 3: ��
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // ���
                return new Vector3(Random.Range(-screenWidth, screenWidth), screenHeight + 1, 0);
            case 1: // �ϴ�
                return new Vector3(Random.Range(-screenWidth, screenWidth), -screenHeight - 1, 0);
            case 2: // ����
                return new Vector3(-screenWidth - 1, Random.Range(-screenHeight, screenHeight), 0);
            case 3: // ����
                return new Vector3(screenWidth + 1, Random.Range(-screenHeight, screenHeight), 0);
            default:
                return Vector3.zero;
        }
    }

    // ���༺ & ���� ���� ��ƾ
    IEnumerator CreateAsteroidsAndCoinRoutine()
    {
        while (true)
        {
            // speed�� ���� ���༺�� ���� ���� ����
            int meteorCount = speed > 18 ? 13 : (speed > 12 ? 9 : 5);
            int coinCount = speed > 18 ? 3 : (speed > 12 ? 2 : 1);

            // ���༺ ����
            for (int i = 0; i < meteorCount; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject asteroid = Instantiate(asteroidsPrefab, spawnPos, Quaternion.identity);

                // ���༺�� �̵� ���� ���� (���� ��ġ�� �ݴ� ����)
                Asteroid_Obstacle obstacle = asteroid.GetComponent<Asteroid_Obstacle>();
                if (obstacle != null)
                {
                    obstacle.moveDirection = (Vector3.zero - spawnPos).normalized;
                }
            }

            // ���� ����
            for (int i = 0; i < coinCount; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject coinObj = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

                // ������ �̵� ���� ���� (���� ��ġ�� �ݴ� ����)
                Asteroid_Obstacle coin = coinObj.GetComponent<Asteroid_Obstacle>();
                if (coin != null)
                {
                    coin.moveDirection = (Vector3.zero - spawnPos).normalized;
                }

                // speed�� ���� ���� ũ�� ����
                if (speed > 18)
                    coinObj.transform.localScale = coinObj.transform.localScale * 1.5f;
                else if (speed > 12)
                    coinObj.transform.localScale = coinObj.transform.localScale * 1.3f;
            }

            yield return new WaitForSeconds(3f);
        }
    }

    // ���ǵ� ���� �ڷ�ƾ
    IEnumerator SpeedUpRoutine()
    {
        while (speed < 22f)
        {
            yield return new WaitForSeconds(speedUpInterval);
            speed += 0.5f;
        }
    }

    // ���� ���� �ڷ�ƾ
    IEnumerator ScoreUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            score += speed / 4;
            scoreTxt.text = "����: " + (int)score;
        }
    }

    // ���� ����
    public void GetCoin()
    {
        int coinValue;

        if (speed > 18)
            coinValue = 5;
        else if (speed > 12)
            coinValue = 3;
        else
            coinValue = 2;

        DataManager.instance.userData.coin += coinValue;
        coin += coinValue;
        coinTxt.text = coin.ToString();
    }

    // ���� ����
    public void GameOver(bool isOver)
    {
        DataManager.instance.saveData();
        Time.timeScale = 0;

        StopCoroutine("CreateAsteroidsAndCoinRoutine");
        StopCoroutine("SpeedUpRoutine");
        StopCoroutine("ScoreUpRoutine");

        overTxt.text = isOver ? "���ӿ���" : "�Ͻ�����";

        topBar.SetActive(true);

        returnBtn.SetActive(!isOver);
        reStartBtn.SetActive(isOver);
        overCoinTxt.text = "+ " + coin;
        overScoreTxt.text = "���� : " + (int)score;
        if (score > game.high_score)
            game.high_score = (int)score;

        overHighScoreTxt.text = "�ְ� : " + game.high_score;
        gameOverPanel.SetActive(true);
    }

    // [1-3] ���� ���� �г� ��ư �׼ǵ�
    public void ReStartAction()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnAction()
    {
        StartCoroutine("CreateAsteroidsAndCoinRoutine");
        StartCoroutine("SpeedUpRoutine");
        StartCoroutine("ScoreUpRoutine");

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
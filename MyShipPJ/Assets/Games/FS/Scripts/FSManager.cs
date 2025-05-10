using System.Collections;
using System.Collections.Generic;
using GameData;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FSManager : MonoBehaviour
{

    public static FSManager instance;

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
    // public Animator playerAnim;

    public GameObject[] pillarPrefabs;
    public Vector3 pillarSize;
    public Transform[] pillarSpawns; // 0 - top, 1 - bottom
    public Vector3[] pillarSpawnsPos = new Vector3[2];
    public float pillarSpawnRate = 2f;
    public float moveY = 1.77f;
    public int prePattern;
    public int repeat = 0;

    public GameObject coinPrefab;

    [Header("GameTopBar & GameOverPanel")]
    public GameObject gameTopBar, topBar, gameOverPanel, reStartBtn, returnBtn;
    public TextMeshProUGUI scoreTxt, coinTxt, overCoinTxt, overTxt, overScoreTxt, overHighScoreTxt;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        GameManager.instance.returnFromGame = true;
        game = DataManager.instance.userData.games[PlayerPrefs.GetInt("CurGame")];
        // 캐릭터 생성
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/" + GameManager.instance.curCharacter.name);
        GameObject player = Instantiate(playerPrefab, new Vector3(0, 0, 0), Unity.Mathematics.quaternion.identity);
        player.transform.SetParent(playerContainer, false);
        // playerAnim = player.GetComponent<Animator>();

        pillarSpawnsPos[0] = pillarSpawns[0].position;
        pillarSpawnsPos[1] = pillarSpawns[1].position;
        pillarSize = pillarPrefabs[0].GetComponent<SpriteRenderer>().bounds.size;

        StartCoroutine("CreatePillarRoutine");
        StartCoroutine("SpeedUpRoutine");
        StartCoroutine("ScoreUpRoutine");
    }

    // 기둥 & 코인 생성 코루틴
    IEnumerator CreatePillarRoutine()
    {
        while (true)
        {
            int where = Random.Range(0, 3);  // 0: top, 1: bottom, 2: both
            float posY = Random.Range(0, moveY);

            // 같은 패턴 3번 이상 반복 되는거 방지
            if (prePattern == where)
            {
                repeat++;
                if (repeat > 2)
                    where = (where + 1) % 3;
            }
            else
            {
                prePattern = where;
                repeat = 0;
            }

            // 기둥 & 코인 생성 함수 호출
            if (where == 2)  // top & bottom 둘 다 생성
            {
                CreatePillar(0, posY);
                CreatePillar(1, posY);

                CreateCoin(0.5f, posY / 2);
            }
            else  // top or bottom 하나만 생성
            {
                CreatePillar(where, posY);
                CreateCoin(where, posY);
            }

            pillarSpawnRate = Random.Range(0.8f, 1.8f) / (0.5f + 0.05f * speed);

            yield return new WaitForSeconds(pillarSpawnRate);
        }
    }

    // 기둥 생성
    void CreatePillar(int where, float posY)
    {
        GameObject pillar;
        pillar = Instantiate(pillarPrefabs[Random.Range(0, pillarPrefabs.Length)], pillarSpawnsPos[where], Quaternion.identity);
        pillar.transform.position -= Vector3.up * posY;

        if (where == 1)
            pillar.transform.localScale += new Vector3(0, -2, 0);
    }

    // 코인 생성
    void CreateCoin(float where, float posY)
    {
        Vector3 pos;
        Vector3 adjustPos = Vector3.up * pillarSize.y / Random.Range(1f, 1.5f);

        if (where == 0) // 위쪽
            pos = pillarSpawnsPos[0] - (Vector3.up * posY + adjustPos);
        else if (where == 1) // 아래쪽
            pos = pillarSpawnsPos[1] + (Vector3.up * -posY + adjustPos);
        else  // 가운데 (기둥 사이 공간)
            pos = (pillarSpawnsPos[0] + pillarSpawnsPos[1]) / 2 - Vector3.up * posY * 2;

        GameObject coin = Instantiate(coinPrefab, pos + Vector3.left * Random.Range(-1f, 1f), Quaternion.identity);

        if (speed > 18)
            coin.transform.localScale = coin.transform.localScale * 1.5f;
        else if (speed > 12)
            coin.transform.localScale = coin.transform.localScale * 1.3f;

    }

    // 스피드 증가 코루틴
    IEnumerator SpeedUpRoutine()
    {
        while (speed < 22f)
        {
            yield return new WaitForSeconds(speedUpInterval);
            speed += 0.5f;
        }

    }

    // 점수 증가 코루틴
    IEnumerator ScoreUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            score += speed / 4;
            scoreTxt.text = "점수: " + (int)score;
        }
    }

    // 코인 증가
    public void GetCoin()
    {
        int coinValue;

        if (speed > 18)
            coinValue = 5;
        if (speed > 12)
            coinValue = 3;
        else
            coinValue = 2;

        DataManager.instance.userData.coin += coinValue;
        coin += coinValue;
        coinTxt.text = coin.ToString();
    }

    // 게임 오버
    public void GameOver(bool isOver)
    {
        DataManager.instance.saveData();
        Time.timeScale = 0;

        StopCoroutine("CreatePillarRoutine");
        StopCoroutine("SpeedUpRoutine");
        StopCoroutine("ScoreUpRoutine");

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

    // [1-3] 게임 오버 패널 버튼 액션들
    public void ReStartAction()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnAction()
    {
        StartCoroutine("CreatePillarRoutine");
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

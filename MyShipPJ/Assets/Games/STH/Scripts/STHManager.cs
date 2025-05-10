using System;
using System.Collections;
using System.Collections.Generic;
using GameData;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class STHManager : MonoBehaviour
{
    Game game;

    [Header("TopBar")]
    public GameObject topBar;

    [Header("GameTopBar")]
    public GameObject gameTopBar;
    public TextMeshProUGUI scoreTxt, coinTxt, overCoinTxt;
    int coin = 0;
    public int score = 0;

    const int UP = 0;
    const int SHIFT = 1;
    const int LEFT = -1;
    const int RIGHT = 1;

    [Header("Player")]
    public GameObject player;
    public Transform playerContainer;
    public Animator playerAnim;
    int playerDir = LEFT;  // -1 - 왼쪽, 1 - 오른쪽

    [Header("Tile")]
    public List<int> tiles = new List<int>(40);  // 0: 오르기 , 1:시프트
    public List<bool> isCoin = new List<bool>(40); // true - 코인 있음
    public List<GameObject> coinTile = new List<GameObject>();
    public GameObject tilePrefab;
    public GameObject coinTilePrefab;
    public Transform firstTile;
    Vector3 tileSize;
    int tileDir = LEFT;
    int maxStraight = 0;
    readonly int tileCount = 40;
    Vector3 tilePos;

    [Header("TileContainer")]
    public Transform tilesContainer1;
    public Transform tilesContainer2;
    public GameObject bottomContainer;
    int whichTiles = 2;

    [Header("Background")]
    public Transform bg1, bg2;
    Vector3 bgSize;
    // readonly float x = 6.25f;
    readonly float y = -13f;
    Color blueColor = new Color(163 / 255f, 218 / 255f, 1f);
    Color sunsetColor = new Color(1.0f, 195 / 255f, 87 / 255f);
    Color nightColor = new Color(35 / 255f, 37 / 255f, 47 / 255f);
    Color cloudGrey = new Color(169 / 255f, 169 / 255f, 169 / 255f);

    [Header("TimeLimit")]
    public Slider HPSlider;
    public Image HPSliderFill;
    public float stayTime = 0;
    bool start = false;
    readonly float[] timeLimit = { 2.2f, 2f, 1.8f, 1.6f, 1.4f, 1.2f, 1f, 0.8f, 0.8f, 0.8f, 0.8f };    // 10초 경과할 때마다 제한 올리기기
    float limit = 2.2f;
    int timeLimitIndex = 0;

    [Header("Pause & GameOver")]
    public GameObject gameOverPanel, reStartBtn, returnBtn;
    public TextMeshProUGUI overTxt, overScoreTxt, HighScoreTxt;
    bool pause = false;



    void Start()
    {
        GameManager.instance.returnFromGame = true;
        game = DataManager.instance.userData.games[PlayerPrefs.GetInt("CurGame")];
        // 캐릭터 생성
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/" + GameManager.instance.curCharacter.name);
        player = Instantiate(playerPrefab, new Vector3(0, -1, 0), quaternion.identity);
        player.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        player.transform.SetParent(playerContainer, false);
        playerAnim = player.GetComponent<Animator>();
        player.SetActive(true);

        // 타일 큐 생성
        maxStraight = 0;

        tiles.Add(UP);
        for (int i = 0; i < tileCount - 1; i++)
        {
            int x = UnityEngine.Random.Range(0, 5);
            if (x == 4 || maxStraight == 5)
            {
                tiles.Add(SHIFT);
                maxStraight = 0;
            }
            else
            {
                tiles.Add(UP);
                maxStraight++;
            }
        }

        tileSize = tilePrefab.GetComponent<Renderer>().bounds.size;
        bgSize = bg1.GetComponent<Renderer>().bounds.size;

        InitTile();
        // StartCoroutine("UpTimeLimit");

        Time.timeScale = 0;
    }

    void Update()
    {
        if (pause) return;

        stayTime += Time.deltaTime;
        HPSlider.value -= Time.deltaTime;
        SetSliderColor();

        if (stayTime > limit)
        {
            Fail(true);
        }

        // 키보드 입력으로도 action()
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Action(0);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Action(1);
        }


        // 배경 색 변환
        Color targetColor = GetTargetColor(score);
        Color targetColorCloud = GetTargetColorCloud(score);

        bg1.GetComponent<SpriteRenderer>().color = Color.Lerp(bg1.GetComponent<SpriteRenderer>().color, targetColor, Time.deltaTime * 2);
        bg2.GetComponent<SpriteRenderer>().color = Color.Lerp(bg2.GetComponent<SpriteRenderer>().color, targetColor, Time.deltaTime * 2);

        bg1.GetChild(0).GetComponent<SpriteRenderer>().color = Color.Lerp(bg1.GetChild(0).GetComponent<SpriteRenderer>().color, targetColorCloud, Time.deltaTime * 2);
        bg2.GetChild(0).GetComponent<SpriteRenderer>().color = Color.Lerp(bg2.GetChild(0).GetComponent<SpriteRenderer>().color, targetColorCloud, Time.deltaTime * 2);

    }


    // 제한시간 감소 코루틴
    IEnumerator UpTimeLimit()
    {
        while (timeLimitIndex < timeLimit.Length)
        {
            yield return new WaitForSeconds(10);
            HPSlider.maxValue = timeLimit[timeLimitIndex];
            limit = timeLimit[timeLimitIndex];

            timeLimitIndex++;
        }
    }



    // 배경 색 리턴
    Color GetTargetColor(int score)
    {
        if (score < 50)
            return blueColor;

        // 50 ~ 200: 파란색 -> 노을색
        if (score < 200)
            return Color.Lerp(blueColor, sunsetColor, (score - 50) / 150f);

        if (score < 250)
            return sunsetColor;

        // 250 ~ 300: 노을색 -> 밤색
        if (score < 300)
            return Color.Lerp(sunsetColor, nightColor, (score - 250) / 50f);

        if (score < 250)
            return nightColor;

        // 350 ~ 500: 밤색 -> 파란색
        if (score < 500)
            return Color.Lerp(nightColor, blueColor, (score - 350) / 150f);

        return GetTargetColor(score % 500);
    }

    Color GetTargetColorCloud(int score)
    {
        if (score < 200)
            return Color.white;

        // 200 ~ 300: 흰색 -> 회색
        if (score < 300)
            return Color.Lerp(Color.white, cloudGrey, (score - 200) / 300f);
        // 300 ~ 500 : 회색 -> 흰색
        if (score < 500)
            return Color.Lerp(cloudGrey, Color.white, (score - 300) / 200f);

        return GetTargetColorCloud(score % 500);
    }

    void SetSliderColor()
    {
        float red = Mathf.Round(255 * (1 - HPSlider.value / HPSlider.maxValue)); // 값이 0에 가까울수록 빨강
        float green = Mathf.Round(255 * HPSlider.value / HPSlider.maxValue);    // 값이 1에 가까울수록 초록

        HPSliderFill.color = new Color(red / 255, green / 255, 0);
    }



    // 처음 두 타일 초기화
    void InitTile()
    {
        tilePos = firstTile.position;

        // tileContainer 1 채우기
        for (int i = 0; i < tileCount / 2; i++)
        {
            if (tiles[i] == 1)  // 시프트면 방향 바꾸기
                tileDir *= -1;

            tilePos += new Vector3(tileDir, 0, 0) * tileSize.x;
            tilePos += Vector3.up * tileSize.y;

            GameObject tile;

            bool createCoin = UnityEngine.Random.Range(0, 3) == 0;

            if (createCoin)
            {
                tile = Instantiate(coinTilePrefab, transform.position, quaternion.identity);
                coinTile.Add(tile);
            }

            else
                tile = Instantiate(tilePrefab, transform.position, quaternion.identity);

            isCoin.Add(createCoin);

            tile.transform.SetParent(tilesContainer1, false);
            tile.transform.position = tilePos;
            tile.SetActive(true);
        }

        // tileContainer 2 채우기
        tilesContainer2.transform.position = new Vector3(0, tilePos.y, 0);
        tilePos = new Vector3(tilePos.x, 0, 0);

        for (int i = tileCount / 2; i < tileCount; i++)
        {
            if (tiles[i] == 1)  // 시프트면 방향 바꾸기
                tileDir *= -1;

            tilePos += new Vector3(tileDir, 0, 0) * tileSize.x;
            tilePos += Vector3.up * tileSize.y;

            GameObject tile;

            bool createCoin = UnityEngine.Random.Range(0, 3) == 0;
            if (createCoin)
            {
                tile = Instantiate(coinTilePrefab, transform.position, quaternion.identity);
                coinTile.Add(tile);
            }
            else
                tile = Instantiate(tilePrefab, transform.position, quaternion.identity);

            isCoin.Add(createCoin);

            tile.transform.position = tilePos;
            tile.transform.SetParent(tilesContainer2, false);
            tile.SetActive(true);
        }
    }

    // 타일 재생성 후 위로 올리기 
    void CreateTileContainer(Transform newContainer, Transform oldContainer)
    {
        // 아래로 내려간 타일 복사본 만들어서 유지
        if (bottomContainer != null)
            Destroy(bottomContainer);

        bottomContainer = Instantiate(newContainer.gameObject, newContainer.position, quaternion.identity);
        bottomContainer.SetActive(true);

        for (int i = 0; i < newContainer.childCount; i++)
        {
            Destroy(newContainer.GetChild(i).gameObject);
        }

        // 아래 타일 위로 올리기( newContainer )
        tilePos = oldContainer.GetChild(oldContainer.childCount - 1).position;
        newContainer.position = new Vector3(0, tilePos.y, 0);
        tilePos = new Vector3(tilePos.x, 0, 0);

        for (int i = tileCount / 2; i < tileCount; i++)
        {
            if (tiles[i] == 1)  // 시프트면 방향 바꾸기
                tileDir *= -1;

            tilePos += new Vector3(tileDir, 0, 0) * tileSize.x;
            tilePos += Vector3.up * tileSize.y;

            GameObject tile;

            bool createCoin = UnityEngine.Random.Range(0, 3) == 0;
            isCoin.Add(createCoin);

            if (createCoin)
            {
                tile = Instantiate(coinTilePrefab, transform.position, quaternion.identity);
                coinTile.Add(tile);
            }

            else
            {
                tile = Instantiate(tilePrefab, transform.position, quaternion.identity);
            }

            tile.transform.position = tilePos;
            tile.transform.SetParent(newContainer, false);
            tile.SetActive(true);

        }
    }

    // 오를 때마다 배경이랑 타일 내리기 
    public void DownBGAndTile()
    {
        // 타일 아래로 내리기
        Vector3 movement = Vector3.down * tileSize.y + new Vector3(-playerDir, 0, 0) * tileSize.x;
        tilesContainer1.position += movement;
        tilesContainer2.position += movement;

        if (bottomContainer != null)
            bottomContainer.transform.position += movement;

        // 배경 내리기
        bg1TargetPos = bg1.position + movement / 4;
        bg2TargetPos = bg2.position + movement / 4;

        if (!smoothBGRunning)
            StartCoroutine(SmoothBG());
    }

    // 배경 부드럽게 내리기
    Vector3 bg1TargetPos, bg2TargetPos;
    bool smoothBGRunning = false;

    IEnumerator SmoothBG()
    {
        smoothBGRunning = true;
        while (bg1.position != bg1TargetPos || bg2.position != bg2TargetPos)
        {
            yield return null;
            bg2.position = Vector3.Lerp(bg2.position, bg2TargetPos, Time.deltaTime * 5);
            bg1.position = Vector3.Lerp(bg1.position, bg1TargetPos, Time.deltaTime * 5);

            if (bg1.position.y < y)
            {
                bg1.position = bg1TargetPos;
                bg2.position = bg2TargetPos;
                bg1.position += new Vector3(0, bgSize.y * 2 - 0.01f, 0);
                break;
            }

            if (bg2.position.y < y)
            {
                bg1.position = bg1TargetPos;
                bg2.position = bg2TargetPos;
                bg2.position += new Vector3(0, bgSize.y * 2 - 0.01f, 0);
                break;
            }
        }
        smoothBGRunning = false;
    }



    // 클릭 액션(오르기 / 시프트)
    public void Action(int isShift)
    {
        if (!start)
        {
            start = true;
            Time.timeScale = 1;
        }

        playerAnim.SetTrigger("walking");

        stayTime = 0;
        HPSlider.value = HPSlider.maxValue;

        AudioManager.instance.PlaySFX(AudioManager.SFXClip.CLICK);



        if (isShift == 1)
        {
            playerDir *= -1;
            Vector3 playerPos = player.transform.localScale;
            playerPos.x = -playerPos.x;
            player.transform.localScale = playerPos;
        }

        DownBGAndTile();

        if (isShift == tiles[0])
            Success();
        else
            Fail(true);


        /** 테스트용 무조건 성공 코드
        if (tiles[0] == 1)
        {
            playerDir *= -1;
            Vector3 playerPos = player.transform.localScale;
            playerPos.x = -playerPos.x;
            player.transform.localScale = playerPos;
        }

        DownBGAndTile();
        Success();
*/
    }

    // 올바르게 눌렀을 때 
    public void Success()
    {
        tiles.RemoveAt(0);

        if (isCoin[0])
        {
            GetCoin(1);
            coinTile[0].transform.GetChild(0).gameObject.SetActive(false);
            coinTile.RemoveAt(0);
        }

        isCoin.RemoveAt(0);


        int x = UnityEngine.Random.Range(0, 5);
        if (x == 4 || maxStraight == 5)
        {
            tiles.Add(SHIFT);
            maxStraight = 0;
        }
        else
        {
            tiles.Add(UP);
            maxStraight++;
        }

        // 점수 올리기
        score += 1;
        scoreTxt.text = "점수 : " + score;

        if (score % (tileCount / 2) == 0)
        { // 30점 올릴때마다 타일 재생성
            if (whichTiles == 2)
            {
                CreateTileContainer(tilesContainer1, tilesContainer2);
                whichTiles = 1;
            }
            else
            {
                CreateTileContainer(tilesContainer2, tilesContainer1);
                whichTiles = 2;
            }
        }
    }




    // 코인 먹었을 때
    void GetCoin(int coinValue)
    {
        DataManager.instance.userData.coin += coinValue;
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.SUCCESS);
        coin += coinValue;
        coinTxt.text = coin.ToString();
        overCoinTxt.text = "+ " + coin.ToString();
    }




    // 게임 오버 ( 게임오버 패널 세팅 등.. )
    public void Fail(bool isFail)
    {
        pause = true;
        Time.timeScale = 0;
        StopCoroutine("UpTimeLimit");

        topBar.SetActive(true);

        if (score > game.high_score)
        {
            game.high_score = score;
        }

        if (isFail)
            overTxt.text = "게임오버";
        else
            overTxt.text = "일시정지";

        reStartBtn.SetActive(isFail);
        returnBtn.SetActive(!isFail);

        overScoreTxt.text = "점수: " + score;
        HighScoreTxt.text = "최고: " + game.high_score;
        gameOverPanel.SetActive(true);
        DataManager.instance.saveData();
        Time.timeScale = 0;
    }

    // [1-3] 게임오버 패널 버튼 액션들
    public void ReStartAction()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnAction()
    {
        pause = false;
        StartCoroutine("UpTimeLimit");
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

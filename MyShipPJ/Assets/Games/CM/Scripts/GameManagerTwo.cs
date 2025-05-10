using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class GameManagerTwo : MonoBehaviour
{
    public static GameManagerTwo instance;
    private List<Card> allCards;
    private Card flippedCard;
    private bool isFlipping = false;


    [SerializeField]
    private Slider timeoutSlider;

    [SerializeField]
    private TextMeshProUGUI timeoutText;

    [SerializeField]
    public float timeLimit = 60f;


    private float currentTime;
    private int totalMatches = 8;
    private int matchesFound = 0;
    public int Num = 2;
    private Coroutine timerCoroutine;
    private Coroutine flipCoroutine;
    public int consecutiveMatches = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        Time.timeScale = 1;
        Debug.Log("GameManagerTwo Start 호출됨");
        Board board = FindObjectOfType<Board>();
        allCards = board.GetCards();

        currentTime = timeLimit;
        SetCurrentTimeText();
        flipCoroutine = StartCoroutine(FlipAllCardsRoutine());
    }

    public void SetCurrentTimeText()
    {
        int timeSec = Mathf.CeilToInt(currentTime);
        timeoutText.SetText(timeSec.ToString());
    }

    public IEnumerator FlipAllCardsRoutine()
    {
        isFlipping = true; 
        yield return new WaitForSeconds(0.5f);
        FlipAllCards();
        yield return new WaitForSeconds(3f);
        FlipAllCards();
        yield return new WaitForSeconds(0.5f);
        isFlipping = false;


        // 기존 타이머 코루틴 중지
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountDownTimerRoutine());
        yield return timerCoroutine;

    }

    IEnumerator CountDownTimerRoutine()
    {
        Debug.Log("타이머 시작됨");
        float startTime = Time.time;
        float endTime = startTime + timeLimit;

        while (Time.time < endTime)
        {
            currentTime = endTime - Time.time;
            timeoutSlider.value = currentTime / timeLimit;
            SetCurrentTimeText();
            yield return null;
        }

        currentTime = 0;
        timeoutSlider.value = 0;
        SetCurrentTimeText();
        CMManager.instance.GameOver(true);
    }


    /*void FlipAllCards()
    {
        foreach(Card card in allCards) 
        {
            card.FlipCard();
        }
    }*/

    void FlipAllCards()
    {
        foreach (Card card in allCards)
        {
            if (card != null)
                card.FlipCard();
        }
    }
    public void CardClicked(Card card) 
    {
        if (isFlipping)
        {
            return;
        }

        card.FlipCard();
        if (flippedCard == null)
        {
            flippedCard = card;
        }
        else
        {
            StartCoroutine(CheckMatchRoutine(flippedCard, card));
        }

       
    }
    // 코인 
    void SpawnCoinEffect(Vector3 startPos)
    {
        GameObject coin = Instantiate(CMManager.instance.coinPrefab, startPos, Quaternion.identity);
        SpriteRenderer sr = coin.GetComponent<SpriteRenderer>();

        // 코인을 위로 튀게 한 뒤 아래로 사라지게 함
        Sequence seq = DOTween.Sequence();
        seq.Append(coin.transform.DOMoveY(startPos.y + 1f, 0.3f).SetEase(Ease.OutQuad)); // 위로
        seq.Append(coin.transform.DOMoveY(-5f, 0.6f).SetEase(Ease.InQuad));              // 아래로
        seq.Join(sr.DOFade(0, 0.6f)); // 서서히 사라짐
        seq.OnComplete(() => Destroy(coin)); // 코인 오브젝트 제거
    }

    public void punch()
    {
        // 기존 코루틴 모두 정지
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);

        foreach (Card card in allCards)
        {
            Destroy(card.gameObject);
        }
        allCards.Clear();

        Board board = FindObjectOfType<Board>();
        board.InitializeBoard();
        allCards = board.GetCards();


        // 시간 줄이기
        timeLimit = Mathf.Max(5f, timeLimit - 5f);
        currentTime = timeLimit;
        timeoutSlider.value = 1f;
        SetCurrentTimeText();

        // 새 판 시작
        flipCoroutine = StartCoroutine(FlipAllCardsRoutine());
    }
    public void onepunch()
    {
        foreach (Card card in allCards)
        {
            Destroy(card.gameObject);
        }
        allCards.Clear();

        Board board = FindObjectOfType<Board>();
        board.InitializeBoard();
        allCards = board.GetCards();
        SetCurrentTimeText();

        // 새 판 시작
        flipCoroutine = StartCoroutine(FlipAllCardsRoutine());
    }
    IEnumerator CheckMatchRoutine(Card card1, Card card2)
    {
        isFlipping = true;

        if(card1.cardID == card2.cardID)
        {
            card1.SetMatched();
            card2.SetMatched();

            int gainedScore = 1 + consecutiveMatches; // 기본 1점 + 연속 보너스
            consecutiveMatches++; // 연속 정답 횟수 증가

            CMManager.instance.score += gainedScore;
            CMManager.instance.scoreTxt.text = "점수 : " + CMManager.instance.score;
            CMManager.instance.AddCoin(Num);
            SpawnCoinEffect(card1.transform.position);
            SpawnCoinEffect(card2.transform.position);
            matchesFound++;

            if (matchesFound == totalMatches)
            {
    

                matchesFound = 0;
                Num+=2;
                consecutiveMatches++;

                punch();
                yield break;

            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
            card1.FlipCard();
            card2.FlipCard();

            yield return new WaitForSeconds(0.4f);
            consecutiveMatches = 0;
        }
        yield return new WaitForSeconds(0.3f);
        flippedCard = null;
        isFlipping = false;
    }
}

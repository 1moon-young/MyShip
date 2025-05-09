using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class SnakeController : MonoBehaviour
{

    public float moveSpeed = 0.5f;
    private Vector2 direction = Vector2.right;

    private List<Transform> snakeBody = new List<Transform>();
    public Transform snakePartPrefab;
    public Transform playerContainer;
    public GameObject gameOverPanel;
    private int score = 0;
    public TextMeshProUGUI scoreText; // UI �ؽ�Ʈ ������Ʈ�� ���� ���� ǥ��
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI coinText;
    public int coin = 0;
    private bool isGameOver = false;  // Ŭ���� ������ �߰�



    private void Start()
    {
        // 1. ���õ� ĳ���͸� �Ӹ��� ����
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/" + GameManager.instance.curCharacter.name);
        GameObject head = Instantiate(playerPrefab, new Vector3(0, -1, 0), Quaternion.identity);
        head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        head.transform.SetParent(playerContainer, false);
        head.transform.position = new Vector3(0, -1, 0); // Z = 0 ���� ����
        head.SetActive(true);

        // 2. Collider2D�� ���ٸ� BoxCollider2D �߰�
        if (head.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = head.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; // Ʈ���ŷ� ����
        }

        // 3. Rigidbody2D�� ���ٸ� �߰�
        if (head.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = head.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        snakeBody.Add(head.transform); // �Ӹ��� snakeBody�� ù ��°�� ���

        // 4. �ʱ� ���� 2�� �߰�
        AddBodyPart();
        AddBodyPart();

        HeadTriggerRelay relay = head.AddComponent<HeadTriggerRelay>();
        relay.controller = this;

        UpdateScoreText();
    }

    private float moveTimer = 0f;
    private float moveDelay = 0.3f;

    private void Update()
    {
        // 1. �̵� Ÿ�̸� ����
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveDelay)
        {
            MoveSnake();  // �� �̵�
            moveTimer = 0f;
        }

        // 2. ���� ���� üũ (��尡 ȭ���� ��� ���)
        CheckGameOver();
    }

    private void MoveSnake()
    {
        // �� �Ӹ� ��ġ ��� (���� ��ġ�� ����)
        Vector2 newHeadPosition = (Vector2)snakeBody[0].position + direction;
        newHeadPosition.x = Mathf.Round(newHeadPosition.x);
        newHeadPosition.y = Mathf.Round(newHeadPosition.y);

        // ������ �պκ� ��ġ�� �̵�
        for (int i = snakeBody.Count - 1; i > 0; i--)
        {
            snakeBody[i].position = new Vector3(
                snakeBody[i - 1].position.x,
                snakeBody[i - 1].position.y,
                0f
            );
        }

        // �Ӹ� �̵�
        snakeBody[0].position = new Vector3(newHeadPosition.x, newHeadPosition.y, 0f);
    }


    public void SetDirection(string newDirection)
    {
        switch (newDirection)
        {
            case "Up": direction = Vector2.up; break;
            case "Down": direction = Vector2.down; break;
            case "Left": direction = Vector2.left; break;
            case "Right": direction = Vector2.right; break;
        }
    }

    private void AddBodyPart()
    {
        // ���� ��ġ�� �Ӹ��� ��ġ���� �� ĭ �������� ���� (���� �Ӹ� ���� �ݴ���)
        Vector2 spawnPos = (Vector2)snakeBody[snakeBody.Count - 1].position - direction;

        // ���ο� ���� ����
        Transform newPart = Instantiate(snakePartPrefab, spawnPos, Quaternion.identity, playerContainer);

        // �±� ����
        newPart.tag = "Body";

        // �ݶ��̴� ������ �߰�
        if (newPart.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = newPart.gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;  // Ʈ���� ����
        }

        // ������ ������ ����Ʈ�� �߰�
        snakeBody.Add(newPart);
    }



    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("[SnakeController] OnTriggerEnter2D ȣ��� - �浹 ���: " + collision.name);

        if (collision.CompareTag("Food"))
        {
            Destroy(collision.gameObject);
            AddBodyPart();
            SnakeManager.instance.SpawnFood();

            // ���� ���� �� UI ���� �߰�
            score += 2;
            coin += 1;
            UpdateScoreText();

            SnakeManager.instance.AddScore(2);
            SnakeManager.instance.AddCoin(1);
        }
        else if (collision.CompareTag("Body"))
        {
            GameOver();
        }
    }


    private void UpdateScoreText()
    {
        if(scoreText != null)
        {
            scoreText.text = "����: " + score.ToString();
        }
    }
    






    private void CheckGameOver()
    {
        Vector3 headPosition = snakeBody[0].position;

        // ī�޶��� ���� ��ǥ�� ��� ���� ȭ�� ũ�� ���
        Vector3 topLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)); // ȭ���� ���� ��
        Vector3 bottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)); // ȭ���� ������ �Ʒ�

        // ��� ��ġ�� ȭ���� ����ٸ� ���� ���� ó��
        if (headPosition.x < topLeft.x || headPosition.x > bottomRight.x || headPosition.y > topLeft.y || headPosition.y < bottomRight.y)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;  // �̹� ���� ������ ����
        isGameOver = true;
        SnakeManager.instance.score = score;
        SnakeManager.instance.coin = Mathf.FloorToInt(score / 2f);
        SnakeManager.instance.Fail(true);
    }

    // SnakeController.cs
    public void HandleTriggerEnter(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }







}

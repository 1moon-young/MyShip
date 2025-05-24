using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Asteroid_Character : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] float moveSpeed = 5f; // ĳ���� �̵� �ӵ�

    private float minX, maxX, minY, maxY; // ȭ�� ��� ��ǥ
    private float characterHalfWidth, characterHalfHeight; // ĳ���� ũ���� ��
    private int characterDirection = -1; // ĳ���� ���� (-1: ������, 1: ����)

    // ���̽�ƽ ���� ����
    [SerializeField] private GameObject joystickPrefab; // ���̽�ƽ ������
    private GameObject currentJoystick; // ���� ���̽�ƽ ��ü
    private Image joystickBackground; // ���̽�ƽ ��� �̹���
    private Image joystickHandle; // ���̽�ƽ �ڵ� �̹���
    private Vector2 joystickTouchStartPosition; // ���̽�ƽ ��ġ ���� ��ġ
    private bool isJoystickActive = false; // ���̽�ƽ Ȱ��ȭ ����
    private const float joystickRadius = 100f; // ���̽�ƽ �ݰ�
    private const float handleMoveRadius = 50f; // �ڵ� �̵� �ݰ�

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        CalculateScreenBounds(); // ȭ�� ��� ���
    }

    void Update()
    {
        HandleJoystickInput(); // ���̽�ƽ �Է� ó��
        ClampPosition(); // ȭ�� ��� ���� ��ġ ����
    }

    void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
    }

    void CalculateScreenBounds()
    {
        // ĳ���� �ݶ��̴� ũ�� ���
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            characterHalfWidth = collider.bounds.extents.x;
            characterHalfHeight = collider.bounds.extents.y;
        }
        else
        {
            characterHalfWidth = 0.5f;
            characterHalfHeight = 0.5f;
        }

        // ī�޶� ����Ʈ ���
        float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float cameraHalfHeight = Camera.main.orthographicSize;

        // ȭ�� ��� ��� (ĳ���Ͱ� ȭ�� ������ ������ �ʵ���)
        minX = Camera.main.transform.position.x - cameraHalfWidth + characterHalfWidth;
        maxX = Camera.main.transform.position.x + cameraHalfWidth - characterHalfWidth;
        minY = Camera.main.transform.position.y - cameraHalfHeight + characterHalfHeight;
        maxY = Camera.main.transform.position.y + cameraHalfHeight - characterHalfHeight;
    }

    void HandleJoystickInput()
    {
        // ���콺 Ŭ�� ���� (���̽�ƽ ����)
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            CreateJoystick(Input.mousePosition);
            isJoystickActive = true;
            joystickTouchStartPosition = Input.mousePosition;
        }
        // ���콺 �巡�� (���̽�ƽ ����)
        else if (Input.GetMouseButton(0) && isJoystickActive)
        {
            Vector2 currentTouchPosition = Input.mousePosition;
            Vector2 direction = currentTouchPosition - joystickTouchStartPosition;
            float distance = direction.magnitude;

            // ���̽�ƽ �ڵ� �̵�
            Vector2 handlePosition = direction.normalized * Mathf.Min(distance, handleMoveRadius);
            joystickHandle.rectTransform.anchoredPosition = handlePosition;

            // ĳ���� �̵�
            Vector2 movement = direction.normalized * moveSpeed;
            rigid.velocity = movement;

            // ĳ���� ���� ��ȯ
            if (movement.x > 0 && characterDirection != -1)
            {
                characterDirection = -1;
                FlipCharacter();
            }
            else if (movement.x < 0 && characterDirection != 1)
            {
                characterDirection = 1;
                FlipCharacter();
            }
        }
        // ���콺 Ŭ�� ���� (���̽�ƽ ����)
        else if (Input.GetMouseButtonUp(0) && isJoystickActive)
        {
            DestroyJoystick();
            rigid.velocity = Vector2.zero;
            isJoystickActive = false;
        }
    }

    void CreateJoystick(Vector2 screenPosition)
    {
        if (joystickPrefab != null)
        {
            currentJoystick = Instantiate(joystickPrefab, screenPosition, Quaternion.identity);
            currentJoystick.transform.SetParent(GameObject.Find("Canvas").transform, false);

            // ���̽�ƽ ������Ʈ ��������
            joystickBackground = currentJoystick.GetComponent<Image>();
            joystickHandle = currentJoystick.transform.GetChild(0).GetComponent<Image>();

            // �ʱ� ��ġ ����
            joystickHandle.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    void DestroyJoystick()
    {
        if (currentJoystick != null)
        {
            Destroy(currentJoystick);
            currentJoystick = null;
            joystickBackground = null;
            joystickHandle = null;
        }
    }

    void FlipCharacter()
    {
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * characterDirection;
        transform.localScale = newScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Obstacle")
        {
            Asteroid_Manager.instance.GameOver(isOver: true);
        }

        if (other.tag == "Coin")
        {
            AudioManager.instance.PlaySFX(AudioManager.SFXClip.SUCCESS);
            Destroy(other.gameObject);
            Asteroid_Manager.instance.GetCoin();
        }
    }
}
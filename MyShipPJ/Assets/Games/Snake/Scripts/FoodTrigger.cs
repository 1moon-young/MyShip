using UnityEngine;

public class FoodTrigger : MonoBehaviour
{
    public SnakeController snakeController; // SnakeController�� �����ϱ� ���� ����

    void Start()
    {
        // SnakeController�� ������ �ڵ����� ã��
        if (snakeController == null)
        {
            snakeController = FindObjectOfType<SnakeController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))  // �÷��̾�(�Ӹ�)�� �浹 ��
        {
            Debug.Log("FoodTrigger�� �浹!");

            // SnakeController�� �浹 ó�� �޼��� ȣ��
            snakeController.OnTriggerEnter2D(collision);

            // �浹 �� ���� ���� (�浹�� OnTriggerEnter2D���� ó����)
            Destroy(gameObject);  // ���� ������Ʈ ����
        }
    }
}

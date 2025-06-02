using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int dmg;

    void Update()
    {
        // ȭ�� ������ ������ ����
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.y > 1.1f || viewPos.y < -0.1f || viewPos.x < -0.1f || viewPos.x > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (CompareTag("EnemyBullet") && collision.CompareTag("Player"))
        {
            Destroy(collision.gameObject);  // �÷��̾� ����
            Time.timeScale = 0f;  // ���� ���߱�
            Destroy(gameObject);  // �� �Ѿ� ����
            SSManager.instance.GameOver(true);
        }
    }

    }
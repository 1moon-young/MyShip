using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int dmg;

    void Update()
    {
        // 화면 밖으로 나가면 삭제
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
            Destroy(collision.gameObject);  // 플레이어 삭제
            Time.timeScale = 0f;  // 게임 멈추기
            Destroy(gameObject);  // 적 총알 삭제
            SSManager.instance.GameOver(true);
        }
    }

    }
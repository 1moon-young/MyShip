using UnityEngine;

public class Asteroid_Obstacle : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.down; // �⺻�� ���� (Manager���� ��� ��)

    void Update()
    {
        Move();
        //FlipX();
    }

    void Move()
    {
        // ������ �������� �̵�
        Vector3 movement = moveDirection * Time.deltaTime * Asteroid_Manager.instance.speed;
        transform.position += movement;

        // ȭ�� ������ �������� üũ (��� ���⿡ ����)
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void FlipX()
    {
        // X������ �ֱ������� ���� (�ð��� ȿ��)
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}
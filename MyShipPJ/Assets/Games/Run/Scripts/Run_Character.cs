using UnityEngine;

public class Run_Character : MonoBehaviour
{
    Rigidbody2D rigid;
    public float force = 5f;
    public float fastFallGravity = 8f; // ������ ������ ���� �߷� ��
    private float normalGravity = 3f; // �Ϲ� �߷� ��
    private bool isGrounded = false; // ���� ��Ҵ��� ����
    private bool isFastFalling = false; // ������ �������� ������ ����

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.freezeRotation = true; // ȸ�� ���� (������ �ذ�)
        rigid.gravityScale = normalGravity; // �߷� �ʱⰪ ����
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Jump_and_Fall();
        }
    }

    void Jump_and_Fall()
    {
        if (isGrounded)
            Jump();
        else if (!isFastFalling && rigid.velocity.y < 0) //���� �߿��� FastFall ����
            FastFall();
    }

    void Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, force); // ���� X �ӵ� ����
        isGrounded = false;
        isFastFalling = false;
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.FAIL);
    }

    void FastFall()
    {
        isFastFalling = true;
        rigid.gravityScale = fastFallGravity; // �߷� ������ ������ ������
        AudioManager.instance.PlaySFX(AudioManager.SFXClip.FAIL);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Obstacle")
        {
            RunManager.instance.GameOver(isOver: true);
        }
        else if (other.tag == "Coin")
        {
            AudioManager.instance.PlaySFX(AudioManager.SFXClip.SUCCESS);
            Destroy(other.gameObject);
            RunManager.instance.GetCoin();
        }
        else if (other.tag == "Untagged")
        {
            Land();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Untagged")
        {
            Land();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Untagged")
        {
            isGrounded = false; // ������ ������
        }
    }

    private void Land() //�� ���� ����
    {
        isGrounded = true;
        isFastFalling = false;
        rigid.gravityScale = normalGravity; // �߷� ������� ����
        rigid.velocity = new Vector2(rigid.velocity.x, 0); // Y�� �ӵ� 0���� ����
    }
}
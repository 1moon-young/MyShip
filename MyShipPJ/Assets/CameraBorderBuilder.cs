using UnityEngine;

public class CameraBorderBuilder : MonoBehaviour
{
    public float wallThickness = 1f;  // ���� �β�

    void Start()
    {
        Camera cam = Camera.main;

        // ī�޶��� ���� ũ�� ���
        float camHeight = cam.orthographicSize * 2f;

        // ī�޶��� ���� ũ�� ���
        float camWidth = camHeight * cam.aspect;

        // ī�޶��� z ��ġ (z ���� ���� ī�޶�� ��ġ�ϵ��� ����)
        float camZ = cam.transform.position.z;

        // �� ����
        CreateWall("Wall_Top", new Vector2(0, camHeight / 2f + wallThickness / 2f), new Vector2(camWidth, wallThickness), camZ);
        CreateWall("Wall_Bottom", new Vector2(0, -camHeight / 2f - wallThickness / 2f), new Vector2(camWidth, wallThickness), camZ);
        CreateWall("Wall_Left", new Vector2(-camWidth / 2f - wallThickness / 2f, 0), new Vector2(wallThickness, camHeight), camZ);
        CreateWall("Wall_Right", new Vector2(camWidth / 2f + wallThickness / 2f, 0), new Vector2(wallThickness, camHeight), camZ);
    }

    void CreateWall(string name, Vector2 position, Vector2 size, float zPos)
    {
        // �� ����
        GameObject wall = new GameObject(name);
        wall.transform.position = new Vector3(position.x, position.y, zPos);  // �� ��ġ ����

        // SpriteRenderer �߰�
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateGraySprite();  // ȸ�� ��������Ʈ ����
        sr.color = Color.gray;  // ȸ������ ���� ����
        sr.sortingOrder = 0;  // ���� ���� ����

        wall.transform.localScale = size;  // �� ũ�� ����
        wall.AddComponent<BoxCollider2D>();  // �浹 ���� �߰�
        wall.transform.parent = this.transform;  // �θ� ��ü�� ����
    }

    // ȸ�� ��������Ʈ ���� �Լ�
    Sprite GenerateGraySprite()
    {
        // 1x1 �ȼ��� ȸ�� �ؽ�ó ����
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.gray);  // ȸ�� �� ����
        tex.Apply();

        // �ؽ�ó�� ��������Ʈ�� ��ȯ
        Rect rect = new Rect(0, 0, 1, 1);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 1f);
    }
}

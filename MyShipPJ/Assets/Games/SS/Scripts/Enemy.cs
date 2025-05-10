using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public float speed;
    public int health;
    public Sprite[] sprites;

    public GameObject player;
    // 적 발사체 
    public GameObject Bullets_0;
    public GameObject Bullets_1;
    public float maxShotDelay ;
    public float curShotDelay ; 

    SpriteRenderer spriteRenderer;
    Rigidbody2D rigid;


    void Update()
    {
        curShotDelay += Time.deltaTime;
        fire();
       
    }

    void fire()
    {
        if (player == null)
        {
            return;
        }

        if (curShotDelay < maxShotDelay)
            return;

        if (enemyName == "M")
        {

            GameObject bullet = Instantiate(Bullets_0, transform.position, transform.rotation);
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector3 dirVec = player.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

        }
        else if (enemyName == "L")
        {


            GameObject bulletR = Instantiate(Bullets_1, transform.position, transform.rotation);
            GameObject bulletL = Instantiate(Bullets_1, transform.position, transform.rotation);

            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();

            Vector3 dirVecR = player.transform.position - (transform.position + Vector3.right*0.3f);
            Vector3 dirVecL = player.transform.position - (transform.position + Vector3.left * 0.3f); 

            rigidR.AddForce(dirVecR.normalized * 3, ForceMode2D.Impulse);
            rigidL.AddForce(dirVecL.normalized * 3, ForceMode2D.Impulse);
        }
        curShotDelay = 0;
    }



    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void OnHit(int dmg)
    {
        health -= dmg;
        spriteRenderer.sprite = sprites[1];
        Invoke("ReturnSprite", 0.1f);

        if (health <= 0)
        {
            int coinCount = 1; // 기본 개수
            if (enemyName == "M") coinCount = 2;
            else if (enemyName == "L") coinCount = 3;

            float spacing = 0.3f; // 코인 간격

            for (int i = 0; i < coinCount; i++)
            {
                // 중심 기준으로 좌우로 나누어 생성
                float xOffset = (i - (coinCount - 1) / 2f) * spacing;
                Vector3 spawnPos = transform.position + new Vector3(xOffset, 0, 0);

                GameObject coin = Instantiate(SSManager.instance.coinPrefab, spawnPos, Quaternion.identity);

                Rigidbody2D coinRigid = coin.GetComponent<Rigidbody2D>();
                if (coinRigid != null)
                {
                    coinRigid.velocity = new Vector2(0, -2f); // 직선 아래 방향
                }
            }

            Destroy(gameObject);
        }
    }



    void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BulletWall")
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "PlayerBullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            OnHit(bullet.dmg);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        { 
            Destroy(collision.gameObject);  // 플레이어 삭제
            // 게임 멈추기
            SSManager.instance.GameOver(true);
            Time.timeScale = 0;

        }

       
    }
}
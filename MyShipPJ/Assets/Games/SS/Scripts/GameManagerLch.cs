using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerLch : MonoBehaviour
{
    public static GameManagerLch instance; //ㅅㄱㅌ

    public GameObject[] enemyObjs;
    public GameObject[] spawnPoints;
    public GameObject spawnPointGroup;
    public float maxSpawnDelay;
    public float curSpawnDelay;

    public GameObject player; //ㅅㄱㅌ

    void Awake()
    {
        // 싱글턴 초기화
        if (instance == null)
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            DontDestroyOnLoad(gameObject);
          
        }
        else
        {
            Destroy(gameObject);
            
        }


       
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawnPointGroup = GameObject.FindGameObjectWithTag("SpawnPointGroup");

        if (spawnPointGroup == null)
        {
            return;
        }
        /*Transform point0 = spawnPointGroup.transform.Find("point0");
        Transform point1 = spawnPointGroup.transform.Find("point1");
        Transform point2 = spawnPointGroup.transform.Find("point2");
        Transform point3 = spawnPointGroup.transform.Find("point3");
        Transform point4 = spawnPointGroup.transform.Find("point4");
        Transform point5 = spawnPointGroup.transform.Find("point5");
        Transform point6 = spawnPointGroup.transform.Find("point6");
        Transform point7 = spawnPointGroup.transform.Find("point7");
        Transform point8 = spawnPointGroup.transform.Find("point8");

        for (int i = 0; i < 9; i++)
        {
            spawnPoints
        }
        */
        for (int i = 0; i < spawnPointGroup.transform.childCount; i++)
        {
            Transform child = spawnPointGroup.transform.GetChild(i);
            spawnPoints[i] = child.gameObject;
        }
    }

        void Update()
    {
        curSpawnDelay += Time.deltaTime;

        if (curSpawnDelay > maxSpawnDelay)
        {
            SpawnEnemy();
            maxSpawnDelay = Random.Range(0.5f, 3f);
            curSpawnDelay = 0;
        }
    }

    void SpawnEnemy()
    {
        int ranEnemy = Random.Range(0, 3);
        int ranPoint = Random.Range(0, 9);

        if (enemyObjs[ranEnemy] == null || spawnPoints[ranPoint] == null)
        {
            return;
        }
      


        GameObject enemy = Instantiate(enemyObjs[ranEnemy],
                                        spawnPoints[ranPoint].transform.position,
                                         spawnPoints[ranPoint].transform.rotation);

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.player = player; //싱글턴에서 player 참조

        if (ranPoint == 5 || ranPoint == 6)
        { //오른
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), -1);
            enemy.transform.Rotate(Vector3.back * 70);
        }
        else if (ranPoint == 7 || ranPoint == 8)
        {  //왼 
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
            enemy.transform.Rotate(Vector3.forward * 70);
        }
        else
        { // 직선구간
            rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
        }
    }
}
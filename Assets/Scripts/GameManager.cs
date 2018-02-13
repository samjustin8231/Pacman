using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Gamemanager,控制游戏中的UI
public class GameManager : MonoBehaviour
{
    //使用单例模式
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public GameObject pacman;               //pacman
    //5个敌人对象
    public GameObject blinky;
    public GameObject clyde;
    public GameObject inky;
    public GameObject pinky;

    public GameObject startPanel;           //startPanel
    public GameObject gamePanel;            //gamePanel
    public GameObject startCountDownPrefab; //CountDown prefab
    public GameObject gameoverPrefab;       //game over prefab
    public GameObject winPrefab;            //win prefab


    public AudioClip startClip;             //开始时的音乐

    //gamePanel里面的子元素
    public Text remainText;
    public Text nowText;
    public Text scoreText;

    public bool isSuperPacman = false;      //当前状态是否是超级无敌的

    //用来控制敌人的多路径巡逻不会重复
    public List<int> usingIndex = new List<int>();
    public List<int> rawIndex = new List<int> { 0, 1, 2, 3 };

    private List<GameObject> pacdotGos = new List<GameObject>();    //存放所有的豆子

    private int pacdotNum = 0;              //总豆子数目
    private int nowEat = 0;                 //已经吃掉的豆子数目
    public int score = 0;                   //总得分

    //脚本对象实例化时调用，Start()是在对象的第一帧时被调用的，而且是在Update()之前
    private void Awake()
    {
        _instance = this;                   //单例实例化

        Screen.SetResolution(1024, 768, false);

        //用来控制敌人的多路径巡逻不会重复
        int tempCount = rawIndex.Count;
        for (int i = 0; i < tempCount; i++)
        {
            int tempIndex = Random.Range(0, rawIndex.Count);
            usingIndex.Add(rawIndex[tempIndex]);
            rawIndex.RemoveAt(tempIndex);
        }

        //初始化所有的豆子，GameObject.Find("Maze").transform得到的是transform数组
        foreach (Transform t in GameObject.Find("Maze").transform)
        {
            pacdotGos.Add(t.gameObject);
        }
        //总豆子数目
        pacdotNum = GameObject.Find("Maze").transform.childCount;
    }

    private void Start()
    {
        //游戏刚启动时，pacman和敌人的enable都设置为false
        SetGameState(false);
    }

    private void Update()
    {
        //吃的豆=总豆数 && pacman的脚本还在工作
        if (nowEat == pacdotNum && pacman.GetComponent<PacmanMove>().enabled != false)
        {
            //gamePanel隐藏
            gamePanel.SetActive(false);
            //实例化一个winPrefab
            Instantiate(winPrefab);
            //停止所有的线程
            StopAllCoroutines();
            //游戏状态 = false
            SetGameState(false);
        }
        if (nowEat == pacdotNum)
        {
            //已经win的时候，按下任何键重新加载scene
            if (Input.anyKeyDown)
            {
                SceneManager.LoadScene(0);
            }
        }
        //当前gamePanel是active的
        if (gamePanel.activeInHierarchy)
        {
            //则显示game数据
            remainText.text = "Remain:\n\n" + (pacdotNum - nowEat);
            nowText.text = "Eaten:\n\n" + nowEat;
            scoreText.text = "Score:\n\n" + score;
        }
    }

    //开始游戏按钮
    public void OnStartButton()
    {
        //另起一个线程执行倒计时
        StartCoroutine(PlayStartCountDown());
        AudioSource.PlayClipAtPoint(startClip, new Vector3(0, 0, -5));
        startPanel.SetActive(false);
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    IEnumerator PlayStartCountDown()
    {
        //实例化 startCountDownPrefab
        GameObject go = Instantiate(startCountDownPrefab);
        //该线程4s后执行下面的操作
        yield return new WaitForSeconds(4f);

        //4s后，倒计时动画播放完了：销毁该实例，游戏状态为true,10s之后创建超级豆子，gamePanel显示，播放bgm
        Destroy(go);
        SetGameState(true);
        Invoke("CreateSuperPacdot", 10f);
        gamePanel.SetActive(true);
        GetComponent<AudioSource>().Play();
    }

    public void OnEatPacdot(GameObject go)
    {
        //吃到豆子: nowEat++,score += 100, 移除吃到的豆子
        nowEat++;
        score += 100;
        //将豆子从list中移除
        pacdotGos.Remove(go);
    }

    public void OnEatSuperPacdot()
    {
        //吃到超级豆子：10s之后重新创建超级豆子，当前为超级状态，
        score += 200;
        Invoke("CreateSuperPacdot", 10f);
        isSuperPacman = true;
        //冻结所有的敌人
        FreezeEnemy();  
        //开启线程，3秒后恢复敌人
        StartCoroutine(RecoveryEnemy());
    }

    IEnumerator RecoveryEnemy()
    {
        yield return new WaitForSeconds(3f);
        DisFreezeEnemy();
        isSuperPacman = false;
    }

    private void CreateSuperPacdot()
    {
        if (pacdotGos.Count < 5)
        {
            return;
        }
        //随机生成一个超级豆
        int tempIndex = Random.Range(0, pacdotGos.Count);
        //超级豆放大
        pacdotGos[tempIndex].transform.localScale = new Vector3(3, 3, 3);
        pacdotGos[tempIndex].GetComponent<Pacdot>().isSuperPacdot = true;
    }

    private void FreezeEnemy()
    {
        //所有的敌人的脚本disable,此时这些脚本的update()不会执行
        blinky.GetComponent<GhostMove>().enabled = false;
        clyde.GetComponent<GhostMove>().enabled = false;
        inky.GetComponent<GhostMove>().enabled = false;
        pinky.GetComponent<GhostMove>().enabled = false;

        //将这些敌人的颜色变暗
        blinky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        clyde.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        inky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        pinky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    }

    private void DisFreezeEnemy()
    {
        //恢复敌人的脚本
        blinky.GetComponent<GhostMove>().enabled = true;
        clyde.GetComponent<GhostMove>().enabled = true;
        inky.GetComponent<GhostMove>().enabled = true;
        pinky.GetComponent<GhostMove>().enabled = true;

        //恢复敌人的颜色
        blinky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        clyde.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        inky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        pinky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    }

    //设置游戏中的人物状态
    private void SetGameState(bool state)
    {
        pacman.GetComponent<PacmanMove>().enabled = state;
        blinky.GetComponent<GhostMove>().enabled = state;
        clyde.GetComponent<GhostMove>().enabled = state;
        inky.GetComponent<GhostMove>().enabled = state;
        pinky.GetComponent<GhostMove>().enabled = state;
    }
}

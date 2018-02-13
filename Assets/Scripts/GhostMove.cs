using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//敌人脚本
public class GhostMove : MonoBehaviour
{
    //敌人移动路径数组，里面存放的是所有路径的prefab对象
    //每一条路径中的点构成了一个闭合的环
    public GameObject[] wayPointsGos;

    public float speed = 0.2f;

    //某一条路径中的所有点
    private List<Vector3> wayPoints = new List<Vector3>();  
    //敌人目前在一条路径中的位置
    private int index = 0;
    //移动的开始位置
    private Vector3 startPos;

    private void Start()
    {
        //每个敌人移动的开始位置：自己的正上方3个unit
        //transform其实是 this.transform
        startPos = transform.position + new Vector3(0, 3, 0);
        //加载一条路径
        LoadAPath(wayPointsGos[GameManager.Instance.usingIndex[GetComponent<SpriteRenderer>().sortingOrder - 2]]);
    }

    private void FixedUpdate()
    {
        if (transform.position != wayPoints[index])
        {
            //平滑移动到下一个位置
            Vector2 temp = Vector2.MoveTowards(transform.position, wayPoints[index], speed);
            GetComponent<Rigidbody2D>().MovePosition(temp);
        }
        else  
        {
            //敌人的位置目前处于路径中的某个点,则移动到下一个点
            index++;
            if (index >= wayPoints.Count)
            {
                //如果到最后一个点了，重新加载一条随机的路径
                index = 0;
                LoadAPath(wayPointsGos[Random.Range(0, wayPointsGos.Length)]);
            }
        }
        //当前移动的方向
        Vector2 dir = wayPoints[index] - transform.position;
        //设置动画里面的DirX和DirY,这样动画内部根据状态自动显示接下来的动画
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
    }

    /**
     * go是一条路径的prefab
     * 里面有许多的点
     * */
    private void LoadAPath(GameObject go)
    {
        //先清空wayPoints
        wayPoints.Clear();

        foreach (Transform t in go.transform)
        {
            wayPoints.Add(t.position);
        }
        //插入路径起点
        wayPoints.Insert(0, startPos);
        //插入路径终点
        wayPoints.Add(startPos);
    }

    //碰撞检测到了物体
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //接触到pacman
        if (collision.gameObject.name == "Pacman")
        {
            //pacman是无敌状态
            if (GameManager.Instance.isSuperPacman)
            {
                //自己(敌人)需要回到起点
                transform.position = startPos - new Vector3(0, 3, 0);
                index = 0;
                GameManager.Instance.score += 500;
            }
            else
            {
                //pacman处于普通状态
                //将pacman隐藏
                collision.gameObject.SetActive(false);
                //隐藏gamePanel
                GameManager.Instance.gamePanel.SetActive(false);
                //实例化gameoverPrefab
                Instantiate(GameManager.Instance.gameoverPrefab);
                //3s之后restart游戏
                Invoke("ReStart", 3f);
            }
        }
    }

    private void ReStart()
    {
        SceneManager.LoadScene(0);
    }
}

using UnityEngine;

//豆子
public class Pacdot : MonoBehaviour
{
    //是否是超级豆
    public bool isSuperPacdot = false;

    //碰撞检测到了物体
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //被pacman碰到了
        if (collision.gameObject.name == "Pacman")
        {
            //如果是超级豆
            if (isSuperPacdot)
            {
                GameManager.Instance.OnEatPacdot(gameObject);
                GameManager.Instance.OnEatSuperPacdot();
                Destroy(gameObject);
            }
            else
            {
                //普通豆，调用被吃掉方法：将豆子从list移除
                GameManager.Instance.OnEatPacdot(gameObject);
                //销毁豆子
                Destroy(gameObject);
            }
        }
    }
}

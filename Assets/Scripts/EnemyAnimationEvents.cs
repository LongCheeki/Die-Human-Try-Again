using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    public EnemyAI enemyAI;

    public void DealDamage()
    {
        if (enemyAI != null)
        {
            enemyAI.DealDamage();
        }
    }
}
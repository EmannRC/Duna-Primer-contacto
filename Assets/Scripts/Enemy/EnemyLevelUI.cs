using TMPro;
using UnityEngine;

public class EnemyLevelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;

    private EnemyStats enemyStats;

    //==============================================================//

    private void Awake()
    {
        enemyStats = GetComponentInParent<EnemyStats>();
    }

    //==============================================================//

    private void Start()
    {
        if (enemyStats == null)
            enemyStats = GetComponentInParent<EnemyStats>();

        UpdateLevel();
    }

    //==============================================================//

    private void UpdateLevel()
    {
        if (enemyStats == null || levelText == null)
            return;

        levelText.text = "Lv. " + enemyStats.Level;
    }
}

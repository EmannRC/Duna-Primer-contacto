using UnityEngine;

namespace Duna.QuestSystem
{
    public class QuestAreaTrigger : MonoBehaviour
    {
        [SerializeField]
        private string areaID;

        [SerializeField]
        private EnemySpawner enemySpawner;

        private bool activated;


        private void OnTriggerEnter(Collider other)
        {
            if (activated)
                return;

            if (!other.CompareTag("Player"))
                return;

            activated = true;

            QuestEvents.RaiseReachArea(areaID);

            if (enemySpawner != null)
            {
                enemySpawner.SpawnEnemies();
            }
        }
    }
}

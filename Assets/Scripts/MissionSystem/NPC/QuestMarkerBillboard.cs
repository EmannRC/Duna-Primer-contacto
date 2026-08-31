using UnityEngine;

namespace Duna.QuestSystem.UI
{
    public class QuestMarkerBillboard : MonoBehaviour
    {
        private Camera targetCamera;

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;

                if (targetCamera == null)
                    return;
            }

            Vector3 direction =
                targetCamera.transform.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f, 0f);
        }
    }
}

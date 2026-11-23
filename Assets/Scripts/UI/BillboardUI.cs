using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

public class BillboardUI : MonoBehaviour
{
    [Header("UI a rotar")]
    [SerializeField] private GameObject billboardToRotate;

    [Header("Configuración")]
    [SerializeField] private bool faceCamera = true;

    private Camera localCamera;



    private void LateUpdate()
    {
        RotateBillboardTowerCamera();
    }

    private void RotateBillboardTowerCamera()
    {
        // Hacer que el panel mire siempre hacia la cámara del jugador local
        if (!faceCamera || billboardToRotate == null || !billboardToRotate.activeSelf)
            return;

        if (localCamera == null)
        {
            if (LocalPlayerBootstrap.Instance != null &&
                LocalPlayerBootstrap.Instance.LocalPlayer != null)
            {
                localCamera = LocalPlayerBootstrap.Instance.LocalPlayer.mainCamera;
            }
            else
            {
                localCamera = Camera.main;
            }
        }

        if (localCamera != null)
        {
            billboardToRotate.transform.rotation = localCamera.transform.rotation;
        }
    }
}

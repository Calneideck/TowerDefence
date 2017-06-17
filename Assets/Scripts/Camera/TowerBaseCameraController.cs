using UnityEngine;

public class TowerBaseCameraController : MonoBehaviour
{
    void Update()
    {
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;
    }
}

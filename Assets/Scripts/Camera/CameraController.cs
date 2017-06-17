using UnityEngine;

public class CameraController : MonoBehaviour {

    const float MIDDLE_MOUSE_MOVE_FACTOR = 0.05f;
    private bool canPan;
    private bool middleMouseDown = false;
    private Vector3 lastMousePos;

    public float cameraHeight = 20;
    public Camera minimapCamera;
    public LineRenderer[] lines;
    public GameObject guiCanvas;
    public RoundManager roundManager;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canPan)
        {
            KeysMove();
            ScreenMove();
            MiddleMouseMove();
        }
	}

    public void OnMovedToSpawn()
    {
        canPan = true;
        roundManager.StartCountdown();
        GetComponent<Animator>().enabled = false;
        minimapCamera.gameObject.SetActive(true);
        minimapCamera.pixelRect = new Rect(5, 5, 170, 170);
        guiCanvas.SetActive(true);
    }

    void Update()
    {
        if (canPan)
            MinimapMove();
        SetCameraLines();
        if (Input.GetMouseButtonDown(2)) //Middle Mouse
        {
            middleMouseDown = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
            middleMouseDown = false;
    }

    void KeysMove()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && transform.position.x > -34)
            transform.Translate(new Vector3(-0.8f, 0, 0), Space.World);
        if (Input.GetKey(KeyCode.RightArrow) && transform.position.x < 34)
            transform.Translate(new Vector3(0.8f, 0, 0), Space.World);
        if (Input.GetKey(KeyCode.UpArrow) && transform.position.z < 10)
            transform.Translate(new Vector3(0, 0, 0.8f), Space.World);
        if (Input.GetKey(KeyCode.DownArrow) && transform.position.z > -20)
            transform.Translate(new Vector3(0, 0, -0.8f), Space.World);
    }

    void ScreenMove()
    {
        if (Input.mousePosition.x < 10 && transform.position.x > -34)
            transform.Translate(new Vector3(-0.8f, 0, 0), Space.World);
        if (Input.mousePosition.x > Screen.width - 10 && transform.position.x < 34)
            transform.Translate(new Vector3(0.8f, 0, 0), Space.World);
        if (Input.mousePosition.y < 10 && transform.position.z > -20)
            transform.Translate(new Vector3(0, 0, -0.8f), Space.World); //Down
        if (Input.mousePosition.y > Screen.height - 10 && transform.position.z < 10)
            transform.Translate(new Vector3(0, 0, 0.8f), Space.World);  //Up
    }

    void MiddleMouseMove()
    {
        if (middleMouseDown)
        {
            if (lastMousePos != Input.mousePosition)
            {
                Vector3 diff = (lastMousePos - Input.mousePosition) * MIDDLE_MOUSE_MOVE_FACTOR;

                Vector3 pos;
                pos.x = transform.position.x + diff.x;
                pos.x = Mathf.Clamp(pos.x, -34, 34);

                pos.y = cameraHeight;

                pos.z = transform.position.z + diff.y;
                pos.z = Mathf.Clamp(pos.z, -20, 10);

                Camera.main.transform.position = pos;
                lastMousePos = Input.mousePosition;
            }
        }
    }

    void MinimapMove()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x > 5 && Input.mousePosition.x < 170 && Input.mousePosition.y > 5 && Input.mousePosition.y < 170)
            {
                Vector3 pos = GetMousePositionOnXZPlane();
                pos.y = cameraHeight;
                pos.z -= 9;
                Camera.main.transform.position = pos;
            }
        }
    }

    void SetCameraLines()
    {
        Vector3 pos;
        // Top
        pos = GetScreenPosOnXZPlane(new Vector3(0, Screen.height, 0));
        lines[0].SetPosition(0, pos);
        pos = GetScreenPosOnXZPlane(new Vector3(Screen.width, Screen.height, 0));
        lines[0].SetPosition(1, pos);

        // Right
        pos = GetScreenPosOnXZPlane(new Vector3(Screen.width, 0, 0));
        lines[1].SetPosition(0, pos);
        pos = GetScreenPosOnXZPlane(new Vector3(Screen.width, Screen.height, 0));
        lines[1].SetPosition(1, pos);

        // Bottom
        pos = GetScreenPosOnXZPlane(new Vector3(0, 0, 0));
        lines[2].SetPosition(0, pos);
        pos = GetScreenPosOnXZPlane(new Vector3(Screen.width, 0, 0));
        lines[2].SetPosition(1, pos);

        // Left
        pos = GetScreenPosOnXZPlane(new Vector3(0, 0, 0));
        lines[3].SetPosition(0, pos);
        pos = GetScreenPosOnXZPlane(new Vector3(0, Screen.height, 0));
        lines[3].SetPosition(1, pos);
    }

    Vector3 GetMousePositionOnXZPlane()
    {
        float distance;
        Plane XZPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = minimapCamera.ScreenPointToRay(Input.mousePosition);
        if (XZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            //Just double check to ensure the y position is exactly zero
            hitPoint.y = 0;
            return hitPoint;
        }
        return Vector3.zero;
    }

    Vector3 GetScreenPosOnXZPlane(Vector3 screenPos)
    {
        float distance;
        Plane XZPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (XZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            hitPoint.y = 10;
            return hitPoint;
        }
        return Vector3.zero;
    }
}

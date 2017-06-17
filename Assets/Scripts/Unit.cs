using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    const float HEALTH_WIDTH = 0.7f;
    const float SPEED = 3.5f;
    //const float SPEED = 1;
    const float TURN_RATE = 12;
    const float FACING_ANGLE = 20;
    const float NEXT_PATH_DIST = 0.2f;
    const float ENDZONE_Z_POS = -9.9f;

    private float[] height = { 0.41f, 0, 0, 0, 0.5f, 0, 0 };
    private int[] rotationOffset = { 180, 90, 180, 180, 0, 90, 180 };
    private int currentHealth;
    private bool alive = true;
    private bool canMove = false;

    private int unitID;
    private int modelID;

    private Vector3 currentNode;
    private int nodeIndex;
    private Vector3[] path;
    private UnitManager unitManager;

    public Renderer materialRenderer;
    public Material[] tierMaterials;

    private float healthPercent = 1;
    private Quaternion canvasRotation;
    private Image healthImage;
    public GameObject canvas;

    void Awake()
    {
        healthImage = canvas.transform.GetChild(1).GetComponent<Image>();
    }

    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        healthPercent = currentHealth / (float)Info.Unit.health[unitID];
        Rect r = healthImage.rectTransform.rect;
        healthImage.rectTransform.sizeDelta = new Vector2(healthPercent * HEALTH_WIDTH, r.height);
        Color c = new Color(1 - healthPercent, healthPercent, 0);
        healthImage.color = c;

        if (alive)
        {
            if (currentHealth <= 0)
            {
                alive = false;
                unitManager.RemoveUnit(this);
                unitManager.GetComponent<HUD>().Bounty(Info.Unit.bounty[unitID], transform.position);
                GameObject.Destroy(gameObject);
            }
        }
    }

    public void Setup(int unitID, UnitManager unitManager, Vector3[] path, Quaternion canvasRotation)
    {
        this.unitID = unitID;
        modelID = unitID == 24 ? 6 : unitID % 6;

        currentHealth = Info.Unit.health[unitID];
        healthImage.color = Color.green;

        // Set tier colour if not cat
        if (modelID < 6)
        {
            Material[] unitMaterials = materialRenderer.materials;
            unitMaterials[unitMaterials.Length - 1] = tierMaterials[Mathf.FloorToInt(unitID / 6f)];
            materialRenderer.materials = unitMaterials;
        }

        this.unitManager = unitManager;
        this.canvasRotation = canvasRotation;
 
        // Set initial path
        this.path = path;
        currentNode = path[nodeIndex];
        currentNode.y = height[modelID];
    }
    
    void Update()
    {
        if (canMove)
            Move();

        CheckIfReachedEnd();
        canvas.transform.rotation = canvasRotation;
    }

    void Move()
    {
        Vector3 line = (transform.position - currentNode).normalized;
        float angleDeg = (Mathf.Atan2(line.z, -line.x) * Mathf.Rad2Deg) + rotationOffset[modelID];
        Quaternion angle = Quaternion.Euler(0, angleDeg, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, angle, TURN_RATE * Time.deltaTime);

        // Only move if facing target
        if (Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.y, angle.eulerAngles.y)) < FACING_ANGLE)
            transform.position = Vector3.MoveTowards(transform.position, currentNode, SPEED * Time.deltaTime);

        if ((Mathf.Abs(transform.position.x - currentNode.x) < NEXT_PATH_DIST) && (Mathf.Abs(transform.position.z - currentNode.z) < NEXT_PATH_DIST))
        {
            if (nodeIndex < path.Length - 1)
            {
                nodeIndex++;
                currentNode = path[nodeIndex];
                currentNode.y = height[modelID];
            }
        }
    }

    void CheckIfReachedEnd()
    {
        if (alive && transform.position.z < ENDZONE_Z_POS)
        {
            alive = false;
            unitManager.RemoveUnit(this);
            unitManager.GetComponent<HUD>().LifeLost();
            SoundFX.player.LifeLost();
            GameObject.Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (unitManager.drawPaths)
            for (int i = 0; i < path.Length - 1; i++)
                Gizmos.DrawLine(path[i], path[i + 1]);
    }

    public void PathReceived(Vector3[] path)
    {
        if (path.Length == 0)
            unitManager.GetPath(this);
        else
        {
            this.path = path;
            nodeIndex = 0;
            for (int i = 0; i < path.Length; i++)
                if ((path[i] - transform.position).sqrMagnitude < Grid.nodeRadiusSquared)
                {
                    nodeIndex = i + 1;
                    break;
                }

            currentNode = path[nodeIndex];
            currentNode.y = height[modelID];
        }
    }

    public void AbleToMove()
    {
        canMove = true;
        Animator anim = GetComponent<Animator>();
        if (anim != null && modelID != 4)
            anim.SetTrigger("Move");
    }
}

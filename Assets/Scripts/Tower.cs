using UnityEngine;

public class Tower : MonoBehaviour
{
    private int towerID;
    private int damage;
    private float cooldown;
    private float range;
    private float lastFireTime;
    private LayerMask unitMask;
    private Vector2 topLeftNode;
    private GameObject projectilePrefab;
    private Material projectileMaterial;
    private float rangeSqr;
    private GameObject targetUnit;

    public void Setup(int towerID, LayerMask unitMask, Vector2 topLeftNode, GameObject projectilePrefab)
    {
        damage = Info.towers[towerID].damage;
        cooldown = Info.towers[towerID].cooldown;
        range = Info.towers[towerID].range;
        rangeSqr = range * range;
        projectileMaterial = Info.towers[towerID].projectileMaterial;
        this.unitMask = unitMask;
        this.towerID = towerID;
        this.topLeftNode = topLeftNode;
        this.projectilePrefab = projectilePrefab;
    }

    void Update()
	{
        if (Time.time - lastFireTime >= cooldown)
        {
            if (targetUnit == null || (targetUnit.transform.position - transform.position).sqrMagnitude > rangeSqr)
            {
                Collider[] units = Physics.OverlapSphere(transform.position, range, unitMask);
                if (units.Length > 0)
                {
                    targetUnit = units[0].gameObject;
                    Fire();
                }
                else
                    targetUnit = null;
            }
            else
                Fire();
        }
	}

    void Fire()
    {
        GameObject projectile = ObjectPooler.instance.GetObject(projectilePrefab);
        projectile.GetComponent<Projectile>().Setup(targetUnit.gameObject, damage);
        projectile.transform.position = transform.position;
        projectile.GetComponent<Renderer>().material = projectileMaterial;
        lastFireTime = Time.time;
    }

    public int TowerID
    {
        get { return towerID; }
    }

    public Vector2 TopLeftNode
    {
        get { return topLeftNode; }
    }
}

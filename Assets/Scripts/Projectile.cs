using UnityEngine;

public class Projectile : MonoBehaviour
{
    private const float SPEED = 15;
    private GameObject target;
    private int damage;

    public void Setup(GameObject target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

	void Update()
	{
        if (!target)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 dir = target.transform.position - transform.position;
        transform.Translate(dir.normalized * SPEED * Time.deltaTime);
        if (dir.sqrMagnitude < 0.1f)
        {
            target.GetComponent<Unit>().ApplyDamage(damage);
            gameObject.SetActive(false);
        }
    }
}

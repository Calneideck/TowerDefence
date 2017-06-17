using UnityEngine;

public class Bounty : MonoBehaviour {
	
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5), 20, Random.Range(-5, 5)));
    }
    
    void Update()
    {
        if (transform.position.y < -1)
            GameObject.Destroy(gameObject);
    }
}

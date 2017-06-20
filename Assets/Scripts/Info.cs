using UnityEngine;

public class Info : MonoBehaviour
{
    public static Tower[] towers;
    public Tower[] towerData;

    void Start()
    {
        towers = towerData;
    }

    [System.Serializable]
    public struct Tower
    {
        public string name;
        public int cost;
        public float height;
        public int damage;
        public float cooldown;
        public float range;
        public Material projectileMaterial;
    }

    public static class Unit
    {
        public static int[] health = { 18,   70,   150,  200,  290,  460,
                                       700,  890,  1000, 1150, 1300, 1600,
                                       2200, 2500, 2900, 3300, 3750, 4200,
                                       5000, 5500, 6200, 8000, 10000, 18000, 250000 };

        public static int[] bounty = { 1,   2,   4,   6,   9,    14,
                                       20,  28,  34,  44,  54,   64,
                                       70,  82,  98,  114, 180,  256,
                                       320, 422, 556, 830, 1388, 1980, 5000 };
    }
}
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f; // Time before the projectile is destroyed

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Destroy the projectile after a certain time
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Optional: Add effects or damage logic here
        Destroy(gameObject); // Destroy the projectile on collision
    }
}

using UnityEngine;

public class AdaptiveEnemyAI : MonoBehaviour
{
    // References
    private Transform player;

    // Layers
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    // Health
    [SerializeField] private float health = 100f;

    // Patroling Variables
    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f; // Patrol radius
    private Vector3 patrolTarget; // Current patrol target
    private bool patrolTargetSet = false; // Whether the patrol target is set
    [SerializeField] private float patrolSpeed = 2f; // Speed during patrol

    // Combat Variables
    [Header("Combat Settings")]
    [SerializeField] private float attackIntervalMin = 1.5f; // Minimum time between random attacks
    [SerializeField] private float attackIntervalMax = 3.5f; // Maximum time between random attacks
    [SerializeField] private GameObject projectilePrefab; // Projectile object
    [SerializeField] private float projectileSpeed = 20f; // Speed of the projectile

    // Detection Variables
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 15f; // Range within which the enemy detects the player
    [SerializeField] private float attackRange = 5f; // Range within which the enemy can attack
    private bool isPlayerVisible = false; // Whether the player is visible
    private bool isPlayerInRange = false; // Whether the player is in range

    private void Start()
    {
        // Log to indicate initialization
        Debug.Log("Enemy AI initialized and ready. Random shooting enabled.");
        
        // Start random shooting
        ScheduleRandomShoot();
    }

    private void Update()
    {
        // If there's no player, patrol
        if (player == null)
        {
            Patrol();
            return;
        }

        // Check player's proximity and visibility
        CheckPlayerProximity();

        // Decide behavior based on player's state
        if (!isPlayerVisible && !isPlayerInRange)
            Patrol(); // If the player is not visible or in range, patrol
        else if (isPlayerVisible && !isPlayerInRange)
            ChasePlayer(); // If the player is visible but not in range, chase
        else if (isPlayerInRange && isPlayerVisible)
            AttackPlayer(); // If the player is both visible and in range, attack
    }

    private void CheckPlayerProximity()
    {
        if (player == null) return; // Skip if player doesn't exist

        // Check if the player is within detection range
        isPlayerVisible = Physics.CheckSphere(transform.position, detectionRange, playerLayer);

        // Check if the player is within attack range
        isPlayerInRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);
    }

    private void Patrol()
    {
        // If no patrol target is set, find a new target
        if (!patrolTargetSet)
            SetRandomPatrolTarget();

        // Move towards the patrol target
        transform.position = Vector3.MoveTowards(transform.position, patrolTarget, patrolSpeed * Time.deltaTime);

        // Check if the enemy has reached the patrol target
        if (Vector3.Distance(transform.position, patrolTarget) < 1f)
            patrolTargetSet = false; // Reset patrol target if reached
    }

    private void SetRandomPatrolTarget()
    {
        // Generate random offsets within the patrol radius
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        // Calculate the new patrol target position
        patrolTarget = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Ensure the patrol target is on the ground
        if (Physics.Raycast(patrolTarget, Vector3.down, 2f, groundLayer))
            patrolTargetSet = true;
    }

    private void ChasePlayer()
    {
        if (player == null) return; // Skip if player doesn't exist

        // Move towards the player's current position
        transform.position = Vector3.MoveTowards(transform.position, player.position, patrolSpeed * Time.deltaTime);
    }

    private void AttackPlayer()
    {
        if (player == null) return; // Skip if player doesn't exist

        // Stop moving while attacking
        transform.LookAt(player);

        // Shoot a projectile towards the player
        ShootProjectile(player.position);
    }

    private void ShootProjectile(Vector3 targetPosition)
    {
        // Spawn a projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);

        // Calculate direction and apply velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * projectileSpeed;
    }

    private void ShootRandomProjectile()
    {
        // Random direction for shooting
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(0.2f, 1f), Random.Range(-1f, 1f)).normalized;

        // Spawn a projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position + randomDirection * 1.5f, Quaternion.identity);

        // Apply velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = randomDirection * projectileSpeed;

        // Schedule the next random shot
        ScheduleRandomShoot();
    }

    private void ScheduleRandomShoot()
    {
        // Schedule the next random projectile shot
        float randomInterval = Random.Range(attackIntervalMin, attackIntervalMax);
        Invoke(nameof(ShootRandomProjectile), randomInterval);
    }

    public void TakeDamage(float damage)
    {
        // Decrease health when taking damage
        health -= damage;

        // Check if health is depleted
        if (health <= 0)
            Die();
    }

    private void Die()
    {
        // Destroy the enemy object
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection range in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualize attack range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

using IndieMarc.Platformer;
using UnityEngine;

public class Goblin : MonoBehaviour
{
    [Header("Параметры")]
    public float detectionRange = 15f;
    public float attackRange = 1.8f;
    public float chaseSpeed = 3f;
    public LayerMask obstacleMask;

    [Header("Патруль")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;
    public float waypointReachDistance = 1.2f;  

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D mainCollider;

    private bool isDead = false;
    private bool isAttacking = false;
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;
    private int currentWaypointIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        mainCollider = GetComponent<Collider2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null) Debug.LogError("Player not found! Set tag 'Player'.");
        if (waypoints.Length == 0) Debug.LogWarning("No waypoints.");
        else Debug.Log($"Waypoints: {waypoints.Length}. First: {waypoints[0].position}");

    }

    void Update()
    {
        if (isDead) return;

        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 dir = (player.position - transform.position).normalized;

        bool canSeePlayer = true;


        if (distance <= detectionRange && canSeePlayer)
        {
            if (distance <= attackRange && !isAttacking)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
        else
        {
            if (currentState != State.Attack)
                currentState = State.Patrol;
        }

        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
        }
    }

    private void HandlePatrol()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        float dist = Vector2.Distance(transform.position, target.position);

        if (dist <= waypointReachDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        MoveTowards(target.position, patrolSpeed);
    }

    private void HandleChase()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    private void HandleAttack()
    {
        if (isAttacking) return;
        isAttacking = true;
        rb.velocity = Vector2.zero;
        anim.SetBool("ismoving", false);
        anim.SetTrigger("attack");
        Invoke(nameof(PerformAttack), 0.35f);
    }

    private void PerformAttack()
    {
        if (!isDead && player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
            KillPlayer();
        isAttacking = false;
        if (currentState == State.Attack) currentState = State.Patrol;
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        int dir = (target.x > transform.position.x) ? 1 : -1;

        rb.velocity = new Vector2(dir * speed, rb.velocity.y);

        anim.SetBool("ismoving", true);
        transform.localScale = new Vector3(dir, 1, 1);
    }

    private void KillPlayer()
    {
        if (isDead) return;
        Debug.Log("Игрок убит!");

        PlayerCharacter playerController = player.GetComponent<PlayerCharacter>();
        if (playerController != null) playerController.enabled = false;

        Invoke(nameof(CallGameOver), 0.25f);
    }

    private void CallGameOver()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.GameOver();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 contact = collision.GetContact(0).point;
            float top = transform.position.y + mainCollider.bounds.size.y / 2;
            if (contact.y > top - 0.1f && collision.relativeVelocity.y < -0.5f)
                Die();
            else
                KillPlayer();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("die");
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        mainCollider.enabled = false;
        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var wp in waypoints)
                if (wp != null) Gizmos.DrawWireSphere(wp.position, 0.2f);
        }
    }
}
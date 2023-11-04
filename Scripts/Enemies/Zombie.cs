using System.Collections;
using System.Collections.Generic;
using cowsins;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : EnemyHealth
{
    public float speed = 1.5f;

    public NavMeshAgent agent;

    public LayerMask whatIsGround, whatIsPlayer;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    public int damage;

    private RagdollEnabler RagdollEnabler;


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    public override void Update()
    {   
        //Handle UI 
        if (healthSlider != null) healthSlider.value = Mathf.Lerp(healthSlider.value, health,Time.deltaTime * 6);
        if (shieldSlider != null) shieldSlider.value = Mathf.Lerp(shieldSlider.value, shield, Time.deltaTime * 4); 

        // Manage health
        if (health <= 0) 
        {
            Die();
        }
        else
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInAttackRange) Patroling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInAttackRange && playerInSightRange) AttackPlayer();
            
        }

        //AI
        //if (health > 0){
            //RotateToTarget();
            //transform.Translate(Vector3.forward * Time.deltaTime * speed);
        //}
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            GetComponent<Animator>().Play("Z_Attack");
            player.GetComponent<PlayerStats>().Damage(damage);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    public override void Die()
    {   
        agent.SetDestination(transform.position);
        events.OnDeath.Invoke();
        speed = 0;

        if(shieldSlider != null)shieldSlider.gameObject.SetActive(false);
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        //UIEvents.onEnemyKilled.Invoke(name); 

        if (transform.GetComponent<CompassElement>() != null) transform.GetComponent<CompassElement>().Remove(); 

        GetComponent<Animator>().Play("Z_FallingBack");
        Destroy(this.gameObject, 3f);
    }

    public void RotateToTarget()
    {
        //transform.LookAt(player.transform);
        Vector3 direction = player.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = rotation;
    }
}

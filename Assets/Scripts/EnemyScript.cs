using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class EnemyScript : MonoBehaviour, IEnemy
{
    public bool selected;
    NavMeshAgent agent;


    private int randomSpot;

    private Animator anim;
    private float rotationSpeed = 280f;
    private bool playerSeen;
    public static bool playerCaught;
    private bool distracted;
    public float distractedTime = 10f;
    private Vector2 movementDirection;

    private GameObject player;
    private GameObject glowStick;


    public Transform[] points;
    private int destPoint = 0;
    
    
    NavMeshHit hit;
    private RaycastHit2D hitt;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        playerSeen = false;
        playerCaught = false;


        player = GameObject.FindWithTag("Player");


        anim = GetComponent<Animator>();
        GotoNextPoint();
    }

    void Update()
    {
        
        
        if (selected)
        {
            //NavMeshHit hit;
            //Debug.Log(agent.Raycast(player.transform.position, out hit));
        }

        //Debug.DrawLine(transform.position, player.transform.position, Color.red);

        movementDirection = agent.steeringTarget - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, movementDirection);

        //rotate enemy & start srpite animation if enemy is moving
        if (movementDirection != Vector2.zero)
        {
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            anim.enabled = true;
        }
        else
        {
            anim.enabled = false;
        }

        if (!agent.pathPending && !playerSeen && !distracted && agent.remainingDistance < 0.5f)
            GotoNextPoint();

        //print("distracted: "+distracted.ToString()+" | seen: "+playerSeen.ToString()+" | caught: "+playerCaught.ToString());


        if (playerSeen && !distracted)
        {
            agent.SetDestination(player.transform.position);

            
            if (Vector2.Distance(player.transform.position, transform.position) < 1.4f)
            {
                Caught();
            }

            if (Vector2.Distance(player.transform.position, transform.position) > 24f)
            {
                Debug.Log("too far away");
                playerSeen = false;
                agent.speed = 4.5f;
                GotoNextPoint();
            }
        }

    }


    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
    }


    private IEnumerator Distraction()
    {
        
        
        distracted = true;
        agent.SetDestination(glowStick.transform.position);
        yield return new WaitForSeconds(distractedTime);

        Destroy(glowStick);

        distracted = false;
        GotoNextPoint();
    }


    public void onTrigger()
    {
        hitt = Physics2D.Raycast(transform.position, player.transform.position);
        NavMeshHit hit;

        if (agent.Raycast(player.transform.position, out hit))
        {
            if (hitt != null && hitt.collider.tag == "Enenmy")
            {
                return;
            }
        }


        // Target is "visible" from our position.
        playerSeen = true;
        agent.speed = 6.8f;
        
        //Debug.Log(hitt.collider.name+" - "+ hitt.transform.tag+" at "+hitt.point+" (distance: "+hitt.distance+")");

        Debug.Log("Player has been seen!");
        
    }

    public void onTriggerGlowStick(GameObject glowStick)
    {
        hitt = Physics2D.Raycast(transform.position, player.transform.position);
        NavMeshHit hit;
        
        this.glowStick = glowStick;


        if (agent.Raycast(player.transform.position, out hit))
        {
            if (hitt != null && hitt.collider.tag == "Enenmy")
            {
                return;
            }
        }

        Debug.Log(hit);
        // Target is "visible" from our position.
        StartCoroutine(Distraction());

        Debug.Log("Glowstick has been seen!");
    }


    public void Caught()
    {
        playerCaught = true;
        //Debug.Log("Player Caught");
    }
}
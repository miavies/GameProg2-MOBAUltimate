using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class HeroCombat : MonoBehaviour
{
    public enum HeroActionType { Attack, Support, Debilitate, Ultimate };
    public HeroActionType heroActionType;

    public GameObject targeted;
    public float attackRange;
    public float rotateSpeedforAttack;

    private Movement moveScript;
    private Stats statsScript;
    private Animator anim;

    public bool basicAtkIdle = false;
    public bool isHeroAlive;
    public bool performAttack = true;

    //Ultimate
    public GameObject[] alliesNearPlayer;
    public LineRenderer[] ultimateLineRenderers;
    public Abilities abilities;
    public float baseSelfHeal;
    public float baseAllyInitalHeal;
    public float baseAllyHeal;
    public float ultimateRange;
    public float playerUltimateEffectDuration;
    public float playerUltimateEffectTimer;
    public float allyUltimateEffectDuration;
    public float allyUltimateEffectTimer;
    public bool ultimateEffect = true;

    void Start()
    {
        moveScript = GetComponent<Movement>();
        statsScript = GetComponent<Stats>();
        anim = GetComponent<Animator>();
        abilities = GetComponent<Abilities>();

        playerUltimateEffectTimer=0f;
        allyUltimateEffectTimer = 0f;

        foreach (LineRenderer lineRenderer in ultimateLineRenderers)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.enabled = false; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Basic Attack
        if (targeted != null)
        {
            if (Vector3.Distance(gameObject.transform.position, targeted.transform.position) > attackRange)
            {
                moveScript.agent.SetDestination(targeted.transform.position);
                moveScript.agent.stoppingDistance = attackRange;

                Quaternion rotationToLookAt = Quaternion.LookRotation(targeted.transform.position - transform.position);
                float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y, ref moveScript.rotateVelocity, rotateSpeedforAttack * Time.deltaTime * 5);

                transform.eulerAngles = new Vector3(0, rotationY, 0);
            }
            else
            {
                if (heroActionType == HeroActionType.Attack)
                {
                    if (performAttack)
                    {
                        StartCoroutine(AttackInterval());
                    }
                }
            }
        }

        // Ultimate Action
        if (heroActionType == HeroActionType.Ultimate)
        {
            // Heal the player over 8 seconds
            if (playerUltimateEffectTimer < playerUltimateEffectDuration)
            {
                playerUltimateEffectTimer += Time.deltaTime;

                statsScript.health += (baseSelfHeal + (statsScript.magicPower * 2.1f)) * Time.deltaTime;
                if (statsScript.health > statsScript.maxHealth)
                {
                    statsScript.health = statsScript.maxHealth;
                }
            }
            // Reset playerEffectTimer when 8 seconds pass to allow for another ultimate activation later
            else
            {
                heroActionType = HeroActionType.Attack;
                playerUltimateEffectTimer = 0.0f;
            }

            // Heal allies over 3 seconds
            
            if (ultimateEffect)
            {
                if (allyUltimateEffectTimer < allyUltimateEffectDuration)
                {
                    allyUltimateEffectTimer += Time.deltaTime;

                    // Find allies around the player
                    alliesNearPlayer = GameObject.FindGameObjectsWithTag("Ally");

                    int lineIndex = 0;

                    foreach (GameObject ally in alliesNearPlayer)
                    {
                        if (lineIndex >= ultimateLineRenderers.Length)
                            break;

                        if (Vector3.Distance(transform.position, ally.transform.position) <= ultimateRange)
                        {
                            // Render Line
                            ultimateLineRenderers[lineIndex].enabled = true;
                            ultimateLineRenderers[lineIndex].SetPosition(0, transform.position);
                            ultimateLineRenderers[lineIndex].SetPosition(1, ally.transform.position);

                            // Heal Ally
                            ally.GetComponent<Stats>().health += (baseAllyHeal + (statsScript.magicPower * 2.1f)) * Time.deltaTime;
                            if (ally.GetComponent<Stats>().health > ally.GetComponent<Stats>().maxHealth)
                            {
                                ally.GetComponent<Stats>().health = ally.GetComponent<Stats>().maxHealth;
                            }

                            lineIndex++;
                        }
                    }

                    // Disable unused line renderers
                    for (int i = lineIndex; i < ultimateLineRenderers.Length; i++)
                    {
                        ultimateLineRenderers[i].enabled = false;
                    }
                }
                else
                {
                    // End ally healing and reset timers when the ally effect duration is over
                    ultimateEffect = false;
                    allyUltimateEffectTimer = 0.0f;

                    // Disable all line renderers
                    foreach (LineRenderer lineRenderer in ultimateLineRenderers)
                    {
                        lineRenderer.enabled = false;
                    }
                }
            }
        }
    }

    //Basic Attack
    IEnumerator AttackInterval()
    {
        performAttack = false;
        anim.SetBool("Attack", true);

        yield return new WaitForSeconds(statsScript.attackTime);


        if (targeted == null)
        {
            anim.SetBool("Attack", false);
            performAttack = true;
        }
    }

    public void Attack()
    {
        if (targeted != null)
        {
            if (targeted.GetComponent<Targetable>().TargetType == Targetable.TargetableType.Enemy)
            {
                targeted.GetComponent<Stats>().health -= statsScript.attackDamage;
            }
        }

        performAttack = true;
    }

    //Initial Ultimate Heal
    public void InitialUltimateHeal()
    {
        alliesNearPlayer = GameObject.FindGameObjectsWithTag("Ally");

        int lineIndex = 0;

        foreach (GameObject ally in alliesNearPlayer)
        {
            if (lineIndex >= ultimateLineRenderers.Length)
                break;

            if (Vector3.Distance(transform.position, ally.transform.position) <= ultimateRange)
            {
                ally.GetComponent<Stats>().health += baseAllyInitalHeal;
                lineIndex++;
            }
        }
    }
}

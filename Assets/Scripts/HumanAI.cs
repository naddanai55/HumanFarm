using UnityEngine;
using UnityEngine.AI;

public class HumanAI : MonoBehaviour
{
    public enum HumanState
    {
        Wander,
        SeekFood,
        Eating,
        Pooping,
        ReadyToHarvest,
        Dead
    }

    [Header("State")]
    public HumanState currentState = HumanState.Wander;

    [Header("Human Data (Scriptable Object)")]
    public HumanSO humanData;

    [Header("Current Stats (Live)")]
    public float currentHunger;
    public float currentHappiness;
    public float currentGrowth;
    public float currentBowelLevel;
    public float currentHappinessRate;
    private NavMeshAgent agent;
    public GameObject poopPrefab;
    private float poopingTimer = 0f;
    private float timeToPoop = 2f;
    private Transform targetFood;
    private float oneSecondTimer = 0f;
    private float eatingTimer = 0f;
    private FoodItem currentFood;
    private Animator animator;
    private string foodTag = "Food";
    private string poopingAnimString = "IsPooping";
    private string eatingAnimString = "IsEating";
    public GameObject brainPrefab;
    [SerializeField] float brainHeightOffset = -1f;
    public ParticleSystem bloodParticle;
    private string deadAnimString = "IsDead";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        currentHunger = humanData.maxHunger;
        currentHappiness = humanData.maxHappiness;
        currentGrowth = 0f;
        currentBowelLevel = 0f;

        agent.speed = humanData.moveSpeed;
        ChangeState(HumanState.Wander);
    }

    void Update()
    {
        if (currentState == HumanState.ReadyToHarvest || currentState == HumanState.Dead)
        {
            return;
        }

        oneSecondTimer += Time.deltaTime;

        if (oneSecondTimer >= 1f)
        {
            oneSecondTimer = 0f;
            currentHunger -= humanData.hungerDepletionRate;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, humanData.maxHappiness);

            float happinessMultiplier = currentHappiness / humanData.maxHappiness;
            currentHappinessRate = happinessMultiplier;
            currentGrowth += humanData.maxBrainGrowthRate * happinessMultiplier;

            CheckStateTransitions();
        }

        switch (currentState)
        {
            case HumanState.Wander:
                HandleWander();
                break;
            case HumanState.SeekFood:
                HandleSeekFood();
                break;
            case HumanState.Eating:
                HandleEating(currentFood);
                break;
            case HumanState.Pooping:
                HandlePooping();
                break;
        }
    }

    void CheckStateTransitions()
    {
        if (currentHunger <= humanData.starveThreshold)
        {
            ChangeState(HumanState.Dead);
        }
        else if (currentGrowth >= humanData.maxGrowth)
        {
            ChangeState(HumanState.ReadyToHarvest);
        }
        else if (currentBowelLevel >= humanData.bowelCapacity && currentState == HumanState.Wander)
        {
            ChangeState(HumanState.Pooping);
        }
        else if (currentHunger < humanData.seekFoodThreshold && currentState == HumanState.Wander)
        {
            ChangeState(HumanState.SeekFood);
        }
    }

    void ChangeState(HumanState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case HumanState.Wander:
                agent.isStopped = false;
                SetNewWanderDestination();
                break;

            case HumanState.SeekFood:
                targetFood = null;
                break;

            case HumanState.Eating:
                agent.isStopped = true;
                break;

            case HumanState.Pooping:
                agent.isStopped = true;
                StartPooping();
                break;

            case HumanState.ReadyToHarvest:
                break;

            case HumanState.Dead:
                agent.isStopped = true;
                break;
        }
    }

    void HandleWander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewWanderDestination();
        }
    }
    void SetNewWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void HandleSeekFood()
    {
        if (targetFood != null && targetFood.gameObject == null)
        {
            targetFood = null;
        }

        if (targetFood == null)
        {
            Transform closest = GetClosestFood();
            if (closest != null)
            {
                targetFood = closest;
                agent.SetDestination(targetFood.position);
                agent.isStopped = false;
            }
            else
            {
                ChangeState(HumanState.Wander);
                return;
            }
        }
    }

    private Transform GetClosestFood()
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag(foodTag);
        Transform closest = null;
        float bestDist = Mathf.Infinity;

        foreach (GameObject go in all)
        {
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = go.transform;
            }
        }

        return closest;
    }

    void HandleEating(FoodItem food)
    {
        if (targetFood == null)
        {
            ChangeState(HumanState.Wander);
            return;
        }

        animator.SetBool(eatingAnimString, true);

        eatingTimer += Time.deltaTime;
        if (eatingTimer >= food.GetEatingTime())
        {
            food.Consume(this);
            targetFood = null;
            currentFood = null;
            animator.SetBool(eatingAnimString, false);
            ChangeState(HumanState.Wander);
        }
    }

    void StartPooping()
    {
        poopingTimer = 0f;
        if (animator != null) 
        {
            animator.SetBool(poopingAnimString, true);
        }
    }

    void HandlePooping()
    {
        poopingTimer += Time.deltaTime;

        if (poopingTimer >= timeToPoop)
        {
            Vector3 spawnPos = transform.position - (transform.forward * 0.5f);
            Instantiate(poopPrefab, spawnPos, Quaternion.identity);

            currentBowelLevel = 0f;

            if (animator != null)
            {
                animator.SetBool(poopingAnimString, false);
            }

            ChangeState(HumanState.Wander);
        }
    }

    public void ExecuteHuman()
    {
        if (bloodParticle != null)
        {
            bloodParticle.transform.parent = null; 
            bloodParticle.Play();
            Destroy(bloodParticle.gameObject, 2f); 
        }

        if (currentState == HumanState.ReadyToHarvest)
        {
            int finalBrainValue = Mathf.RoundToInt(humanData.baseValueZCoins * currentHappinessRate);

            Debug.Log("Harvested a ripe brain worth: $" + finalBrainValue);

            if (brainPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * brainHeightOffset;
                GameObject droppedBrain = Instantiate(brainPrefab, spawnPosition, Quaternion.identity);
                
                BrainItem brainScript = droppedBrain.GetComponent<BrainItem>();
                
                if (brainScript != null)
                {
                    string nameOfBrain = humanData.HumanName + "Brain"; 
                    brainScript.SetupBrain(nameOfBrain, finalBrainValue);
                }
            }
        }
        else
        {
            Debug.Log("Executed early! The human died, but the brain was ruined. No drop!");
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState != HumanState.SeekFood)
            return;

        if (other.CompareTag("Food"))
        {
            currentFood = other.GetComponent<FoodItem>();
            eatingTimer = 0f;
            ChangeState(HumanState.Eating);
        }
    }
}
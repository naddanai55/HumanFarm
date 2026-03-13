using UnityEngine;
using UnityEngine.AI;

public class HumanAI : MonoBehaviour
{
    public enum HumanState
    {
        Wander,
        SeekFood,
        Eating,
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

    // [Header("References")]
    // [SerializeField] GameObject headModel;

    private NavMeshAgent agent;
    private Transform targetFood;
    private string foodTag = "Food";
    private float oneSecondTimer = 0f;
    private float eatingTimer = 0f;
    private FoodItem currentFood;
    private Animator animator;
    private string eatingAnimString = "IsEating";


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // 1. Initialize stats from the ScriptableObject
        currentHunger = humanData.maxHunger;
        currentHappiness = humanData.maxHappiness;
        currentGrowth = 0f;

        // 2. Set Movement Speed from SO
        agent.speed = humanData.moveSpeed;

        ChangeState(HumanState.Wander);
    }

    void Update()
    {
        // 1. If dead or fully grown, do absolutely nothing.
        if (currentState == HumanState.ReadyToHarvest || currentState == HumanState.Dead)
        {
            return;
        }

        // --- ONE SECOND TICK LOGIC (Updates Stats) ---
        oneSecondTimer += Time.deltaTime;

        if (oneSecondTimer >= 1f) // Has 1 second passed?
        {
            oneSecondTimer = 0f; // Reset the timer

            // 2. Update Stats! 
            // Notice we REMOVED Time.deltaTime here, because this only runs once per second!
            currentHunger -= humanData.hungerDepletionRate;

            currentHappiness -= humanData.happinessDepletionRate;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, humanData.maxHappiness);

            // 3. Growth depends on Happiness! 
            float happinessMultiplier = currentHappiness / humanData.maxHappiness;
            currentGrowth += humanData.maxBrainGrowthRate * happinessMultiplier;

            // 4. Check if we need to change state based on the new stats
            CheckStateTransitions();
        }

        // 5. Run the current state behavior
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
        }
    }

    // --- CLEAN TRANSITION SYSTEM ---

    void CheckStateTransitions()
    {
        // Use the SO thresholds!
        if (currentHunger <= humanData.starveThreshold)
        {
            ChangeState(HumanState.Dead);
        }
        else if (currentGrowth >= humanData.maxGrowth)
        {
            ChangeState(HumanState.ReadyToHarvest);
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

            case HumanState.ReadyToHarvest:
                agent.isStopped = true;
                // headModel.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                break;

            case HumanState.Dead:
                agent.isStopped = true;
                // e.g., headModel.GetComponent<Renderer>().material.color = Color.gray;
                break;
        }
    }

    // --- STATE BEHAVIORS ---

    void HandleWander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewWanderDestination();
        }
    }

    void HandleSeekFood()
    {
        // if targetFood was destroyed while we were en route, forget it
        if (targetFood != null && targetFood.gameObject == null)
        {
            targetFood = null;
        }

        // if we don't yet have a food target, grab the closest one and head toward it
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

        // fallback in case we enter the collider without triggering OnTriggerEnter
        // if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        // {
        //     ChangeState(HumanState.Eating);
        // }
    }

    void HandleEating(FoodItem food)
    {
        if (targetFood == null)
        {
            ChangeState(HumanState.Wander);
            return;
        }

        animator.SetBool(eatingAnimString, true);

        // Count up the timer
        eatingTimer += Time.deltaTime;

        // Is the timer finished?
        if (eatingTimer >= food.GetEatingTime())
        {
            // 1. Get the FoodItem script

            // 2. Tell the food to consume itself and apply stats to "this" human!
            food.Consume(this);

            // clear the reference; the object may now be destroyed
            targetFood = null;
            currentFood = null;


            // 3. Go back to wandering
            animator.SetBool(eatingAnimString, false);
            ChangeState(HumanState.Wander);
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

    /// <summary>
    /// Finds the closest food object using the configured tag.  Returns null if none are found.
    /// </summary>
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
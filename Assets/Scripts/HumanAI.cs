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

    // public enum EmojiType 
    // { 
    //     None, 
    //     Happy, 
    //     Hungry, 
    //     Angry, 
    //     Brain 
    // }

    // [Header("3D Emoji Models")]
    // public GameObject happyEmoji;
    // public GameObject hungryEmoji;
    // public GameObject angryEmoji;
    // public GameObject brainEmoji;
    // private EmojiType currentEmoji = EmojiType.None;


    [Header("State")]
    public HumanState currentState = HumanState.Wander;

    [Header("Human Data (Scriptable Object)")]
    public HumanSO humanData;

    [Header("Current Stats (Live)")]
    public float currentHunger;
    public float currentHappiness;
    public float currentGrowth;
    public float currentBowelLevel;
    private NavMeshAgent agent;
    public GameObject poopPrefab;
    // public ParticleSystem poopParticle;
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

    [Header("Harvesting Effects")]
    public GameObject brainPrefab;      // The item that drops (or just a particle effect)
    public ParticleSystem bloodParticle; // The blood effect that plays on execution
    private string deadAnimString = "IsDead";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // 1. Initialize stats from the ScriptableObject
        currentHunger = humanData.maxHunger;
        currentHappiness = humanData.maxHappiness;
        currentGrowth = 0f;
        currentBowelLevel = 0f;

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

            // currentHappiness -= humanData.happinessDepletionRate;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, humanData.maxHappiness);

            // 3. Growth depends on Happiness! 
            float happinessMultiplier = currentHappiness / humanData.maxHappiness;
            currentGrowth += humanData.maxBrainGrowthRate * happinessMultiplier;

            // UpdateEmojiIndicator();

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
            case HumanState.Pooping:   // <-- RUN NEW BEHAVIOR
                HandlePooping();
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

            case HumanState.Pooping: // <-- NEW STATE SETUP
                StartPooping();
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

            currentBowelLevel += 50f;

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

    void StartPooping()
    {
        agent.isStopped = true;
        poopingTimer = 0f;

        if (animator != null) animator.SetBool(poopingAnimString, true);
        // if (poopParticle != null) poopParticle.Play();
    }

    void HandlePooping()
    {
        poopingTimer += Time.deltaTime;

        if (poopingTimer >= timeToPoop)
        {
            // 1. Spawn the poop slightly behind the human so they don't get stuck in it
            Vector3 spawnPos = transform.position - (transform.forward * 0.5f);
            Instantiate(poopPrefab, spawnPos, Quaternion.identity);

            // 2. Reset the bowel level
            currentBowelLevel = 0f;

            // 3. Stop the animation and go back to wandering
            if (animator != null) animator.SetBool(poopingAnimString, false);
            ChangeState(HumanState.Wander);
        }
    }

    public void ExecuteHuman()
    {
        // 1. Play the blood/death effect no matter what!
        if (bloodParticle != null)
        {
            bloodParticle.transform.parent = null; 
            bloodParticle.Play();
            Destroy(bloodParticle.gameObject, 2f); 
        }

        // 2. Did we wait until they were fully grown?
        if (currentState == HumanState.ReadyToHarvest)
        {
            // YES! Drop the Brain!
            float happinessMultiplier = currentHappiness / humanData.maxHappiness;
            int finalBrainValue = Mathf.RoundToInt(humanData.baseBrainValue * happinessMultiplier);

            Debug.Log("Harvested a ripe brain worth: $" + finalBrainValue);

            if (brainPrefab != null)
            {
                GameObject droppedBrain = Instantiate(brainPrefab, transform.position, Quaternion.identity);
                
                // 2. Get the script on the brain
                BrainItem brainScript = droppedBrain.GetComponent<BrainItem>();
                
                // 3. Tell the brain its name and calculated value!
                if (brainScript != null)
                {
                    // (Assuming you add 'typeName' to your HumanSO, like "Gym Bro Brain")
                    string nameOfBrain = humanData.HumanName + " Brain"; 
                    brainScript.SetupBrain(nameOfBrain, finalBrainValue);
                }
            }
        }
        else
        {
            // NO! Executed too early.
            Debug.Log("Executed early! The human died, but the brain was ruined. No drop!");
        }

        // 3. Destroy the Human body no matter what!
        Destroy(gameObject);
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

    // void UpdateEmojiIndicator()
    // {
    //     // PRIORITY 1: Ready to Harvest (Highest Priority)
    //     if (currentState == HumanState.ReadyToHarvest)
    //     {
    //         SetEmoji(EmojiType.Brain);
    //         return; // Stop checking!
    //     }

    //     // PRIORITY 2: Angry (Because they smelled poop!)
    //     // If Happiness drops below 50%, they show the angry face.
    //     if (currentHappiness < (humanData.maxHappiness * 0.5f))
    //     {
    //         SetEmoji(EmojiType.Angry);
    //         return;
    //     }

    //     // PRIORITY 3: Hungry
    //     if (currentState == HumanState.SeekFood)
    //     {
    //         SetEmoji(EmojiType.Hungry);
    //         return;
    //     }

    //     // PRIORITY 4: Happy (Default state if well-fed and clean!)
    //     if (currentState == HumanState.Eating)
    //     {
    //         SetEmoji(EmojiType.Happy);
    //         return;
    //     }
    // }

    // void SetEmoji(EmojiType newEmoji)
    // {
    //     // If we are already showing this emoji, do nothing to save performance!
    //     if (currentEmoji == newEmoji) return;

    //     currentEmoji = newEmoji;

    //     // 1. Turn ALL emojis off first
    //     if (happyEmoji != null) happyEmoji.SetActive(false);
    //     if (hungryEmoji != null) hungryEmoji.SetActive(false);
    //     if (angryEmoji != null) angryEmoji.SetActive(false);
    //     if (brainEmoji != null) brainEmoji.SetActive(false);

    //     // 2. Turn on the exact one we need
    //     switch (newEmoji)
    //     {
    //         case EmojiType.Happy:
    //             if (happyEmoji != null) happyEmoji.SetActive(true);
    //             break;
    //         case EmojiType.Hungry:
    //             if (hungryEmoji != null) hungryEmoji.SetActive(true);
    //             break;
    //         case EmojiType.Angry:
    //             if (angryEmoji != null) angryEmoji.SetActive(true);
    //             break;
    //         case EmojiType.Brain:
    //             if (brainEmoji != null) brainEmoji.SetActive(true);
    //             break;
    //     }
    // }
}
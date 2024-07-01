using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public enum AgentType { Player, Bot, TrainerBot }

public class AgentController : Agent
{
    public AgentType AgentType;

    public GameObject tube;
    public Puncture puncture;
    public CounterText counter;
    public ParticleSystem swimParticles;
    public Animator animator;
    public GameObject Avatar;
    public ParticleSystem waterSplash;
    public float MaxSpeed;
    public float LeakRate;
    public float DrownRate;
    public float DrownDistance;
    public float MaxScaleFactor;
    [Range(0,1)] public float RotationSpeed;

    [HideInInspector] public float currentAirCount;

    [Header("Training")]
    public Material WinMaterial;
    public Material FailMaterial;
    private GameObject[] Walls;

    private Rigidbody rb;
    private Joystick joystick;
    private Transform targetAirUnit;
    private Vector3 inputs;
    private Quaternion lookRotation;
    private float targetScaleFactor;
    private float diffInLookRotation;

    void Start()
    {
        joystick = GameObject.FindGameObjectWithTag("Joystick").GetComponentInChildren<FloatingJoystick>();
        Walls = GameObject.FindGameObjectsWithTag("Wall");
        rb = GetComponent<Rigidbody>();

        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        SetInitialAirCount();

        //For bots that are spawned mid round, Gamestate will be GAME allowing them to start moving around
        //For bots that are spawned at the beginning, Gamestate will be PRE-Game -> unharfmul
        //ToDo : Move to seperate function for spawned bots
        OnGameStateChanged(GameManager.Instance.GameState);
    }    

    void Update()
    {
        if (GameManager.Instance.GameState.Equals(GameState.Game))
        {
            if (AgentType.Equals(AgentType.Player))
            {
                if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Ended)))
                {
                    rb.angularVelocity = Vector3.zero;
                }
            }

            LeakAir(LeakRate);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GameState.Equals(GameState.Game) || GameManager.Instance.GameState.Equals(GameState.PostGame))
        {
            if (rb.velocity.magnitude > MaxSpeed)
            {
                rb.velocity = rb.velocity.normalized * MaxSpeed;                
            }

            if (inputs.magnitude > 0)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, RotationSpeed);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(GameManager.Instance.TrainingMode && collision.collider.CompareTag("Wall"))
        {
            AddReward(-0.01f);
            SetTrainingMaterial(false);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Bot") || collision.gameObject.CompareTag("Player"))
        {
            float otherAgentAirCount = collision.gameObject.GetComponent<AgentController>().currentAirCount;

            if (currentAirCount < otherAgentAirCount)
            {
                PopTube();
            }
            else if (currentAirCount > otherAgentAirCount)
            {
                FillUpAir(otherAgentAirCount/2);
            }
            //In case of a collision between Player and a bot of equal size, player gets advantage.
            else
            {
                if(AgentType.Equals(AgentType.Player))
                {
                    FillUpAir(otherAgentAirCount/2);
                }
                else
                {
                    PopTube();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Air"))
        {
            FillUpAir(other.GetComponent<AirUnit>().FillAmount);
            //used only for bots - no harm for players
            targetAirUnit = AirUnitsManager.Instance.GetRandomAirUnit().transform;
            
            if (GameManager.Instance.TrainingMode)
            {
                AddReward(1);
                SetTrainingMaterial(true);
                EndEpisode();
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        inputs = Vector3.zero;
        inputs.x = actions.ContinuousActions[0];
        inputs.z = actions.ContinuousActions[1];

        Debug.Log("Action : " + inputs.x + "," + inputs.z);

        #region Smooth Input Learning
        //This snippet rewards the AI for giving out smooth inputs
        //Otherwise it might give out opposing inputs and thereby rendering the bot as stationary or jittery in some cases

        lookRotation = Quaternion.LookRotation(inputs, Vector3.up);
        diffInLookRotation = Quaternion.Angle(lookRotation, transform.rotation);

        if (diffInLookRotation > 90)
            AddReward(-0.001f);
        else if (diffInLookRotation < 5 && Mathf.Approximately(inputs.magnitude, 0))
            AddReward(0.01f);

        #endregion
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (GameManager.Instance.GameState == GameState.Game || GameManager.Instance.GameState == GameState.PostGame)
        {
            if(targetAirUnit == null)
                targetAirUnit = AirUnitsManager.Instance.GetRandomAirUnit().transform;

            Vector3 targetDirection = targetAirUnit.position - transform.position;

            // Position of player, Space Size : 3
            sensor.AddObservation(transform.position);
            // Position of AirUnit, Space Size : 3
            sensor.AddObservation(targetAirUnit.position);
            // Direction to AirUnit, Space Size : 3
            sensor.AddObservation(targetDirection.normalized);
            // Distance to AirUnit, Space Size : 1
            sensor.AddObservation(targetDirection.magnitude);
            // Current AirUnits, Space Size : 1
            sensor.AddObservation(currentAirCount);
            // Player Speed, Space Size : 1
            sensor.AddObservation(rb.velocity.magnitude);
            // Player Direction, Space Size : 3
            sensor.AddObservation(rb.velocity.normalized);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> heuristicInputs = actionsOut.ContinuousActions;
        heuristicInputs[0] = joystick.Horizontal;
        heuristicInputs[1] = joystick.Vertical;

        Debug.Log("Heuristic : " + heuristicInputs[0] + "," + heuristicInputs[1]);
    }
    
    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin");

        if (GameManager.Instance.TrainingMode)
        {
            SetAir(100);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            inputs = Vector3.zero;
        }
    }

    private void SetInitialAirCount()
    {
        if (AgentType.Equals(AgentType.Player))
        {
            currentAirCount = 100;
        }
        else
        {
            currentAirCount = UnityEngine.Random.Range(95, 99.9f);
        }
    }

    private void SetTrainingMaterial(bool win)
    {
        if (AgentType.Equals(AgentType.TrainerBot))
        {
            foreach (GameObject wall in Walls)
            {
                if (win)
                    wall.GetComponent<MeshRenderer>().material = WinMaterial;
                else
                    wall.GetComponent<MeshRenderer>().material = FailMaterial;
            }
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state.Equals(GameState.Game))
        {
            puncture.enabled = true;
        }
    }

    private void SetAir(float amount)
    {
        currentAirCount = amount;
        UpdateTargetScale();
    }
    
    private void FillUpAir(float amount)
    {
        currentAirCount += amount;
        UpdateTargetScale();
    }

    private void LeakAir(float amount)
    {
        if(currentAirCount > 0)
        {
            currentAirCount -= amount;
            UpdateTargetScale();
        }
        else
        {
            PopTube();
        }        
    }

    private void UpdateTargetScale()
    {
        if(currentAirCount < 100)
        {
            targetScaleFactor = 0.5f + (currentAirCount / 200);
            tube.transform.localScale = Vector3.one * targetScaleFactor;
            transform.localScale = Vector3.one;
        }
        else
        {
            targetScaleFactor = Mathf.Clamp(currentAirCount / 100, 1, MaxScaleFactor);
            tube.transform.localScale = Vector3.one;
            transform.localScale = Vector3.one * targetScaleFactor;
        }

        counter.UpdateCount(currentAirCount);
    }

    private void PopTube()
    {
        if (GameManager.Instance.TrainingMode)
        {
            AddReward(-1);
            SetTrainingMaterial(false);
            EndEpisode();
        }
        else
        {
            tube.GetComponent<Collider>().enabled = false;
            tube.GetComponent<Renderer>().enabled = false;
            rb.velocity *= 0.25f;

            puncture.enabled = false;
            this.enabled = false;

            StartCoroutine(Drown(Avatar.transform.position - new Vector3(0, DrownDistance, 0)));
            animator.SetTrigger("Drown");
            waterSplash.Play();
            swimParticles.Stop();
            counter.DisableCounter();

            if (AgentType == AgentType.Player)
            {
                GameManager.Instance.ChangeGameState(GameState.PostGame);
                UIManager.Instance.ShowGameOverScreenFail();
            }
            else if (AgentType == AgentType.Bot)
            {
                BotsManager.Instance.SpawnBot(1);
                BotsManager.Instance.RemoveBotFromActiveList(transform.parent.gameObject);
            }
        }
    }

    /// <summary>
    /// Slow drowning Animation
    /// </summary>
    IEnumerator Drown(Vector3 target)
    {
        yield return new WaitForEndOfFrame();
        Avatar.transform.position = Vector3.MoveTowards(Avatar.transform.position, target, DrownRate);

        if(Vector3.Distance(Avatar.transform.position, target) > 0.1f)
        {
            StartCoroutine(Drown(target));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

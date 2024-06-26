using MLAgentsExamples;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using BodyPart = Unity.MLAgentsExamples.BodyPart;

public class WalkerAgent : Agent
{
    public float maximumWalkingSpeed = 999; //The max walk velocity magnitude an agent will be rewarded for
    Vector3 m_WalkDir; //Direction to the target
    Quaternion m_WalkDirLookRot; //Will hold the rotation to our target
    
    [Header("Target To Walk Towards")] [Space(10)]
    public float targetSpawnRadius; //The radius in which a target can be randomly spawned.

    public Transform target; //Target the agent will walk towards.
    public Transform ground; //Ground gameobject. The height will be used for target spawning
    public bool detectTargets; //Should this agent detect targets
    public bool respawnTargetWhenTouched; //Should the target respawn to a different position when touched

    [Header("Body Parts")] [Space(10)] public Transform hips;
    public Transform chest;
    public Transform spine;
    public Transform head;
    public Transform thighL;
    public Transform shinL;
    public Transform footL;
    public Transform thighR;
    public Transform shinR;
    public Transform footR;
    public Transform armL;
    public Transform forearmL;
    public Transform handL;
    public Transform armR;
    public Transform forearmR;
    public Transform handR;
    
    [Header("Orientation")] [Space(10)]
    //This will be used as a stable reference point for observations
    //Because ragdolls can move erratically, using a standalone reference point can significantly improve learning
    public GameObject orientationCube;

    JointDriveController m_JdController;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {

        UpdateOrientationCube();

        m_JdController = GetComponent<JointDriveController>();
        m_JdController.SetupBodyPart(hips);
        m_JdController.SetupBodyPart(chest);
        m_JdController.SetupBodyPart(spine);
        m_JdController.SetupBodyPart(head);
        m_JdController.SetupBodyPart(thighL);
        m_JdController.SetupBodyPart(shinL);
        m_JdController.SetupBodyPart(footL);
        m_JdController.SetupBodyPart(thighR);
        m_JdController.SetupBodyPart(shinR);
        m_JdController.SetupBodyPart(footR);
        m_JdController.SetupBodyPart(armL);
        m_JdController.SetupBodyPart(forearmL);
        m_JdController.SetupBodyPart(handL);
        m_JdController.SetupBodyPart(armR);
        m_JdController.SetupBodyPart(forearmR);
        m_JdController.SetupBodyPart(handR);

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
    {
        //GROUND CHECK
        sensor.AddObservation(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground

        //Get velocities in the context of our orientation cube's space
        //Note: You can get these velocities in world space as well but it may not train as well.
        sensor.AddObservation(orientationCube.transform.InverseTransformDirection(bp.rb.velocity));
        sensor.AddObservation(orientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));

        //Get position relative to hips in the context of our orientation cube's space
        sensor.AddObservation(orientationCube.transform.InverseTransformDirection(bp.rb.position - hips.position));

        if (bp.rb.transform != hips && bp.rb.transform != handL && bp.rb.transform != handR)
        {
            sensor.AddObservation(bp.rb.transform.localRotation);
            sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
        }
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Quaternion.FromToRotation(hips.forward, orientationCube.transform.forward));
        sensor.AddObservation(Quaternion.FromToRotation(head.forward, orientationCube.transform.forward));

        sensor.AddObservation(orientationCube.transform.InverseTransformPoint(target.position));

        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            CollectObservationBodyPart(bodyPart, sensor);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActionOut = actionBuffers.ContinuousActions;
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;

        bpDict[chest].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], continuousActionOut[++i]);
        bpDict[spine].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], continuousActionOut[++i]);

        bpDict[thighL].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], 0);
        bpDict[thighR].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], 0);
        bpDict[shinL].SetJointTargetRotation(continuousActionOut[++i], 0, 0);
        bpDict[shinR].SetJointTargetRotation(continuousActionOut[++i], 0, 0);
        bpDict[footR].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], continuousActionOut[++i]);
        bpDict[footL].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], continuousActionOut[++i]);

        bpDict[armL].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], 0);
        bpDict[armR].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], 0);
        bpDict[forearmL].SetJointTargetRotation(continuousActionOut[++i], 0, 0);
        bpDict[forearmR].SetJointTargetRotation(continuousActionOut[++i], 0, 0);
        bpDict[head].SetJointTargetRotation(continuousActionOut[++i], continuousActionOut[++i], 0);

        //update joint strength settings
        bpDict[chest].SetJointStrength(continuousActionOut[++i]);
        bpDict[spine].SetJointStrength(continuousActionOut[++i]);
        bpDict[head].SetJointStrength(continuousActionOut[++i]);
        bpDict[thighL].SetJointStrength(continuousActionOut[++i]);
        bpDict[shinL].SetJointStrength(continuousActionOut[++i]);
        bpDict[footL].SetJointStrength(continuousActionOut[++i]);
        bpDict[thighR].SetJointStrength(continuousActionOut[++i]);
        bpDict[shinR].SetJointStrength(continuousActionOut[++i]);
        bpDict[footR].SetJointStrength(continuousActionOut[++i]);
        bpDict[armL].SetJointStrength(continuousActionOut[++i]);
        bpDict[forearmL].SetJointStrength(continuousActionOut[++i]);
        bpDict[armR].SetJointStrength(continuousActionOut[++i]);
        bpDict[forearmR].SetJointStrength(continuousActionOut[++i]);
    }

    void UpdateOrientationCube()
    {
        //FACING DIR
        m_WalkDir = target.position - orientationCube.transform.position;
        m_WalkDir.y = 0; //flatten dir on the y
        m_WalkDirLookRot = Quaternion.LookRotation(m_WalkDir); //get our look rot to the target

        //UPDATE ORIENTATION CUBE POS & ROT
        orientationCube.transform.position = hips.position;
        orientationCube.transform.rotation = m_WalkDirLookRot;
    }

    void FixedUpdate()
    {
        if (detectTargets)
        {
            foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
            {
                if (bodyPart.targetContact && bodyPart.targetContact.touchingTarget)
                {
                    TouchedTarget();
                }
            }
        }

        UpdateOrientationCube();

        // Set reward for this step according to mixture of the following elements.
        // a. Velocity alignment with goal direction.
        var moveTowardsTargetReward = Vector3.Dot(orientationCube.transform.forward,
            Vector3.ClampMagnitude(m_JdController.bodyPartsDict[hips].rb.velocity, maximumWalkingSpeed));
        // b. Rotation alignment with goal direction.
        var lookAtTargetReward = Vector3.Dot(orientationCube.transform.forward, head.forward);
        // c. Encourage head height.
        var headHeightOverFeetReward = (head.position.y - footL.position.y) + (head.position.y - footR.position.y);
        AddReward(
            +0.02f * moveTowardsTargetReward
            + 0.01f * lookAtTargetReward
            + 0.01f * headHeightOverFeetReward
        );
    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(1f);
        if (respawnTargetWhenTouched)
        {
            MoveTargetToRandomPosition();
        }
    }

    /// <summary>
    /// Moves target to a random position within specified radius.
    /// </summary>
    public void MoveTargetToRandomPosition()
    {
        var newTargetPos = Random.insideUnitSphere * targetSpawnRadius;
        newTargetPos.y = 5;
        target.position = newTargetPos + ground.position;
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Reset all of the body parts
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        //Random start rotation to help generalize
        transform.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);

        UpdateOrientationCube();

        if (detectTargets && respawnTargetWhenTouched)
        {
            MoveTargetToRandomPosition();
        }

        SetResetParameters();
    }

    public void SetTorsoMass()
    {
        m_JdController.bodyPartsDict[chest].rb.mass = m_ResetParams.GetWithDefault("chest_mass", 8);
        m_JdController.bodyPartsDict[spine].rb.mass = m_ResetParams.GetWithDefault("spine_mass", 8);
        m_JdController.bodyPartsDict[hips].rb.mass = m_ResetParams.GetWithDefault("hip_mass", 8);
    }

    public void SetResetParameters()
    {
        SetTorsoMass();
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = orientationCube.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, orientationCube.transform.localScale);
            Gizmos.DrawRay(Vector3.zero, Vector3.forward);
        }
    }
}

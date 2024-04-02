using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BallAgentLogic : Agent
{
    Rigidbody rBody;
    public Transform target;
    public float speed = 40f;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0.65f, 0.25f, 0f);

        target.localPosition = new Vector3(-1.5f * Random.value * 0.7f, -0.5f + Random.value * 1f, -0.8f + Random.value * 1.6f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rBody.velocity);

    }
   
    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = discreteActions[0] * -1;

        if (discreteActions[1] == 0) controlSignal.z = 0;
        if (discreteActions[1] == 1) controlSignal.z = -1;
        if (discreteActions[1] == 2) controlSignal.z = 1;

        if (this.transform.localPosition.x > -0.7f)
        {
            rBody.AddForce(controlSignal * speed);
        }

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);

        if (distanceToTarget < 0.1f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        if(this.transform.localPosition.y < -0.1f)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        float axisVertFloat = Input.GetAxisRaw("Vertical");
        int axisVertInt = 0;

         if (axisVertFloat < 0f)
        {
            axisVertInt = -1;
        }

         else if (axisVertFloat > 0f)
        {
            axisVertInt = 1;
        }


        float axisHorizFloat = Input.GetAxisRaw("Horizontal");
        int axisHorizInt = 0;

        if (axisHorizFloat < 0f)
        {
            axisHorizInt = -1;
        }

        else if (axisHorizFloat > 0f)
        {
            axisHorizInt = 1;
        }

        discreteActionsOut[0] = axisVertInt;
        discreteActionsOut[1] = axisHorizInt;

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

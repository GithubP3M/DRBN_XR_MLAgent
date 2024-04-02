using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

using System.Collections.Generic;
using System.Linq;
using System;


public class ReacherAgent2 : Agent
{
    public List<GameObject> pendulumlist;
    public GameObject hand;
    public GameObject goal;
    float m_GoalDegree;

    public Rigidbody[] RBlist;
    
    // speed of the goal zone around the arm (in radians)
    float m_GoalSpeed;
    // radius of the goal zone
    float m_GoalSize;
    // Magnitude of sinusoidal (cosine) deviation of the goal along the vertical dimension
    float m_Deviation;
    // Frequency of the cosine deviation of the goal along the vertical dimension
    float m_DeviationFreq;

    

    EnvironmentParameters m_ResetParams;

    List<GameObject> CountObjects()
    {
        GameObject[] pendulumarr = GameObject.FindGameObjectsWithTag("reachlink") as GameObject[];

        pendulumlist = pendulumarr.ToList<GameObject>();
        List<GameObject> quarantinelist = new List<GameObject>();

        Debug.Log(pendulumlist.Count);
        
        foreach(GameObject pendu in pendulumlist)
        {
            if (pendu.transform.root==this.gameObject.transform)
            {
                quarantinelist.Add(pendu);
            }
        }

        Debug.Log(quarantinelist.Count);

        return quarantinelist;
    }

    /// <summary>
    /// Collect the rigidbodies of the reacher in order to resue them for
    /// observations and actions.
    /// </summary>
    public override void Initialize()
    {
        
        pendulumlist = CountObjects();
        RBlist = new Rigidbody[pendulumlist.Count];

        for (int i = 0; i < pendulumlist.Count; i++)
        {
            RBlist[i] = pendulumlist[i].GetComponent<Rigidbody>();
        }
        

        //m_Rb1 = pendulum1.GetComponent<Rigidbody>();
        //m_Rb2 = pendulum2.GetComponent<Rigidbody>();
        //m_Rb3 = pendulum3.GetComponent<Rigidbody>();
        //m_Rb4 = pendulum4.GetComponent<Rigidbody>();
        //m_Rb5 = pendulum5.GetComponent<Rigidbody>();
        //m_Rb6 = pendulum6.GetComponent<Rigidbody>();
        //m_Rb7 = pendulum7.GetComponent<Rigidbody>();
        //m_Rb8 = pendulum8.GetComponent<Rigidbody>();
        //m_Rb9 = pendulum9.GetComponent<Rigidbody>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    /// <summary>
    /// We collect the normalized rotations, angularal velocities, and velocities of both
    /// limbs of the reacher as well as the relative position of the target and hand.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        for (int i = 0; i < pendulumlist.Count; i++)
        {
            sensor.AddObservation(pendulumlist[i].transform.localPosition);
            sensor.AddObservation(pendulumlist[i].transform.rotation);
            sensor.AddObservation(RBlist[i].angularVelocity);
            sensor.AddObservation(RBlist[i].velocity);
        }
            

        //sensor.AddObservation(pendulum1.transform.localPosition);
        //sensor.AddObservation(pendulum1.transform.rotation);
        //sensor.AddObservation(m_Rb1.angularVelocity);
        //sensor.AddObservation(m_Rb1.velocity);

        //sensor.AddObservation(pendulum2.transform.localPosition);
        //sensor.AddObservation(pendulum2.transform.rotation);
        //sensor.AddObservation(m_Rb2.angularVelocity);
        //sensor.AddObservation(m_Rb2.velocity);

        //sensor.AddObservation(pendulum3.transform.localPosition);
        //sensor.AddObservation(pendulum3.transform.rotation);
        //sensor.AddObservation(m_Rb3.angularVelocity);
        //sensor.AddObservation(m_Rb3.velocity);

        //sensor.AddObservation(pendulum4.transform.localPosition);
        //sensor.AddObservation(pendulum4.transform.rotation);
        //sensor.AddObservation(m_Rb4.angularVelocity);
        //sensor.AddObservation(m_Rb4.velocity);

        //sensor.AddObservation(pendulum5.transform.localPosition);
        //sensor.AddObservation(pendulum5.transform.rotation);
        //sensor.AddObservation(m_Rb5.angularVelocity);
        //sensor.AddObservation(m_Rb5.velocity);

        //sensor.AddObservation(pendulum6.transform.localPosition);
        //sensor.AddObservation(pendulum6.transform.rotation);
        //sensor.AddObservation(m_Rb6.angularVelocity);
        //sensor.AddObservation(m_Rb6.velocity);

        //sensor.AddObservation(pendulum7.transform.localPosition);
        //sensor.AddObservation(pendulum7.transform.rotation);
        //sensor.AddObservation(m_Rb7.angularVelocity);
        //sensor.AddObservation(m_Rb7.velocity);

        //sensor.AddObservation(pendulum8.transform.localPosition);
        //sensor.AddObservation(pendulum8.transform.rotation);
        //sensor.AddObservation(m_Rb8.angularVelocity);
        //sensor.AddObservation(m_Rb8.velocity);

        //sensor.AddObservation(pendulum9.transform.localPosition);
        //sensor.AddObservation(pendulum9.transform.rotation);
        //sensor.AddObservation(m_Rb9.angularVelocity);
        //sensor.AddObservation(m_Rb9.velocity);

        sensor.AddObservation(goal.transform.localPosition);
        sensor.AddObservation(hand.transform.localPosition);

        sensor.AddObservation(m_GoalSpeed);
    }

    /// <summary>
    /// The agent's four actions correspond to torques on each of the two joints.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actionBuffers)//(float[] vectorAction)
    {
        var continuousActionOut = actionBuffers.ContinuousActions;
        Debug.Log("continuousActionOut.Length " + continuousActionOut.Length);
        m_GoalDegree += m_GoalSpeed;
        UpdateGoalPosition();

        for (int i = 0; i < pendulumlist.Count; i++)
        {
            var torqueX = Mathf.Clamp(continuousActionOut[0], -1f, 1f) * 150f;
            var torqueZ = Mathf.Clamp(continuousActionOut[1], -1f, 1f) * 150f;
            RBlist[i].AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //var torqueX = Mathf.Clamp(continuousActionOut[0], -1f, 1f) * 150f;
            //var torqueZ = Mathf.Clamp(continuousActionOut[1], -1f, 1f) * 150f;
            //m_Rb1.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb2.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb3.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb4.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb5.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb6.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb7.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb8.AddTorque(new Vector3(torqueX, 0f, torqueZ));

            //torqueX = Mathf.Clamp(continuousActionOut[2], -1f, 1f) * 150f;
            //torqueZ = Mathf.Clamp(continuousActionOut[3], -1f, 1f) * 150f;
            //m_Rb9.AddTorque(new Vector3(torqueX, 0f, torqueZ));
        }
    }

    /// <summary>
    /// Used to move the position of the target goal around the agent.
    /// </summary>
    void UpdateGoalPosition()
    {
        var radians = m_GoalDegree * Mathf.PI / 180f;
        var goalX = 8f * Mathf.Cos(radians)*1.2f;
        var goalY = 8f * Mathf.Sin(radians)*1.2f;
        var goalZ = m_Deviation * Mathf.Cos(m_DeviationFreq * radians);
        goal.transform.position = new Vector3(goalY, goalZ, goalX) + transform.position;
    }

    /// <summary>
    /// Resets the position and velocity of the agent and the goal.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        for (int i = 0; i < pendulumlist.Count; i++)
        {
            //pendulumlist[i].transform.position = new Vector3(0f, -4f, 0f) + transform.position;
            //pendulumlist[i].transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            RBlist[i].velocity = Vector3.zero;
            RBlist[i].angularVelocity = Vector3.zero;

            //pendulum1.transform.position = new Vector3(0f, -4f, 0f) + transform.position;
            //pendulum1.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb1.velocity = Vector3.zero;
            //m_Rb1.angularVelocity = Vector3.zero;

            //pendulum2.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum2.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb2.velocity = Vector3.zero;
            //m_Rb2.angularVelocity = Vector3.zero;

            //pendulum3.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum3.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb3.velocity = Vector3.zero;
            //m_Rb3.angularVelocity = Vector3.zero;

            //pendulum4.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum4.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb4.velocity = Vector3.zero;
            //m_Rb4.angularVelocity = Vector3.zero;

            //pendulum5.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum5.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb5.velocity = Vector3.zero;
            //m_Rb5.angularVelocity = Vector3.zero;

            //pendulum5.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum5.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb6.velocity = Vector3.zero;
            //m_Rb6.angularVelocity = Vector3.zero;

            //pendulum5.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum5.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb7.velocity = Vector3.zero;
            //m_Rb7.angularVelocity = Vector3.zero;

            //pendulum5.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum5.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb8.velocity = Vector3.zero;
            //m_Rb8.angularVelocity = Vector3.zero;

            //pendulum5.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
            //pendulum5.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            //m_Rb9.velocity = Vector3.zero;
            //m_Rb9.angularVelocity = Vector3.zero;
        }

        

        m_GoalDegree = UnityEngine.Random.Range(0, 360);
        UpdateGoalPosition();

        SetResetParameters();


        goal.transform.localScale = new Vector3(m_GoalSize, m_GoalSize, m_GoalSize);
    }

    public void SetResetParameters()
    {
        m_GoalSize = m_ResetParams.GetWithDefault("goal_size", 5);
        m_GoalSpeed = UnityEngine.Random.Range(-1f, 1f) * m_ResetParams.GetWithDefault("goal_speed", 1);
        m_Deviation = m_ResetParams.GetWithDefault("deviation", 0);
        m_DeviationFreq = m_ResetParams.GetWithDefault("deviation_freq", 0);
    }
}

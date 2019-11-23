using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private bool initialized = false;
    private Rigidbody rBody;
    private AIPawn aiPawn;
    public NeuralNetwork net;
    private Material[] mats;
    private Transform tf;
    public float distance = 10.0f;
    public RaycastHit[] hit;
    public Vector3[] directions;
    public float[] distancesToObjects;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        tf = GetComponent<Transform>();
        aiPawn = GetComponent<AIPawn>();
        distancesToObjects = new float[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        mats = new Material[transform.childCount];
        for (int i = 0; i < mats.Length; i++)
            mats[i] = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (initialized == true)
        {
            float distance = Vector3.Distance(tf.position, GameManager.instance.objective.GetComponent<Transform>().position);
            if (distance > 20f)
                distance = 20f;
            for (int i = 0; i < mats.Length; i++)
                mats[i].color = new Color(distance / 20f, (1f - (distance / 20f)), (1f - (distance / 20f)));

            float[] inputs = new float[2];

            float angle = transform.eulerAngles.z % 360f;
            if (angle < 0f)
                angle += 360f;

            Vector3 deltaVector = (GameManager.instance.objective.GetComponent<Transform>().position - tf.position).normalized;


            float rad = Mathf.Atan2(deltaVector.x, deltaVector.z);
            rad *= Mathf.Rad2Deg;

            rad = rad % 360;
            if (rad < 0)
            {
                rad = 360 + rad;
            }

            rad = 90f - rad;
            if (rad < 0f)
            {
                rad += 360f;
            }
            rad = 360 - rad;
            rad -= angle;
            if (rad < 0)
                rad = 360 + rad;
            if (rad >= 180f)
            {
                rad = 360 - rad;
                rad *= -1f;
            }
            rad *= Mathf.Deg2Rad;

            inputs[0] = rad / (Mathf.PI);

            rad = Mathf.Atan2(deltaVector.z, deltaVector.x);
            rad *= Mathf.Rad2Deg;

            rad = rad % 360;
            if (rad < 0)
            {
                rad = 360 + rad;
            }

            rad = 90f - rad;
            if (rad < 0f)
            {
                rad += 360f;
            }
            rad = 360 - rad;
            rad -= angle;
            if (rad < 0)
                rad = 360 + rad;
            if (rad >= 180f)
            {
                rad = 360 - rad;
                rad *= -1f;
            }
            rad *= Mathf.Deg2Rad;

            inputs[1] = rad / (Mathf.PI);

            //inputs[0] = distance;

            float[] outputs = net.CalcNeuronValue(inputs);

            aiPawn.ForwardBackMovement(outputs[0]);
            aiPawn.LateralMovement(outputs[1]);
            aiPawn.Rotation(outputs[2]);

            for (int input = 0; input < inputs.Length; input++)
            {
                net.AddFitness((1f - Mathf.Abs(inputs[0]))); // When facing the target, it will subtract 1 - abs(radians / pi) to fitness every update
            }            
        }
            //if (initialized == true)
            //{
            //    float[] inputs = new float[8];

            //    for (int input = 0; input < inputs.Length; input++)
            //    {
            //        inputs[input] = distancesToObjects[input];  
            //    }
            //    float[] outputs = net.CalcNeuronValue(inputs);
            //    aiPawn.ForwardBackMovement(outputs[0]);
            //    aiPawn.LateralMovement(outputs[1]);
            //    aiPawn.Rotation(outputs[2]);
            hit = new RaycastHit[8];
            int hitL = hit.Length;

            // Directions Start forward then moves clockwise
            directions = new Vector3[] { new Vector3(0, 0, distance), new Vector3(distance, 0, distance),
            new Vector3(distance, 0, 0), new Vector3(distance, 0, -distance),  new Vector3(0, 0, -distance),
            new Vector3(-distance, 0, -distance), new Vector3(-distance, 0, 0), new Vector3(-distance, 0, distance) };

            while (hitL > 0)
            {
                hitL--;
                if (Physics.Raycast(tf.position, directions[hitL], out hit[hitL], distance))
                {
                    //Debug.DrawRay(tf.position, directions[i], Color.blue, distance);

                //    distancesToObjects[hitL] = hit[hitL].distance;

                //    if (hit[hitL].collider.CompareTag("Objective"))
                //    {
                //        float proxBonus = distance - hit[hitL].distance;
                //        float fitnessBonus = (float)Math.Tanh(proxBonus);
                //        net.AddFitness(fitnessBonus);
                //    }
                //    if (hit[hitL].collider.CompareTag("Obstacle"))
                //    {
                //        float proxBonus = hit[hitL].distance - distance;
                //        float fitnessBonus = (float)Math.Tanh(proxBonus);
                //        net.AddFitness(-fitnessBonus);
                //    }
                }
            }         
        }
    public void Init(NeuralNetwork net)
    {
        this.net = net;
        initialized = true;
    }
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Obstacle"))
        {
            GameManager.instance.aiControllerList.Remove(this.gameObject.GetComponent<AIController>());
            Destroy(this.gameObject);
        }
    }
}

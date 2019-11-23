using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject aiObjectPrefab;

    public bool isTraining = false;
    public int populationSize = 50;
    public int generationNumber = 0;
    public int[] layers = new int[] { 2, 10, 10, 3 };
    public List<NeuralNetwork> neuralNets;
    public List<AIController> aiControllerList;
    public List<GameObject> aiObjectsList;
    public Transform spawnpoint;
    public float genLifespan = 10f;
    int averageFitness = 0;
    public GameObject objective;
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // TODO: Allow Game to persist
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Timer()
    {
        isTraining = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (isTraining == false)
        {
            if (generationNumber == 0)
            {
                InitAINeuralNet();
            }
            else
            {
                neuralNets.Sort();
                float totalFitness = 0;
                for (int i = 0; i < populationSize / 2; i++)
                {
                    neuralNets[i].Mutate();
                    //if (i < populationSize / 2)
                    //{
                    //    neuralNets[i] = new NeuralNetwork(layers);
                    //}
                    //else
                    //{
                    //    neuralNets[i].Mutate();
                    //    totalFitness +=neuralNets[i].fitness;
                    //}
                }

                float averageFitness = totalFitness / populationSize;
                //Debug.Log(averageFitness);

                for (int i = 0; i < populationSize; i++)
                {
                    neuralNets[i].SetFitness(0f);
                }
            }
            generationNumber++;

            isTraining = true;
            Invoke("Timer", genLifespan); // Adds a timer
            SpawnAI();
        }
    }

    private void SpawnAI()
    {
        if (aiObjectsList != null)
        {
            for (int i = 0; i < aiObjectsList.Count; i++)
            {
                GameObject.Destroy(aiObjectsList[i]);
            }
        }

        aiControllerList = new List<AIController>();

        for (int i = 0; i < populationSize; i++)
        {
            GameObject aiObject = Instantiate(aiObjectPrefab, spawnpoint.position, spawnpoint.rotation) as GameObject;

            AIController controller = aiObject.GetComponent<AIController>();

            controller.Init(neuralNets[i]);

            aiObjectsList.Add(aiObject);
            aiControllerList.Add(controller);
        }  
    }
    private void InitAINeuralNet()
    {
        if (populationSize % 2 != 0)
        {
            populationSize = 10;
        }

        neuralNets = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            neuralNets.Add(net);
        }
    }
}

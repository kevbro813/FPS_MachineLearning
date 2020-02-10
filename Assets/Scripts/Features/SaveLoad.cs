using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static List<NeuralNetwork> savedNetworks = new List<NeuralNetwork>();

    public static void SaveNet(string fileName, DQN dqn)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            Debug.Log("A file with that name already exists. Please choose a different name.");
        }
        else
        {
            NeuralNetwork nets = dqn.mainNet;
            savedNetworks.Add(nets);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Path.Combine(Application.persistentDataPath, fileName));
            bf.Serialize(file, savedNetworks);
            file.Close();
            Debug.Log("Save Network");
        }
    }
    public static void LoadNet(string fileName, DQN dqn)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.Open);
            savedNetworks = (List<NeuralNetwork>)bf.Deserialize(file);
            dqn.mainNet = savedNetworks[savedNetworks.Count - 1];
            file.Close();
            Debug.Log("Load Network");
        }
        else
        {
            Debug.Log("There is no file with that name.");
        }
    }
}

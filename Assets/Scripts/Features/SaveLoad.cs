using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static List<List<NeuralNetwork>> savedNetworks = new List<List<NeuralNetwork>>();
    public static void SaveNet(string fileName)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            Debug.Log("A file with that name already exists. Please choose a different name.");
        }
        else
        {
            //List<NeuralNetwork> nets = DQN.mainNet;
            //savedNetworks.Add(nets);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Path.Combine(Application.persistentDataPath, fileName));
            bf.Serialize(file, savedNetworks);
            file.Close();
            Debug.Log("Save Network");
        }
    }
    public static void LoadNet(string fileName)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.Open);
            savedNetworks = (List<List<NeuralNetwork>>)bf.Deserialize(file);
            //DQN.mainNet = savedNetworks[savedNetworks.Count - 1];
            file.Close();
            Debug.Log("Load Network");
        }
        else
        {
            Debug.Log("There is no file with that name.");
        }
    }
}

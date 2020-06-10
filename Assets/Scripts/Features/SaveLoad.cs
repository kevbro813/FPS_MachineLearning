using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static void SaveNet(string fileName, DQN dqn)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            Debug.Log("A file with that name already exists. Please choose a different name.");
        }
        else
        {
            NeuralNet nets = dqn.mainNet;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Path.Combine(Application.persistentDataPath, fileName));
            bf.Serialize(file, nets);
            file.Close();
            Debug.Log("Save Network: " + fileName);
        }
    }
    public static void LoadNet(string fileName, DQN dqn)
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.Open);
            dqn.mainNet = (NeuralNet)bf.Deserialize(file);
            file.Close();
            Debug.Log("Load Network: " + fileName);
        }
        else
        {
            Debug.Log("There is no file with that name.");
        }
    }
    public static void SaveSettings(string fileName)
    {
        Settings s = GameManager.instance.settings;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path.Combine(Application.persistentDataPath, fileName));
        bf.Serialize(file, s);
        file.Close();
        Debug.Log("Save Settings: " + fileName);
    }
    public static void LoadSettings(string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Path.Combine(Application.persistentDataPath, fileName), FileMode.Open);
        GameManager.instance.settings = (Settings)bf.Deserialize(file);
        file.Close();
        Debug.Log("Load Settings: " + fileName);
    }

}

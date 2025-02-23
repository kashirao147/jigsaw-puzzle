using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
public static class PuzzleSaveSystem
{
   private static string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void SaveGame(PuzzleSaveData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to: " + savePath);
    }


   public static PuzzleSaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogError("Save file not found!");
            return null;
        }

        string json = File.ReadAllText(savePath);
        PuzzleSaveData data = JsonConvert.DeserializeObject<PuzzleSaveData>(json);
        Debug.Log("Game loaded from: " + savePath);
        return data;
    }




}

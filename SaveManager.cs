using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static readonly string BossKey = "bosskey103bosskey103"; // 24 chars for AES-192
    private static readonly string SaveFileName = "savegame.dat";

    public SaveData currentSaveData = new SaveData();

    private void Start()
    {
        // Optional: Load automatically when the game starts
        LoadGame();
    }

    public void SaveGame()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        string jsonData = JsonUtility.ToJson(currentSaveData);
        string encryptedData = Encrypt(jsonData, BossKey);
        File.WriteAllText(path, encryptedData);
        Debug.Log($"Game saved (encrypted) at: {path}");
    }

    public void LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        if (File.Exists(path))
        {
            string encryptedData = File.ReadAllText(path);
            string decryptedData = Decrypt(encryptedData, BossKey);
            currentSaveData = JsonUtility.FromJson<SaveData>(decryptedData);
            Debug.Log("Game loaded successfully!");
        }
        else
        {
            Debug.LogWarning("No save file found. Creating new save data.");
            currentSaveData = new SaveData(); // Fresh new save
        }
    }

    public void ResetSave()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save file deleted. Reset complete.");
        }
        currentSaveData = new SaveData(); // Reset in memory too
    }

    private static string Encrypt(string plainText, string key)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private static string Decrypt(string cipherText, string key)
    {
        byte[] bytes = Convert.FromBase64String(cipherText);
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);

        byte[] iv = new byte[16];
        Array.Copy(bytes, 0, iv, 0, iv.Length);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(bytes, 16, bytes.Length - 16);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}

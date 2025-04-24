using System.Collections;
using UnityEngine;
using Fusion;

public class SaveManager : NetworkBehaviour {

    /// <summary>
    /// SaveManager 싱글톤 구현
    /// </summary>
    #region Singleton Pattern
    private static SaveManager instance = null;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }
    #endregion

    public string saveFilePath;
    public SaveData saveData;
    public NetworkRunner Runner { get; private set; }

    public override void Spawned() {        
        Runner = FindObjectOfType<NetworkRunner>();

        // 싱글톤
        if (instance == null)
        {
            instance = this;
            Runner.MakeDontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Runner.DestroySingleton<SaveManager>();
            Runner.Despawn(this.GetComponent<NetworkObject>());
        }

        // 경로 설정 및 세이브 파일 불러오기
        saveFilePath = Application.persistentDataPath + "/save.json";
        LoadGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(saveFilePath, json);
    }

    public void LoadGame()
    {
        //세이브 파일이 있을 경우 불러오기
        if (System.IO.File.Exists(saveFilePath))
        {
            string json = System.IO.File.ReadAllText(saveFilePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        //없을경우 새로 생성
        else
        {
            Debug.LogWarning("Save file not found. Creating a new one.");
            saveData = new SaveData();
            SaveGame();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]   
    public void SetClearLevelRpc(NostalgiaGameLevel level) {
        if(saveData.clearLevel >= level) return;

        //기존보다 높은 레벨로 클리어한 경우에만 저장
        saveData.clearLevel = level;
        SaveGame();
    }
}

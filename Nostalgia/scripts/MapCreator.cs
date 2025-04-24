using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Fusion;
using Nostal.Util;
using ExitGames.Client.Photon.StructWrapping;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Serialization;

[System.Serializable]
public struct Area
{
    public GameObject[] Tiles;
    public Vector3[]    Positions;
    public Quaternion[] Rotations;
}

public class MapCreator : NetworkBehaviour
{
    // 타일을 object pool느낌으로 저장
    [Header("Area 섞기")]
    [SerializeField] private Area m_area4;
    [SerializeField] private Area m_area3;
    [SerializeField] private Area m_area2;

    [Header("아이템 생성 위치")]
    [SerializeField] public Transform[] _itemPositions;
    [SerializeField] public GameObject[] _itemPrefab;
    [SerializeField] public int[] _itemNums;

    [Header("플레이어 스폰 위치")]
    [SerializeField] public Transform[] _spawnPositions;

    [Header("탈출구 설정")]
    [SerializeField] public GameObject _exitPrefab;
    [SerializeField] public Transform[] _exitPositions;
    [SerializeField] public NostalgiaGameLevel NextSceneName;

    [Header("스포너")] 
    [SerializeField] private MobSpawner m_mobSpawner;

    [Header("NavMeshSurface")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    
    [Header("Jumpscares")]
    [SerializeField] public Jumpscare[] jumpscares;
    private int m_shuffleCompleteCount = 3;

    [Networked]
    public bool bMapCreated { get; private set; } = false;

    public IEnumerator CreateMap()
    {
        if (bMapCreated == true || !HasStateAuthority)
        {
            yield break;
        }
        
        //4갈래, 3갈래, 2갈래 맵들을 배치
        StartCoroutine(ShuffleAreaCoroutine(m_area4));
        StartCoroutine(ShuffleAreaCoroutine(m_area3));
        StartCoroutine(ShuffleAreaCoroutine(m_area2));
    }

    private IEnumerator ShuffleAreaCoroutine(Area area)
    {
        int length = area.Tiles.Length;
        
        //랜덤 포지션 뽑기
        int[] random = Utility.GetRandomIntArray(length, 0, length - 1);
        //해당하는 포지션과 회전값으로 타일을 배치
        for (int i = 0; i < length; i++)
        {
            area.Tiles[random[i]].transform.position = area.Positions[i];
            area.Tiles[random[i]].transform.rotation = area.Rotations[i];
            yield return null;
        }

        CompleteTileShuffle();
    }

    private void CompleteTileShuffle()
    {
        //각 area의 타일을 섞는 코루틴이 끝날 때마다 호출됨
        --m_shuffleCompleteCount;
        if (m_shuffleCompleteCount > 0)
        {
            return;
        }
        
        //모든 area 타일 생성이 끝나면 navmesh를 구움
        navMeshSurface.BuildNavMesh();
        bMapCreated = true;
        
        //mob, item, player, exit 생성
        m_mobSpawner.SpawnMobs();
        StartCoroutine(SetItems());
        StartCoroutine(SetPlayers());
        StartCoroutine(SetExit());
    }

    public IEnumerator SetPlayers() {
        PlayerSpawner playerSpawner = GameManager.Instance.PlayerSpawner;

        //랜덤 포지션 뽑기
        int[] random = Utility.GetRandomIntArray(1, 0, _spawnPositions.Length-1);
        Transform spawnPosition = _spawnPositions[random[0]];

        //두 플레이어 스폰함수 호출
        playerSpawner.PlayerSpawnRpc(GameManager.Instance.FatherPlayerRef, spawnPosition.position);
        yield return null;
        playerSpawner.PlayerSpawnRpc(
            GameManager.Instance.DaughterPlayerRef, 
            new Vector3(
                spawnPosition.position.x, 
                spawnPosition.position.y, 
                spawnPosition.position.z + 1));
        yield return null;
    }

    public IEnumerator SetItems() {
        //랜덤 포지션 뽑기
        int[] random = Utility.GetRandomIntArray(_itemPositions.Length, 0, _itemPositions.Length-1);
        int cnt = 0;
        //아이템 종류별로 스폰
        for(int i=0; i<_itemNums.Length; i++) {
            for(int j=0; j<_itemNums[i]; j++) {
                var item = Runner.Spawn(
                    _itemPrefab[i], 
                    _itemPositions[random[cnt]].position, 
                    _itemPositions[random[cnt]].rotation, 
                    Runner.LocalPlayer);
                cnt++;
                yield return null;
            }
        }
    }

    public IEnumerator SetExit() {
        //랜덤 포지션 뽑기
        int[] random = Utility.GetRandomIntArray(1, 0, _exitPositions.Length-1);
        //탈출구 스폰
        Runner.Spawn(
            _exitPrefab,
            _exitPositions[random[0]].position, 
            _exitPositions[random[0]].rotation, 
            Runner.LocalPlayer,
            OnBeforeExitSpawned);
        yield return null;
    }

    //탈출구 스폰 전 초기화를 위한 호출
    public void OnBeforeExitSpawned(NetworkRunner runner, NetworkObject obj) 
    {
        ExitDoor exitDoor = obj.GetComponent<ExitDoor>();
        exitDoor.nextScene = NextSceneName;
    }
}

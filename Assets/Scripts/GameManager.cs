using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Police Spawning")]
    public GameObject policePrefab;
    public float firstSpawnDelay = 3f;
    public float spawnInterval = 8f;
    public int maxPolice = 4;
    public float spawnDistance = 12f;

    [Header("Cash Spawning")]
    public GameObject cashPrefab;
    public float cashSpawnInterval = 2.5f;
    public float roadHalfWidth = 2.5f;
    public float spawnAheadDist = 10f;

    [Header("Difficulty")]
    public float difficultyBonus = 0f;

    public int policeCount = 0; // public so PoliceAI.OnDestroy can decrement it

    void Awake() { Instance = this; }

    void Start()
    {
        StartCoroutine(SpawnPoliceLoop());
        StartCoroutine(SpawnCashLoop());
    }

    IEnumerator SpawnPoliceLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);
        while (true)
        {
            if (policeCount < maxPolice) SpawnPolice();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPolice()
    {
        if (policePrefab == null || Player.Instance == null) return;

        Vector3 spawnPos = Player.Instance.transform.position
                         - Player.Instance.transform.up * spawnDistance;

        Instantiate(policePrefab, spawnPos, Quaternion.identity);
        policeCount++;
    }

    IEnumerator SpawnCashLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(cashSpawnInterval);
            SpawnCash();
        }
    }

    void SpawnCash()
    {
        if (cashPrefab == null || Player.Instance == null) return;

        float randomX = Random.Range(-roadHalfWidth, roadHalfWidth);
        Vector3 spawnPos = Player.Instance.transform.position
                         + Player.Instance.transform.up * spawnAheadDist
                         + Vector3.right * randomX;

        Instantiate(cashPrefab, spawnPos, Quaternion.identity);
    }

    public void OnCashCollected(int currentScore)
    {
        difficultyBonus = (currentScore / 3) * 1.2f;
        cashSpawnInterval = Mathf.Max(1f, 2.5f - currentScore * 0.1f);
        spawnInterval = Mathf.Max(4f, 8f - currentScore * 0.3f);
    }
}

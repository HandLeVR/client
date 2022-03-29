using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Manages the selection table containing a basket with balls.
/// </summary>
public class SelectionTable : MonoBehaviour
{
    public Transform[] spawnPoints;
    public Ball ballPrefab;
    public List<Ball> spawnedBalls;

    private List<MeshRenderer> _meshList;

    private void Awake()
    {
        _meshList = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    public void FadeIn(int ballCount)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            _meshList.ForEach(material => material.FadeInMaterials(0.5f));
        }
        SpawnBalls(ballCount);
        spawnedBalls.ForEach(ball => ball.GetComponentInChildren<MeshRenderer>().FadeInMaterials(0.5f));
    }

    public void FadeOut(bool fadeOutTable = true)
    {
        if (fadeOutTable)
            _meshList.ForEach(material => material.FadeOutMaterials(0.5f, () => gameObject.SetActive(false)));
        spawnedBalls.ForEach(b =>
            b.GetComponentInChildren<MeshRenderer>().FadeOutMaterials(0.5f, () => Destroy(b.gameObject)));
        spawnedBalls = new List<Ball>();
    }

    public void DisableBalls()
    {
        spawnedBalls.ForEach(ball => ball.isDisabled = true);
    }

    private void SpawnBalls(int count)
    {
        int[] spawnIndices = GetSpawnIndices(count);
        spawnedBalls = new List<Ball>();
        for (int i = 0; i < count; i++)
        {
            Ball spawnedBall = Instantiate(ballPrefab);
            spawnedBall.transform.position = spawnPoints[spawnIndices[i]].position;
            spawnedBalls.Add(spawnedBall);
        }
    }

    /// <summary>
    /// Randomly chooses some of the possible spawn positions.
    /// </summary>
    private int[] GetSpawnIndices(int length)
    {
        List<int> posIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
            posIndices.Add(i);
        var spawnIndices = new int[length];
        for (int i = 0; i < spawnIndices.Length; i++) {
            var thisNumber = Random.Range(0, posIndices.Count);
            spawnIndices[i] = posIndices[thisNumber];
            posIndices.RemoveAt(thisNumber);
        }

        return spawnIndices;
    }
}

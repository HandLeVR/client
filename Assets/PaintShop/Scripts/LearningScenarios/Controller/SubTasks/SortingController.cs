using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// Manages the sorting sub task. In this sub task the user needs to put the correct objects into a basket.
/// </summary>
public class SortingController : VRSubTaskController
{
    public TextPanel textPanel;
    public Basket basket;
    public GameObject table;
    public GameObject roundCanisterPrefab;
    public GameObject bigCanisterPrefab;
    public GameObject pouringCanisterPrefab;

    public Transform[] spawnPoints;
    public List<Canister> spawnedObjects;

    private bool _isFinished;
    private MeshRenderer _basketMesh;
    private List<MeshRenderer> _tableMeshList;

    private void Awake()
    {
        _basketMesh = basket.GetComponentInChildren<MeshRenderer>();
        _tableMeshList = table.GetComponentsInChildren<MeshRenderer>().ToList();
    }

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        string textMonitor = GetStringFromJSON("textMonitor", jsonObject);
        List<JObject> objects = GetItemsFromJSON("items", jsonObject);
        List<Item> items = new List<Item>();
        objects.ForEach(o => items.Add(new Item((string)o["model"], (string)o["text"], (bool)o["correct"])));

        _isFinished = false;
        textPanel.InitPanel(textMonitor);
        LearningScenariosMonitorController.Instance.ChangePanel(textPanel.gameObject);
        spawnedObjects = new List<Canister>();
        basket.gameObject.SetActive(true);
        basket.onCorrect = OnCorrect;
        table.SetActive(true);

        SpawnItems(items);
        FadeIn();

        SetEducationMasterAndCoins(true);
    }

    private void OnDisable()
    {
        basket.gameObject.SetActive(false);
        table.SetActive(false);
        spawnedObjects.ForEach(o => Destroy(o.gameObject));
    }

    private void OnCorrect()
    {
        if (_isFinished)
            return;

        _isFinished = true;
        SpawnCoins(canSkipSpeech, true);
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(_isFinished, true);
    }

    public override void ContinueCoinSelected()
    {
        base.ContinueCoinSelected();
        FadeOut();
    }

    public override void ReturnCoinSelected()
    {
        base.ReturnCoinSelected();
        FadeOut();
    }

    private void FadeIn()
    {
        _basketMesh.FadeInMaterials(0.5f,
            () => StartCoroutine(Lerp.Color(_basketMesh, _basketMesh.material.color, Color.red, 0.5f)));
        _tableMeshList.ForEach(m => m.FadeInMaterials(0.5f));
        spawnedObjects.ForEach(o => o.FadeIn(0.5f));
    }

    private void FadeOut()
    {
        _basketMesh.FadeOutMaterials(0.5f);
        _tableMeshList.ForEach(m => m.FadeOutMaterials(0.5f));
        spawnedObjects.ForEach(o => o.FadeOut(0.5f));
    }

    /// <summary>
    /// Spawns the items based on the input data, assigns them the appropriate values
    /// and attaches them to random snap points on the table.
    /// </summary>
    private void SpawnItems(List<Item> items)
    {
        int[] spawnIndices = GetSpawnIndices(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            GameObject spawnedItem;
            switch (items[i].model)
            {
                case "CanRound":
                    spawnedItem = Instantiate(roundCanisterPrefab);
                    break;
                case "CanRectangular":
                    spawnedItem = Instantiate(bigCanisterPrefab);
                    break;
                case "CoatCanWithAttachement":
                    spawnedItem = Instantiate(pouringCanisterPrefab);
                    break;
                default:
                    return;
            }

            Canister canister = spawnedItem.GetComponent<Canister>();
            canister.labelText = items[i].text;
            canister.transform.position = spawnPoints[spawnIndices[i]].position;
            canister.transform.Rotate(Vector3.up, Random.Range(0, 360));
            if (items[i].correct)
                basket.correctObjects.Add(canister.gameObject);
            spawnedObjects.Add(canister);
        }
    }

    /// <summary>
    /// Returns a list of indices representing random positions in the original position array.
    /// </summary>
    private int[] GetSpawnIndices(int length)
    {
        List<int> posIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
            posIndices.Add(i);
        var spawnIndices = new int[length];
        for (int i = 0; i < spawnIndices.Length; i++)
        {
            var thisNumber = Random.Range(0, posIndices.Count);
            spawnIndices[i] = posIndices[thisNumber];
            posIndices.RemoveAt(thisNumber);
        }

        return spawnIndices;
    }
}

public class Item
{
    public Item(string model, string text, bool correct)
    {
        this.model = model;
        this.text = text;
        this.correct = correct;
    }

    public string model;
    public string text;
    public bool correct;
}
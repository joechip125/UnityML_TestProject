using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TileStatus
{
    Blank,
    Found,
    Wrong,
    Right
}

public class FieldTile : MonoBehaviour
{
    public Material tileMat;

    private TextMeshProUGUI _text;
    public IntVector2 coordinates;
    private float _collectValue;

    public GameObject spawnCollect;
    
    private List<GameObject> _collectRef = new();

    public TileStatus currentStatus;

    public int numberCollect;

    public float CollectValue
    {
        get => _collectValue;
        set
        {
            _collectValue = value;
            UpdateText();
        }
    }

    
    private void UpdateText()
    {
        _text.text = $"{_collectValue}";
    }
    
    
    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateTileStatus(TileStatus newStatus)
    {
        currentStatus = newStatus;
        switch (newStatus)
        {
            case TileStatus.Blank:
                tileMat.color = Color.black;
                break;
            case TileStatus.Found:
                tileMat.color = Color.white;
                break;
            case TileStatus.Wrong:
                tileMat.color = Color.red;
                break;
            case TileStatus.Right:
                tileMat.color = Color.green;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null);
        }
    }

    public float range = 3;
    public void SpawnRandomAmount()
    {
        var amount = Random.Range(0, 1);
        numberCollect = amount;

        for (int i = 0; i < amount; i++)
        {
            var temp  = Instantiate(spawnCollect,
                transform, false);
            temp.transform.localPosition += new 
                Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
            
            _collectRef.Add(temp);
        }
    }


    public void SpawnSetAmount(int amount)
    {
        numberCollect = amount;
        for (int i = 0; i < amount; i++)
        {
            var temp  = Instantiate(spawnCollect,
                transform, false);
            temp.transform.localPosition += new 
                Vector3(Random.Range(-range, range), 0.15f, Random.Range(-range, range));
            
            _collectRef.Add(temp);
        }
    }
    
    public void ClearAllCollect()
    {
        foreach (var c in _collectRef)
        {
            if(c) Destroy(c);
        }
        
        _collectRef.Clear();
    }
    
    void Start()
    {
        UpdateTileStatus(TileStatus.Found);
        //UpdateText();
    }
}

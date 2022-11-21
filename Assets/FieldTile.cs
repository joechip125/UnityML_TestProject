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
    Found
}

public class FieldTile : MonoBehaviour
{
    public Material tileMat;

    private TextMeshProUGUI _text;

    public float collectValue;

    public GameObject spawnCollect;
    
    private List<GameObject> _collectRef;

    public int numberCollect;

    public float CollectValue
    {
        get => collectValue;
        set
        {
            collectValue = value;
            UpdateText();
        }
    }

    
    private void UpdateText()
    {
        _text.text = $"CS:{collectValue}";
    }
    
    
    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateTileStatus(TileStatus newStatus)
    {
        switch (newStatus)
        {
            case TileStatus.Blank:
                tileMat.color = Color.black;
                break;
            case TileStatus.Found:
                tileMat.color = Color.white;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null);
        }
    }
    
    public void SpawnRandomAmount()
    {
        var amount = Random.Range(1, 10);
        numberCollect = amount;

        for (int i = 0; i < amount; i++)
        {
            var random = transform.localPosition   
                         + new Vector3(Random.Range(-4, 4), 0.15f, Random.Range(-4, 4));
            var temp  = Instantiate(spawnCollect,
                transform, false);
            
            _collectRef.Add(temp);
        }
    }

    public void ClearAllCollect()
    {
        foreach (var c in _collectRef)
        {
            Destroy(c);
        }
        
        _collectRef.Clear();
    }
    
    void Start()
    {
        UpdateTileStatus(TileStatus.Found);
        UpdateText();
    }
}

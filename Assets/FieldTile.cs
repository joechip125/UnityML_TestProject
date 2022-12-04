using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject spawnPoison;
    
    private List<GameObject> _collectRef = new();

    public TileStatus currentStatus;

    public int numberCollect;
    List<Vector3> tempLoc = new();

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

    private float range = 2f;
    public void SpawnRandomAmount()
    {
        var amount = Random.Range(0, 1);
        numberCollect = amount;
        tempLoc.Clear();

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
        tempLoc.Clear();
        numberCollect = amount;
        var tolerance = 1f;
        var unique = false;
        for (int i = 0; i < amount; i++)
        {
            while (!unique)
            {
                var location = new Vector3(Random.Range(-range, range), 0.15f, Random.Range(-range, range));
                if (tempLoc.Any(x => Vector3.Distance(location, x) < tolerance))
                {
                    continue;
                }
              
                tempLoc.Add(location);
                unique = true;
            }
            
            var temp  = Instantiate(Random.Range(0, 2) == 0 
                    ? spawnCollect : spawnPoison,
                transform, false);
            temp.transform.localPosition += tempLoc[i];
            
            _collectRef.Add(temp);

            unique = false;
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

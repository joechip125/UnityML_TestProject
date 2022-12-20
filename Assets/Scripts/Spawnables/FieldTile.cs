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

public enum TileLocations
{
    Min,
    Max,
    Center
}

public class FieldTile : MonoBehaviour
{
    public Material tileMat;

    private TextMeshProUGUI _text;
    public IntVector2 coordinates;
    private float _collectValue;
    public Transform owner;

    public GameObject spawnCollect;
    public GameObject spawnPoison;
    
    private List<GameObject> _collectRef = new();

    public TileStatus currentStatus;
    
    private float range = 2f;
    
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
        var extents = GetComponent<MeshCollider>().bounds.extents;
        var extent = transform.localPosition;
        range = Mathf.Min(extents.x, extents.z) - 1;
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

    public Vector3 GetTileLocation(TileLocations location)
    {
        var retVec = new Vector3();
        var bounds = GetComponent<MeshCollider>().bounds;

        retVec = location switch
        {
            TileLocations.Min => bounds.min,
            TileLocations.Max => bounds.max,
            TileLocations.Center => bounds.center,
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
        };

        return retVec;
    }
    
    public void SpawnSetAmount(int amount)
    {
        SetUniqueLocations(amount, 1.2f);
        for (int i = 0; i < amount; i++)
        {
            var temp  = Instantiate(Random.Range(0, 2) == 0 
                    ? spawnCollect : spawnPoison,
                transform, false);
            temp.transform.localPosition += tempLoc[i];
            
            _collectRef.Add(temp);
        }
    }
    
    public void SpawnSetAmount(List<CollectTypes> typesList)
    {
        SetUniqueLocations(typesList.Count, 1.2f);
     
        for (int i = 0; i < typesList.Count; i++)
        {
            var temp  = Instantiate(typesList[i] == CollectTypes.Collect 
                    ? spawnCollect : spawnPoison,
                transform, false);
            temp.transform.localPosition += tempLoc[i];
            
            _collectRef.Add(temp);
        }
    }

    private void SetUniqueLocations(int numberLocations, float tolerance)
    {
        tempLoc.Clear();
        var unique = false;
        
        for (var i = 0; i < numberLocations; i++)
        {
            while (!unique)
            {
                var location =  new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
                if (tempLoc.Any(x => Vector3.Distance(location, x) < tolerance))
                {
                    continue;
                }
                tempLoc.Add(location);
                unique = true;
            }

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public enum CollectTypes
{
    Collect,
    Poison
}
[Serializable]
public class Collections
{
    public List<CollectTypes> collectTypesList = new();
}

public class SpawnArea : MonoBehaviour
{
    public GameObject spawnPrototype;
    public GameObject wallPrototype;
    public int xTiles;
    public int zTiles;
    [SerializeField, HideInInspector] public List<FieldTile> tiles;
    [SerializeField]private float _percentCollect;
    [SerializeField]private float _percentPoison;
    private IntVector2 _theRightChoice;
    private int _rightChoice;
    private int _totalScore;

    private int _numWrong;
    private int _numRight;
    private List<CollectTypes> _spawnType = new();
    private List<Collections> _collections = new();
    private List<int> _numberToSpawn = new();

    public float PercentCollect
    {
        get => _percentCollect;
        set
        {
            _percentCollect = value;
            _percentPoison = 100 - _percentCollect;
        }
    }
    
    public float PercentPoison
    {
        get => _percentPoison;
        set
        {
            _percentPoison = value;
            _percentCollect = 100 - _percentPoison;
        }
    }
    
    void Start()
    {
        SpawnAreas(xTiles, zTiles);
    }

    public void GetCollection()
    {
        _numberToSpawn.Clear();
        _spawnType.Clear();
        _collections.Clear();
        var total = 0f;

        for (int i = 0; i < tiles.Count; i++)
        {
            var random = Random.Range(1, 4);
            total += random;
            _numberToSpawn.Add(random);
        }
        
        var poison = Mathf.CeilToInt(total / 10 * _percentPoison / 10);
        var collect = Mathf.CeilToInt(total / 10 * _percentCollect / 10);
        _spawnType.AddRange(Enumerable.Repeat(CollectTypes.Poison, poison));
        _spawnType.AddRange(Enumerable.Repeat(CollectTypes.Collect, collect));

        foreach (var n in _numberToSpawn)
        {
            _collections.Add(new Collections());
            for (int i = 0; i < n; i++)
            {
                var choice =   Random.Range(0, _spawnType.Count - 1);

                if (choice < _spawnType.Count)
                {
                    var type = _spawnType[choice];
                    _spawnType.RemoveAt(choice);
                    
                    _collections[^1].collectTypesList.Add(type);
                }
            }
        }
    }
    
    public void RespawnCollection()
    {
        GetCollection();
        var counter = 0;
        foreach (var t in tiles)
        {
            t.ClearAllCollect();

            if (counter != 0)
            {
                t.SpawnSetAmount(_collections[counter].collectTypesList);
            }

            counter++;
        }
    }
    
    public void RespawnCollect()
    {
        var randChoice = Random.Range(1, 4);
        var counter = 0;

        foreach (var t in tiles)
        {
            t.ClearAllCollect();

            if (counter == randChoice)
            {
                _rightChoice = counter;
                _theRightChoice = t.coordinates;
                t.SpawnSetAmount(1);
            }
            counter++;
        }
    }

    public bool CheckTile(int tileChoice)
    {
        if (tileChoice == _rightChoice)
        {
            tiles[tileChoice].CollectValue++;
            return true;
        }

        tiles[tileChoice].CollectValue--;
        return false;
    }

    public Vector3 GetLocationAtTile(IntVector2 tileCoords)
    {
        return tiles.Single(x => x.coordinates == tileCoords)
            .transform.localPosition;
    }
    
    public bool CheckTile(IntVector2 tileChoice)
    {
        if (tileChoice == _theRightChoice)
        {
            Debug.Log($"Right{tileChoice} {_numRight++}");
            //tiles.Single(x => x.coordinates == tileChoice).CollectValue++;
            return true;
        }
        Debug.Log($"Wrong{tileChoice} {_numWrong++}");
        //tiles.Single(x => x.coordinates == tileChoice).CollectValue--;
        return false;
    }
    

    void SpawnAreas(int numX, int numZ)
    {
        Vector3 spawnLoc = transform.localPosition;
        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                var temp = Instantiate(spawnPrototype, spawnLoc + new Vector3(0,0, z * 10), 
                    quaternion.identity, transform).GetComponent<FieldTile>();
                temp.owner = transform;
                tiles.Add(temp);
                temp.coordinates = new IntVector2(x, z);
            }

            spawnLoc += new Vector3(10,0,0);
        }
        SpawnWalls(numX, numZ);
    }

    private void SpawnWalls(int numX, int numZ)
    {
        var min = tiles[0].GetTileLocation(TileLocations.Min);
        var max = tiles[^1].GetTileLocation(TileLocations.Max);
        var center = (max - new Vector3(Mathf.Abs(min.x), 0,Mathf.Abs(min.z))) / 2;
        //var rot = Quaternion.LookRotation( center - new Vector3(1, 5, numZ * 10), Vector3.up);
        
        var temp2 = Instantiate(wallPrototype, 
            new Vector3(center.x, 0, min.z), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(numX * 10, 5, 1);
        
        temp2 = Instantiate(wallPrototype, 
            new Vector3(center.x, 0, max.z), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(numX * 10, 5, 1);
        
        temp2 = Instantiate(wallPrototype, 
            new Vector3(min.x, 0, center.z), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(1, 5, numZ * 10);
        
        temp2 = Instantiate(wallPrototype, 
            new Vector3(max.x, 0, center.z), 
            Quaternion.identity, transform);
        temp2.transform.localScale = new Vector3(1, 5, numZ * 10);
    }
    
 
}

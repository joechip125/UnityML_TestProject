using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileStatus
{
    Blank,
    Found
}

public class FieldTile : MonoBehaviour
{
    public Material tileMat;

    private void Awake()
    {
      
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
    
    
    void Start()
    {
        UpdateTileStatus(TileStatus.Blank);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

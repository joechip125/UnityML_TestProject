using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TensorTile : MonoBehaviour
{
    private TextMeshProUGUI _tileText;
    private float _tileNumber;
    
    private void Awake()
    {
        _tileText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void AddTileNum(float addNum)
    {
        _tileNumber += addNum;
        _tileText.text = $"{_tileNumber}";
    }
    
    public void SetTileNum(float tileNum)
    {
        _tileNumber = tileNum;
        _tileText.text = $"{tileNum}";
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TensorTile : MonoBehaviour
{
    private TextMeshProUGUI _tileText;
    private float _tileNumber;
    public int tileIndex;
    
    private void Awake()
    {
        _tileText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void AddTileNum(float addNum)
    {
        _tileNumber = Mathf.Clamp(_tileNumber + addNum, 0, 1);
        _tileText.text = $"{_tileNumber}";
    }
    
    public void SetTileNum(float tileNum)
    {
        _tileNumber = Mathf.Clamp(tileNum, 0, 1);
        _tileText.text = $"{tileNum}";
    }
}

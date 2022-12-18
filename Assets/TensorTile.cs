using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TensorTile : MonoBehaviour
{
    private TextMeshProUGUI _tileText;
    private void Awake()
    {
        _tileText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetTileNum(float tileNum)
    {
        _tileText.text = $"{tileNum}";
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using UnityEngine;
using Unity.MLAgents.Sensors;

public enum CellContents
{
    None,
    Collector,
    Collectable,
    Poison
}


[Serializable]
public class CellInfo
{
    public IntVector2 index;
    public Vector3 location;
    public CellContents contents;
}

public class MyGrid : MonoBehaviour
{
    
    public const int NumChannels = 4;
    // Buffer channels.
    public const int Collectable = 1;
    public const int Poison = 2;
    public const int Visit = 3;
    public const int Wall = 4;
    
    private bool m_ShowGizmos = false;
    [SerializeField] public Vector3 minCellScale = new Vector3(1f, 0.01f, 1f);
    [SerializeField] public int cellsX = 12;
    [SerializeField] public int cellsZ = 12;
    [HideInInspector, SerializeField]
    internal float m_GizmoYOffset = 0f;

    public List<ChannelLabel> labels;

    private GridBuffer _gridBuffer;

    // Start is called before the first frame update
    void Start()
    {
       
    }


    private void Awake()
    {
        _gridBuffer = new ColorGridBuffer(NumChannels, new Vector2Int(cellsX, cellsZ));
        m_ShowGizmos = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Color ScanCell(int indexX, int indexZ)
    {
        var pos = transform.position + 
                  new Vector3(indexX * minCellScale.x, 5, indexZ * minCellScale.z);

        var ray = new Ray(pos, Vector3.down);
        Physics.SphereCast(ray, 0.5f,out var sphereHit, 12f);
        if (!sphereHit.collider) return Color.white;
        
        var gObj = sphereHit.collider.gameObject;


        for (int i = 0; i < labels.Count; i++)
        {
            if (gObj.CompareTag(labels[i].Name))
            {
                return labels[i].Color;
            }
        }
       
        return Color.white;
    }

    public void DivideAnd()
    {
        
    }
    
    void OnDrawGizmos()
    {
        //if (!Application.IsPlaying(gameObject)) return;
        var scale = new Vector3(minCellScale.x * 6 , 1, minCellScale.z * 6);
        var placement = transform.position;
     
        var divide = 2;


        for (int x = 0; x < divide; x++)
        {
            for (int z = 0; z < divide; z++)
            {
                var debugRayColor = ScanCell(x, z);
                
                Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
                Gizmos.DrawCube(placement + new Vector3(0,0, scale.z * z), scale);
            }

            placement += new Vector3(scale.x, 0, 0);
        }
    }
}

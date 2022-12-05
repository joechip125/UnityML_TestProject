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
    
    public const int NumChannels = 3;
    // Buffer channels.
    public const int Visit = 0;
    public const int Collectable = 1;
    public const int Poison = 2;
    
    private bool m_ShowGizmos = false;
    [SerializeField] public Vector3 m_CellScale = new Vector3(1f, 0.01f, 1f);
    [SerializeField] public int cellsX = 12;
    [SerializeField] public int cellsZ = 12;
    [HideInInspector, SerializeField]
    internal float m_GizmoYOffset = 0f;

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
                  new Vector3(indexX * m_CellScale.x, 5, indexZ * m_CellScale.z);

        var ray = new Ray(pos, Vector3.down);
        Physics.SphereCast(ray, 0.5f,out var sphereHit, 12f);
        if (!sphereHit.collider) return Color.white;
        
        var gObj = sphereHit.collider.gameObject;
        
        if (gObj.CompareTag("Collectable"))
        {
            _gridBuffer.Write(Collectable, indexX, indexZ, 1);
            return Color.magenta;
        }
        if (gObj.CompareTag("Poison"))
        {
            _gridBuffer.Write(Poison, indexX, indexZ, 1);
            return Color.red;
        }
        return Color.white;
    }
    
    void OnDrawGizmos()
    {
        if (Application.IsPlaying(gameObject))
        {
            _gridBuffer.Clear();
            var scale = new Vector3(m_CellScale.x, 1, m_CellScale.z);
            var placement = transform.position;
    
            for (int x = 0; x < cellsX; x++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    var debugRayColor = ScanCell(x, z);
                    var something = _gridBuffer.Read(1, x, z);
                    if (something == 1)
                    {
                        debugRayColor = Color.green;
                    }
                    
                    Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
                    Gizmos.DrawCube(placement + new Vector3(0,0, scale.z * z), Vector3.one);
                }

                placement += new Vector3(scale.x, 0, 0);
            }
        }
    }
}

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
    private bool m_ShowGizmos = true;
    [SerializeField] public Vector3 m_CellScale = new Vector3(1f, 0.01f, 1f);
    [SerializeField] public int cellsX = 12;
    [SerializeField] public int cellsZ = 12;
    [HideInInspector, SerializeField]
    internal float m_GizmoYOffset = 0f;

    public ColorGridBuffer GridBuffer;

    // Start is called before the first frame update
    void Start()
    {
       
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
            
        if (gObj.CompareTag("Collector"))
        {
            return Color.green;
        }
        if (gObj.CompareTag("Collectable"))
        {
            return Color.magenta;
        }
        if (gObj.CompareTag("Poison"))
        {
            return Color.red;
        }
        return Color.white;
    }
    
    void OnDrawGizmos()
    {
        if (m_ShowGizmos)
        {
            var scale = new Vector3(m_CellScale.x, 1, m_CellScale.z);
            var placement = transform.position;
    
            for (int x = 0; x < cellsX; x++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    var debugRayColor = ScanCell(x, z);
                    Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, .5f);
                    Gizmos.DrawCube(placement + new Vector3(0,0, scale.z * z), Vector3.one);
                }

                placement += new Vector3(scale.x, 0, 0);
            }
        }
    }
}

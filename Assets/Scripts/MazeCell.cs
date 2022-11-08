using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct IntVector2 
{
    public int x, z;
	
    public IntVector2 (int x, int z) 
    {
        this.x = x;
        this.z = z;
    }
    
    public static IntVector2 operator + (IntVector2 a, IntVector2 b) 
    {
        a.x += b.x;
        a.z += b.z;
        return a;
    }
}

public class MazeCell : MonoBehaviour
{
    
    public IntVector2 Coordinates;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

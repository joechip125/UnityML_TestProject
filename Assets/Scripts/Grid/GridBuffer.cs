using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class GridBuffer
{
    [Serializable]
    public struct Shape
    {
        
        public int NumChannels;
        
        public int Width;
        
        public int Height;
        
        public Vector2Int Size
        {
            get { return new Vector2Int(Width, Height); }
            set { Width = value.x; Height = value.y; }
        }
        
        public Shape(int numChannels, int width, int height)
        {
            NumChannels = numChannels;
            Width = width;
            Height = height;
        }
        
        public Shape(int numChannels, Vector2Int size)
            : this(numChannels, size.x, size.y) { }
        
        public void Validate()
        {
            if (NumChannels < 1)
            {
                throw new UnityAgentsException("Grid buffer has no channels.");
            }
    
            if (Width < 1)
            {
                throw new UnityAgentsException("Invalid grid buffer width " + Width);
            }
    
            if (Height < 1)
            {
                throw new UnityAgentsException("Invalid grid buffer height " + Height);
            }
        }
    
        public override string ToString()
        {
            return $"Grid {NumChannels} x {Width} x {Height}";
        }
    }
    
    public Shape GetShape()
    {
        return new Shape(m_NumChannels, m_Width, m_Height);
    }
    
    public int NumChannels
    {
        get { return m_NumChannels; }
        set { m_NumChannels = value; Initialize(); }
    }
    private int m_NumChannels;
    
    public int Width
    {
        get { return m_Width; }
        set { m_Width = value; Initialize(); }
    }
    private int m_Width;
    
    public int Height
    {
        get { return m_Height; }
        set { m_Height = value; Initialize(); }
    }
    private int m_Height;

    /// <summary>
    /// Whether the buffer was changed since last Clear() call.
    /// </summary>
    public bool IsDirty { get; private set; }
    
    // [channel][y * width + x]
    private float[][] m_Values;
    
    protected virtual void Initialize()
    {
        m_Values = new float[NumChannels][];

        for (int i = 0; i < NumChannels; i++)
        {
            m_Values[i] = new float[Width * Height];
        }
    }
    
    public GridBuffer(int numChannels, int width, int height)
    {
        m_NumChannels = numChannels;
        m_Width = width;
        m_Height = height;

        Initialize();
    }
    
    public GridBuffer(int numChannels, Vector2Int size)
        : this(numChannels, size.x, size.y) { }
    
    public GridBuffer(Shape shape)
        : this(shape.NumChannels, shape.Width, shape.Height) { }
    
    public virtual void Clear()
    {
        ClearChannels(0, NumChannels);
        //IsDirty = false;
    }

    /// <summary>
    /// Clears grid values of specified channels by setting them to 0.
    /// <param name="start">The first channel's index</param>
    /// <param name="length">The number of channels to clear</param>
    /// </summary>
    public virtual void ClearChannels(int start, int length)
    {
        for (int i = 0; i < length; i++)
        {
            ClearChannel(start + i);
        }
    }

    /// <summary>
    /// Clears grid values of a specified channel by setting them to 0.
    /// <param name="channel">The channel index</param>
    /// </summary>
    public virtual void ClearChannel(int channel)
    {
        if (channel < NumChannels)
        {
            Array.Clear(m_Values[channel], 0, m_Values[channel].Length);
        }
    }
    
    /// <summary>
    /// Writes a float value to a specified grid cell.
    /// </summary>
    /// <param name="channel">The cell's channel index</param>
    /// <param name="x">The cell's x position</param>
    /// <param name="y">The cell's y position</param>
    /// <param name="value">The value to write</param>
    public virtual void Write(int channel, int x, int y, float value)
    {
        m_Values[channel][y * Width + x] = value;
        //IsDirty = true;
    }

    /// <summary>
    /// Writes a float value to a specified grid cell.
    /// </summary>
    /// <param name="channel">The cell's channel index</param>
    /// <param name="pos">The cell's x/y position</param>
    /// <param name="value">The value to write</param>
    public virtual void Write(int channel, Vector2Int pos, float value)
    {
        Write(channel, pos.x, pos.y, value);
    }
    
    
    public virtual bool TryWrite(int channel, int x, int y, float value)
    {
        bool hasPosition = Contains(x, y);
        if (hasPosition)
        {
            Write(channel, x, y, value);
        }
        return hasPosition;
    }
    
    public virtual bool TryWrite(int channel, Vector2Int pos, float value)
    {
        return TryWrite(channel, pos.x, pos.y, value);
    }
    
    public virtual float Read(int channel, int x, int y)
    {
        return m_Values[channel][y * Width + x];
    }
    
    
    public virtual float Read(int channel, Vector2Int pos)
    {
        return Read(channel, pos.x, pos.y);
    }
    
    
    public virtual bool TryRead(int channel, int x, int y, out float value)
    {
        bool hasPosition = Contains(x, y);
        value = hasPosition ? Read(channel, x, y) : 0;
        return hasPosition;
    }
    
    public virtual bool TryRead(int channel, Vector2Int pos, out float value)
    {
        return TryRead(channel, pos.x, pos.y, out value);
    }
    
    
    public virtual bool Contains(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
    
    public virtual bool Contains(Vector2Int pos)
    {
        return Contains(pos.x, pos.y);
    }
    /// <summary>
    /// Calculates a grid position from a normalized Vector2.
    /// </summary>
    /// <param name="norm">The normalized vector</param>
    /// <returns>The grid position</returns>
    public Vector2Int NormalizedToGridPos(Vector2 norm)
    {
        return new Vector2Int(
            (int)(norm.x * Width),
            (int)(norm.y * Height)
        );
    }

    /// <summary>
    /// Calculates a grid rectangle from a normalized Rect.
    /// </summary>
    /// <param name="norm">The normalized rectangle</param>
    /// <returns>The grid rectangle</returns>
    public RectInt NormalizedToGridRect(Rect norm)
    {
        return new RectInt(
            (int)(norm.xMin * Width),
            (int)(norm.yMin * Height),
            (int)(norm.width * Width),
            (int)(norm.height * Height)
        );
    }

    /// <summary>
    /// Returns the number of grid layers.
    /// Not supported by <see cref="GridBuffer"/> base class.
    /// </summary>
    /// <returns>Number of layers</returns>
    public virtual int GetNumLayers()
    {
        ThrowNotSupportedError();
        return 0;
    }

    /// <summary>
    /// Returns the grid layer colors.
    /// Not supported by <see cref="GridBuffer"/> base class.
    /// </summary>
    /// <returns>Color32 array [layerIndex][gridPosition]</returns>
    public virtual Color32[][] GetLayerColors()
    {
        ThrowNotSupportedError();
        return null;
    }

    private void ThrowNotSupportedError()
    {
        throw new UnityAgentsException(
            "GridBuffer doesn't support PNG compression. " +
            "Use the ColorGridBuffer instead.");
    }
}   
    
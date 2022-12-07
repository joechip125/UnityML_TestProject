using System;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using UnityEngine;

namespace DefaultNamespace.Grid
{
    [Serializable]
    public class SingleChannel
    {
        public Vector2Int Size
        {
            get { return new Vector2Int(Width, Height); }
            set
            {
                Width = value.x;
                Height = value.y;
            }
        }
        
        public override string ToString()
        {
            return $"Gridx {Width} x {Height}";
        }
        
        public int Width
        {
            get { return m_Width; }
            set
            {
                m_Width = value;
                Initialize();
            }
        }

        private int m_Width;
        
        public int Height
        {
            get { return m_Height; }
            set
            {
                m_Height = value;
                Initialize();
            }
        }

        private int m_Height;

        // [channel][y * width + x]
        private float[] m_Values;


        public SingleChannel(int width, int height)
        {
            m_Width = width;
            m_Height = height;

            Initialize();
        }

        public SingleChannel(Vector2Int size)
            : this(size.x, size.y)
        {
        }
        
        protected virtual void Initialize()
        {
            m_Values = new float[Width * Height];
        }
        
        public virtual void Clear(int channel)
        {
            Array.Clear(m_Values, 0, m_Values.Length);
        }
        
        public virtual void Write(int x, int y, float value)
        {
            m_Values[y * Width + x] = value;
            //IsDirty = true;
        }
        
        public virtual void Write(Vector2Int pos, float value)
        {
            Write(pos.x, pos.y, value);
        }

        public virtual void Write(int index, float value)
        {
            m_Values[index] = value;
        }
        
        public virtual bool TryWrite(int x, int y, float value)
        {
            bool hasPosition = Contains(x, y);
            if (hasPosition)
            {
                Write(x, y, value);
            }

            return hasPosition;
        }
        
        public virtual bool TryWrite( Vector2Int pos, float value)
        {
            return TryWrite(pos.x, pos.y, value);
        }
        
        public virtual float Read( int x, int y)
        {
            return m_Values[y * Width + x];
        }

        public virtual float Read(int index)
        {
            return m_Values[index];
        }
        
        public virtual float Read(Vector2Int pos)
        {
            return Read( pos.x, pos.y);
        }
        
        public virtual bool TryRead(int x, int y, out float value)
        {
            bool hasPosition = Contains(x, y);
            value = hasPosition ? Read( x, y) : 0;
            return hasPosition;
        }

       
        public virtual bool TryRead(Vector2Int pos, out float value)
        {
            return TryRead(pos.x, pos.y, out value);
        }
        
        public virtual bool Contains(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }


        public virtual bool Contains(Vector2Int pos)
        {
            return Contains(pos.x, pos.y);
        }
        
        public Vector2Int NormalizedToGridPos(Vector2 norm)
        {
            return new Vector2Int(
                (int) (norm.x * Width),
                (int) (norm.y * Height)
            );
        }
        
        public RectInt NormalizedToGridRect(Rect norm)
        {
            return new RectInt(
                (int) (norm.xMin * Width),
                (int) (norm.yMin * Height),
                (int) (norm.width * Width),
                (int) (norm.height * Height)
            );
        }
    }
}





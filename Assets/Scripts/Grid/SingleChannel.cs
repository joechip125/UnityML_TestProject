using System;
using System.Collections.Generic;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using UnityEngine;

namespace DefaultNamespace.Grid
{
  
    public class SingleChannel
    {
        public Vector2Int Size
        {
            get { return new Vector2Int(SizeX, SizeZ); }
            set
            {
                SizeX = value.x;
                SizeZ = value.y;
            }
        }

        public int ChannelIndex
        {
            get => _channelIndex;
            set => _channelIndex = value;
        }

        private int _channelIndex;
        
        public override string ToString()
        {
            return $"Grid {SizeX} x {SizeZ}";
        }
        
        public int SizeX
        {
            get { return _mSizeX; }
            set
            {
                _mSizeX = value;
                Initialize();
            }
        }

        private int _mSizeX;
        
        public int SizeZ
        {
            get { return _mSizeZ; }
            set
            {
                _mSizeZ = value;
                Initialize();
            }
        }

        private int _mSizeZ;

        //[y * width + x]
        private float[] m_Values;

        //public HashSet<Vector2> m_GridPositions;
        
        public SingleChannel(int sizeX, int sizeZ, int channelIndex)
        {
            _mSizeX = sizeX;
            _mSizeZ = sizeZ;
            ChannelIndex = channelIndex;

            Initialize();
        }

        public SingleChannel(Vector2Int size, int channelIndex)
            : this(size.x, size.y, channelIndex)
        {
        }
        
        protected virtual void Initialize()
        {
            m_Values = new float[SizeX * SizeZ];
        }
        
        public virtual void Clear()
        {
            Array.Clear(m_Values, 0, m_Values.Length);
        }
        
        public virtual void Write(int x, int y, float value)
        {
            m_Values[y * SizeX + x] = value;
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
            return m_Values[y * SizeX + x];
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
            return x >= 0 && x < SizeX && y >= 0 && y < SizeZ;
        }


        public virtual bool Contains(Vector2Int pos)
        {
            return Contains(pos.x, pos.y);
        }
        
        public Vector2Int NormalizedToGridPos(Vector2 norm)
        {
            return new Vector2Int(
                (int) (norm.x * SizeX),
                (int) (norm.y * SizeZ)
            );
        }
        
        public RectInt NormalizedToGridRect(Rect norm)
        {
            return new RectInt(
                (int) (norm.xMin * SizeX),
                (int) (norm.yMin * SizeZ),
                (int) (norm.width * SizeX),
                (int) (norm.height * SizeZ)
            );
        }
    }
}





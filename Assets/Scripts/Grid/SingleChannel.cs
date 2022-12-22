using System;
using System.Collections.Generic;
using System.Linq;
using MBaske.Sensors.Grid;
using Unity.MLAgents;
using UnityEngine;

namespace DefaultNamespace.Grid
{
  
    public class SingleChannel
    {
        public Vector2Int Size
        {
            get => new Vector2Int(SizeX, SizeZ);
            set
            {
                SizeX = value.x;
                SizeZ = value.y;
            }
        }

        public int ChannelIndex { get; set; }

        public override string ToString()
        {
            return $"Grid {SizeX} x {SizeZ}";
        }
        
        public int SizeX
        {
            get => _mSizeX;
            set
            {
                _mSizeX = value;
                Initialize();
            }
        }

        private int _mSizeX;
        
        public int SizeZ
        {
            get => _mSizeZ;
            set
            {
                _mSizeZ = value;
                Initialize();
            }
        }

        private int _mSizeZ;

        private float[] m_Values;
        public int SmallGridSize;
        public Vector3Int MinorMin;
        private Stack<Vector3Int> _pastIndex = new();
        private Stack<int> _pastSizes = new();


        public int CountChannel()
        {
            return m_Values.Count(x => x > 0);
        }
        
        
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
        
        public virtual void Write(int x, int z, float value)
        {
            m_Values[z * SizeX + x] = value;
        }
        
        public virtual void Write(Vector2Int pos, float value)
        {
            Write(pos.x, pos.y, value);
        }

        public virtual void Write(int index, float value)
        {
            m_Values[index] = value;
        }
        
        public virtual bool TryWrite(int x, int z, float value)
        {
            bool hasPosition = Contains(x, z);
            if (hasPosition)
            {
                Write(x, z, value);
            }

            return hasPosition;
        }
        
        public virtual bool TryWrite( Vector2Int pos, float value)
        {
            return TryWrite(pos.x, pos.y, value);
        }
        
        public virtual float Read( int x, int z)
        {
            return m_Values[z * SizeX + x];
        }

        public virtual float Read(int index)
        {
            return m_Values[index];
        }
        
        public virtual float Read(Vector2Int pos)
        {
            return Read( pos.x, pos.y);
        }
        
        public virtual bool TryRead(int x, int z, out float value)
        {
            bool hasPosition = Contains(x, z);
            value = hasPosition ? Read( x, z) : 0;
            return hasPosition;
        }

       
        public virtual bool TryRead(Vector2Int pos, out float value)
        {
            return TryRead(pos.x, pos.y, out value);
        }
        
        public virtual bool Contains(int x, int z)
        {
            return x >= 0 && x < SizeX && z >= 0 && z < SizeZ;
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

        public void ResetMinorGrid()
        {
            SmallGridSize = SizeX;
            MinorMin = Vector3Int.zero;
            _pastIndex.Clear();
            _pastSizes.Clear();
        }

        private void RevertMinorGrid()
        {
            if (_pastIndex.Count <= 0) return;
            
            MinorMin = _pastIndex.Pop();
            SmallGridSize = _pastSizes.Pop();
        }
        
        public bool GetNewGridShape(int index)
        {
            Clear();
            var stepX = 0;
            var stepZ = 0;
            
            switch (index)
            {
                case 0:
                    break;
                case 1:
                    stepX = 1;
                    break;
                case 2:
                    stepZ = 1;
                    break;
                case 3:
                    stepX = 1;
                    stepZ = 1;
                    break;
                case 4:
                   // RevertMinorGrid();
                    break;
            }

            if (index != 4)
            {
                if (SmallGridSize % 2 != 0)
                {
                    SmallGridSize -= 1;
                    MinorMin += new Vector3Int(stepX, 0, stepZ);
                }
                else
                {
                    SmallGridSize /= 2;
                    MinorMin += new Vector3Int(stepX * SmallGridSize, 0, stepZ * SmallGridSize);
                }
                //_pastIndex.Push(MinorMin);
                //_pastSizes.Push(SmallGridSize);
            }
            
            DrawToGrid(MinorMin, SmallGridSize, 0.5f);
           
            return SmallGridSize != 1;
        }

        private void DrawToGrid(Vector3Int start, int size, float drawValue, bool setOrAdd = false)
        {
            var numX = size;
            var numZ = size;
            var xCount = start.x;
            var zCount = start.z;
            
            for (int z = 0; z < numZ; z++)
            {
                for (int x = 0; x < numX; x++)
                {
                    if (xCount < 0 || xCount >= SizeX)
                    {
                        xCount++;
                        continue;
                    }

                    if (zCount < 0 || zCount >= SizeZ)
                    {
                        break;
                    }
                    
                    if (setOrAdd)
                    {
                        Write(xCount, zCount, drawValue);
                    }
                    else
                    {
                        Write(xCount, zCount, Mathf.Clamp(Read(xCount, zCount) + drawValue,0 ,1));
                    }
                
                    xCount++;
                }
                
                xCount = start.x;
                zCount++;
            }
        }
    }
}





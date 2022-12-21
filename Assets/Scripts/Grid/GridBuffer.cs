using UnityEngine;
using Unity.MLAgents;
using System;
using System.Linq;

namespace MBaske.Sensors.Grid
{
    /// <summary>
    /// 3D data structure for storing float values.
    /// Dimensions: channels x width x height.
    /// </summary>
    public class GridBuffer
    {
        /// <summary>
        /// Grid shape.
        /// </summary>
        [Serializable]
        public struct Shape
        {
            /// <summary>
            /// The number of grid channels.
            /// </summary>
            public int NumChannels;

            /// <summary>
            /// The width of the grid.
            /// </summary>
            public int SizeX;

            /// <summary>
            /// The height of the grid.
            /// </summary>
            public int SizeZ;

            /// <summary>
            /// The grid size as Vector2Int.
            /// </summary>
            public Vector2Int Size
            {
                get { return new Vector2Int(SizeX, SizeZ); }
                set { SizeX = value.x; SizeZ = value.y; }
            }

            /// <summary>
            /// Creates a <see cref="Shape"/> instance.
            /// </summary>
            /// <param name="numChannels">Number of grid channels</param>
            /// <param name="sizeX">Grid width</param>
            /// <param name="sizeZ">Grid height</param>
            public Shape(int numChannels, int sizeX, int sizeZ)
            {
                NumChannels = numChannels;
                SizeX = sizeX;
                SizeZ = sizeZ;
            }

            /// <summary>
            /// Creates a <see cref="Shape"/> instance.
            /// </summary>
            /// <param name="numChannels">Number of grid channels</param>
            /// <param name="size">Grid size</param>
            public Shape(int numChannels, Vector2Int size)
                : this(numChannels, size.x, size.y) { }

            /// <summary>
            /// Validates the <see cref="Shape"/>.
            /// </summary>
            public void Validate()
            {
                if (NumChannels < 1)
                {
                    throw new UnityAgentsException("Grid buffer has no channels.");
                }

                if (SizeX < 1)
                {
                    throw new UnityAgentsException("Invalid grid buffer width " + SizeX);
                }

                if (SizeZ < 1)
                {
                    throw new UnityAgentsException("Invalid grid buffer height " + SizeZ);
                }
            }

            public override string ToString()
            {
                return $"Grid {NumChannels} x {SizeX} x {SizeZ}";
            }
        }

        /// <summary>
        /// Returns a new <see cref="Shape"/> instance.
        /// </summary>
        /// <returns>Grid shape</returns>
        public Shape GetShape()
        {
            return new Shape(m_NumChannels, _mSizeX, _mSizeZ);
        }

        /// <summary>
        /// The number of grid channels.
        /// </summary>
        public int NumChannels
        {
            get { return m_NumChannels; }
            set { m_NumChannels = value; Initialize(); }
        }
        private int m_NumChannels;

        /// <summary>
        /// The width of the grid.
        /// </summary>
        public int SizeX
        {
            get { return _mSizeX; }
            set { _mSizeX = value; Initialize(); }
        }
        private int _mSizeX;

        /// <summary>
        /// The height of the grid.
        /// </summary>
        public int SizeZ
        {
            get { return _mSizeZ; }
            set { _mSizeZ = value; Initialize(); }
        }
        private int _mSizeZ;
        
        // [channel][y * width + x]
        private float[][] m_Values;

     
        public GridBuffer(int numChannels, int sizeX, int sizeZ)
        {
            m_NumChannels = numChannels;
            _mSizeX = sizeX;
            _mSizeZ = sizeZ;

            Initialize();
        }
        
        public GridBuffer(int numChannels, Vector2Int size)
            : this(numChannels, size.x, size.y) { }
        
        public GridBuffer(Shape shape)
            : this(shape.NumChannels, shape.SizeX, shape.SizeZ) { }


        protected virtual void Initialize()
        {
            m_Values = new float[NumChannels][];

            for (int i = 0; i < NumChannels; i++)
            {
                m_Values[i] = new float[SizeX * SizeZ];
            }
        }

        /// <summary>
        /// Clears all grid values by setting them to 0.
        /// </summary>
        public virtual void Clear()
        {
            ClearChannels(0, NumChannels);
            //IsDirty = false;
        }

        public virtual void Clear(int excludeChannel)
        {
            ClearChannels(0, NumChannels, excludeChannel);
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
        
        public virtual void ClearChannels(int start, int length, int excludeChanel)
        {
            for (int i = 0; i < length; i++)
            {
                if(i != excludeChanel)
                    ClearChannel(start + i);
            }
        }

        public int CountLayer(int channel, float minValue)
        {
            return m_Values[channel].Count(x => x > minValue);
        }

        public void GetQuadrant()
        {}

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
        /// <param name="z">The cell's y position</param>
        /// <param name="value">The value to write</param>
        public virtual void Write(int channel, int x, int z, float value)
        {
            m_Values[channel][z * SizeX + x] = value;
            //m_Values[channel][x * SizeZ + z] = value;
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

        public virtual void Write(int channel, int index, float value)
        {
            m_Values[channel][index] = value;
        }

        /// <summary>
        /// Tries to write a float value to a specified grid cell.
        /// </summary>
        /// <param name="channel">The cell's channel index</param>
        /// <param name="x">The cell's x position</param>
        /// <param name="z">The cell's y position</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if the specified cell exists, false otherwise</returns>
        public virtual bool TryWrite(int channel, int x, int z, float value)
        {
            bool hasPosition = Contains(x, z);
            if (hasPosition)
            {
                Write(channel, x, z, value);
            }
            return hasPosition;
        }

        /// <summary>
        /// Tries to write a float value to a specified grid cell.
        /// </summary>
        /// <param name="channel">The cell's channel index</param>
        /// <param name="pos">The cell's x/y position</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if the specified cell exists, false otherwise</returns>
        public virtual bool TryWrite(int channel, Vector2Int pos, float value)
        {
            return TryWrite(channel, pos.x, pos.y, value);
        }

        /// <summary>
        /// Reads a float value from a specified grid cell.
        /// </summary>
        /// <param name="channel">The cell's channel index</param>
        /// <param name="x">The cell's x position</param>
        /// <param name="z">The cell's y position</param>
        /// <returns>Float value of the specified cell</returns>
        public virtual float Read(int channel, int x, int z)
        {
            return m_Values[channel][z * SizeX + x];
        }

        public virtual float Read(int channel, int index)
        {
            return m_Values[channel][index];
        }

        public virtual void MaskSelection(int size, Vector2Int index, float value, int channel, bool addOrReplace = true)
        {
            
            var start = index - new Vector2Int(size, size);
            var end = index + new Vector2Int(size + 1, size + 1);
            var numX = end.x - start.x;
            var numZ = end.y - start.y;
            var xCount = start.x;
            var zCount = start.y;
            
            m_Values[channel][index.y * SizeX + index.x] = 1.0f;

            for (int z = 0; z < numZ; z++)
            {
                for (int x = 0; x < numX; x++)
                {
                    if (xCount < 0 )
                    {
                        xCount++;
                        continue;
                    }

                    if (zCount < 0 || zCount >= SizeZ || xCount >= SizeX) break;
                    
                    var newValue = value / Vector2Int.Distance(index, new Vector2Int(xCount, zCount));
                    var roundedValue = (float)Math.Round(newValue * 100f) / 100f;
                    
                    var theIndex = zCount * SizeX + xCount;
                    var indexValue = m_Values[channel][theIndex];

                    if (addOrReplace)
                    {
                        m_Values[channel][theIndex] = Mathf.Clamp(indexValue + roundedValue, 0, 1);
                    }
                    else
                    {
                        m_Values[channel][theIndex] = Mathf.Clamp(value, 0, 1);
                    }
                    
                    xCount++;
                }
                xCount = start.x;
                zCount++;
            }
        }


        public int ReadFromGrid(Vector3Int start, int size, int channel)
        {
            var numX = size;
            var numZ = size;
            var xCount = start.x;
            var zCount = start.z;
            var hitCount = 0;

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

                    var single = zCount * SizeX + xCount;
                    if (m_Values[channel][single] > 0)
                    {
                        hitCount++;
                    }
                    
                    xCount++;
                }
                xCount = start.x;
                zCount++;
            }

            return hitCount;
        }

        public int ReadSection(int startIndex, int endIndex, int channel)
        {
            var retInt = 0;
            
            for (int i = startIndex; i < endIndex; i++)
            {
                
                if (m_Values[channel][i] > 0)
                {
                    retInt++;
                }
                
            }

            return retInt;
        }

        public bool ReadAllChannelsAtIndex(int index, out int hitChannel, out float channelValue)
        {
            var outBool = false;
            hitChannel = 0;
            channelValue = 0;
            for (int i = 0; i < NumChannels; i++)
            {
                channelValue = m_Values[i][index];
                if (channelValue == 0 || i == 3) continue;
                
                outBool = true;
                hitChannel = i;
            }

            return outBool;
        }

        /// <summary>
        /// Reads a float value from a specified grid cell.
        /// </summary>
        /// <param name="channel">The cell's channel index</param>
        /// <param name="pos">The cell's x/y position</param>
        /// <returns>Float value of the specified cell</returns>
        public virtual float Read(int channel, Vector2Int pos)
        {
            return Read(channel, pos.x, pos.y);
            //return Read(channel, pos.y, pos.x);
        }

        /// <summary>
        /// Tries to read a float value from a specified grid cell.
        /// </summary>
        /// <param name="channel">The cell's channel index</param>
        /// <param name="x">The cell's x position</param>
        /// <param name="y">The cell's y position</param>
        /// <param name="value">The value of the specified cell if it exists, 0 otherwise</param>
        /// <returns>True if the specified cell exists, false otherwise</returns>
        public virtual bool TryRead(int channel, int x, int y, out float value)
        {
            bool hasPosition = Contains(x, y);
            value = hasPosition ? Read(channel, x, y) : 0;
            return hasPosition;
        }

        /// <summary>
        /// Tries to read a float value from a specified grid cell.
        /// </summary>
        /// <param name="channel">The cell's channel index</param>
        /// <param name="pos">The cell's x/y position</param>
        /// <param name="value">The value of the specified cell if it exists, 0 otherwise</param>
        /// <returns>True if the specified cell exists, false otherwise</returns>
        public virtual bool TryRead(int channel, Vector2Int pos, out float value)
        {
            return TryRead(channel, pos.x, pos.y, out value);
        }

        /// <summary>
        /// Checks if a specified position exists in the grid.
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        /// <returns>True if the specified position exists, false otherwise</returns>
        public virtual bool Contains(int x, int y)
        {
            return x >= 0 && x < SizeX && y >= 0 && y < SizeZ;
        }

        /// <summary>
        /// Checks if a specified position exists in the grid.
        /// </summary>
        /// <param name="pos">The x/y position</param>
        /// <returns>True if the specified position exists, false otherwise</returns>
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
                (int)(norm.x * SizeX),
                (int)(norm.y * SizeZ)
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
                (int)(norm.xMin * SizeX),
                (int)(norm.yMin * SizeZ),
                (int)(norm.width * SizeX),
                (int)(norm.height * SizeZ)
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
}

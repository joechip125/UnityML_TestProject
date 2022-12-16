using System;
using UnityEngine;

namespace Grid
{
    public class ChannelMask
    {
        // [channel][y * width + x]
        private float[] m_Values;
        private readonly int _sizeX;
        private readonly int _sizeZ;

        public ChannelMask(int sizeX, int sizeZ)
        {
            _sizeX = sizeX;
            _sizeZ = sizeZ;

            Initialize();
        }

        public ChannelMask(Vector2Int size)
            : this(size.x, size.y)
        {
        }
        
        protected virtual void Initialize()
        {
            m_Values = new float[_sizeX * _sizeZ];
        }
        
        public virtual void Clear()
        {
            Array.Clear(m_Values, 0, m_Values.Length);
        }
        
        public virtual void Write(int x, int z, float value)
        {
            m_Values[z * _sizeX + x] = value;
        }

        private void AddMask(int x, int z)
        {
            
        }
    }
}
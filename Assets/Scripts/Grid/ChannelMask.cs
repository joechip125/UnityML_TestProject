using System;
using UnityEngine;

namespace Grid
{
    public class ChannelMask
    {
        private float[] _channel;
        private readonly int _sizeX;
        private readonly int _sizeZ;
        public Vector2Int[] Indexes;

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
            _channel = new float[_sizeX * _sizeZ];
        }
        
        public virtual void Clear()
        {
            Array.Clear(_channel, 0, _channel.Length);
        }
        
        public virtual void Write(int x, int z, float value)
        {
            _channel[z * _sizeX + x] = value;
        }

        private void AddMask(Vector2Int index, int maskExtent)
        {
            
        }
    }
}
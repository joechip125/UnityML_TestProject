using UnityEngine;
using System.Linq;

namespace MBaske.Sensors.Grid
{
    /// <summary>
    /// Extends <see cref="GridBuffer"/>. Use this buffer variant for
    /// PNG compression of sensor observations, as it stores color values
    /// corresponding to grid positions.
    /// A layer contains three (color) channels. The number of layers is 
    /// equivalent to the number of textures encoded by the <see cref="GridSensor"/>.
    /// </summary>
    public class ColorGridBuffer : GridBuffer
    {
        protected int m_NumLayers;
        protected Color32[][] m_Colors;
        protected Color32[] c_White;

        /// <summary>
        /// Creates a <see cref="ColorGridBuffer"/> instance.
        /// </summary>
        /// <param name="numChannels">Number of grid channels</param>
        /// <param name="sizeX">Grid width</param>
        /// <param name="sizeZ">Grid height</param>
        public ColorGridBuffer(int numChannels, int sizeX, int sizeZ)
            : base(numChannels, sizeX, sizeZ) { }

        /// <summary>
        /// Creates a <see cref="ColorGridBuffer"/> instance.
        /// </summary>
        /// <param name="numChannels">Number of grid channels</param>
        /// <param name="size">Grid size</param>
        public ColorGridBuffer(int numChannels, Vector2Int size)
            : base(numChannels, size.x, size.y) { }

        /// <summary>
        /// Creates a <see cref="ColorGridBuffer"/> instance.
        /// </summary>
        /// <param name="shape"><see cref="GridBuffer.Shape"/> of the grid</param>
        public ColorGridBuffer(Shape shape) 
            : base(shape.NumChannels, shape.SizeX, shape.SizeZ) { }

        protected override void Initialize()
        {
            base.Initialize();

            m_NumLayers = Mathf.CeilToInt(NumChannels / 3f);
            m_Colors = new Color32[m_NumLayers][];

            for (int i = 0; i < m_NumLayers; i++)
            {
                m_Colors[i] = new Color32[SizeX * SizeZ];
            }

            c_White = Enumerable.Repeat(new Color32(255, 255, 255, 255 / 2), SizeX * SizeZ).ToArray();
            ClearColors();
        }

        /// <summary>
        /// Clears all grid values by setting them to 0. Sets all pixels to white.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ClearColors();
        }
        public override void Clear(int excludeChannel)
        {
            base.Clear(excludeChannel);
            ClearColors();
        }

        /// <summary>
        /// Clears grid values of a specified layer by setting them to 0. Sets layer pixels to black.
        /// <param name="layer">The layer index</param>
        /// </summary>
        public void ClearLayer(int layer)
        {
            int channel = layer * 3;
            base.ClearChannel(channel);
            base.ClearChannel(channel + 1);
            base.ClearChannel(channel + 2);
            ClearLayerColors(layer);
        }

        /// <summary>
        /// Clears grid values of specified channels by setting them to 0. Sets layer pixels to black.
        /// <param name="start">The first channel's index</param>
        /// <param name="length">The number of channels to clear</param>
        /// </summary>
        public override void ClearChannels(int start, int length)
        {
            int channel = start;
            int n = start + length;

            while (channel < n)
            {
                if (channel % 3 == 0 && channel < n - 1)
                {
                    // Faster than clearing individual channel colors.
                    ClearLayerColors(channel / 3);

                    base.ClearChannel(channel);
                    base.ClearChannel(channel + 1);
                    base.ClearChannel(channel + 2);
                    channel += 3;
                }
                else
                {
                    ClearChannel(channel);
                    channel++;
                }
            }
        }


        /// <summary>
        /// Clears grid values of a specified channel by setting them to 0. Sets channel's pixels' color to 0.
        /// <param name="channel">The channel index</param>
        /// </summary>
        public override void ClearChannel(int channel)
        {
            base.ClearChannel(channel);
            ClearChannelColors(channel);
        }

        /// <summary>
        /// Writes a float value to a specified grid cell and sets the corresponding pixel color.
        /// </summary>
        /// <param name="channel">The channel index</param>
        /// <param name="x">The x position of the cell</param>
        /// <param name="z">The y position of the cell</param>
        /// <param name="value">The value to write</param>
        public override void Write(int channel, int x, int z, float value)
        {
            base.Write(channel, x, z, value);

            int layer = channel / 3;
            int color = channel - layer * 3;
            // Bottom to top, left to right.
            m_Colors[layer][(SizeZ - z - 1) * SizeX + x][color] = (byte)(value * 255);
        }

        /// <inheritdoc/>
        public override int GetNumLayers()
        {
            return m_NumLayers;
        }

        /// <inheritdoc/>
        public override Color32[][] GetLayerColors()
        {
            return m_Colors;
        }

        private void ClearColors()
        {
            for (int i = 0; i < m_NumLayers; i++)
            {
                ClearLayerColors(i);
            }
        }

        private void ClearLayerColors(int layer)
        {
            System.Array.Copy(c_White, m_Colors[layer], m_Colors[layer].Length);
        }

        private void ClearChannelColors(int channel)
        {
            int layer = channel / 3;
            int color = channel - layer * 3;

            for (int i = 0, n = m_Colors[layer].Length; i < n; i++)
            {
                m_Colors[layer][i][color] = 0;
            }
        }
    }
}

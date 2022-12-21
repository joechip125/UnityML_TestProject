using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Grid;
using MBaske.Sensors.Grid;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Profiling;

public class CustomGridSensor : ISensor, IDisposable
{
    private ObservationType _mObservationType = ObservationType.Default;

    string _mName;
    
    private List<ChannelLabel> _labels = new();

    SensorCompressionType m_CompressionType;

    ObservationSpec m_ObservationSpec;

    public OverlapChecker m_BoxOverlapChecker;
    private GridBuffer _gridBuffer;

    // Buffers
    Texture2D m_PerceptionTexture;
    
    private  ColorGridBuffer m_GridBuffer;

    private SingleChannel _externalChannel;

    private List<byte> m_CompressedObs;

    // Utility Constants Calculated on Init
    int m_NumCells;
    
    Vector3 m_CellCenterOffset;
    
    public event Action<int, int> MaskObjectDetected;
    
    public CustomGridSensor(
        string name,
        SensorCompressionType compression,
        ColorGridBuffer gridBuffer,
        List<ChannelLabel> labels,
        SingleChannel externChannel)
    {
        _mName = name;
        CompressionType = compression;
        
        _externalChannel = externChannel;
        
        _labels = labels;
        
        gridBuffer.GetShape().Validate();
        m_GridBuffer = gridBuffer;
        m_NumCells = m_GridBuffer.SizeZ * m_GridBuffer.SizeX;
        m_ObservationSpec = ObservationSpec.Visual(m_GridBuffer.SizeZ, m_GridBuffer.SizeX, m_GridBuffer.NumChannels, _mObservationType);
        
        HandleCompressionType();

        ResetGridBuffer();
    }

    public Texture2D GetPerceptionTexture()
    {
        return m_PerceptionTexture;
    }
    
    public SensorCompressionType CompressionType
    {
        get { return m_CompressionType; }
        set
        {
            if (!IsDataNormalized() && value == SensorCompressionType.PNG)
            {
                Debug.LogWarning($"Compression type {value} is only supported with normalized data. " +
                                 "The sensor will not compress the data.");
                return;
            }
            m_CompressionType = value;
        }
    }
    
    public void Reset() { }
    
    public void ResetGridBuffer()
    {
        m_GridBuffer.Clear(3);
        
        if (_externalChannel != null)
        {
            CombineChannels();
        }
    }

    private void CombineBuffers(GridBuffer readBuffer, GridBuffer writeBuffer)
    {
        for (var i = 0; i < m_NumCells; i++)
        {
            for (int j = 0; j < _labels.Count; j++)
            {
                writeBuffer.Write(j, i,  readBuffer.Read(j, i));
            }
        }
    }

    private void CombineChannels()
    {
        int channelIndex = _externalChannel.ChannelIndex;
        for (var i = 0; i < m_NumCells; i++)
        {
            m_GridBuffer.Write(channelIndex, i,  _externalChannel.Read(i));
        }
    }
    
    /// <inheritdoc/>
    public string GetName()
    {
        return _mName;
    }
    
    public CompressionSpec GetCompressionSpec()
    {
        return new CompressionSpec(CompressionType);
    }

    public BuiltInSensorType GetBuiltInSensorType()
    {
        return BuiltInSensorType.GridSensor;
    }
    
    protected void HandleCompressionType()
    {
        DestroyTexture();

        if (m_CompressionType == SensorCompressionType.PNG)
        {
            m_PerceptionTexture = new Texture2D(
                m_GridBuffer.SizeX, m_GridBuffer.SizeZ, TextureFormat.RGB24, false);
            m_CompressedObs = new List<byte>(
                m_GridBuffer.SizeX * m_GridBuffer.SizeZ * m_GridBuffer.NumChannels);
        }
    }

    /// <inheritdoc/>
    public byte[] GetCompressedObservation()
    {
        m_CompressedObs.Clear();
    
        var colors = m_GridBuffer.GetLayerColors();
        for (int i = 0, n = colors.Length; i < n; i++)
        {
            m_PerceptionTexture.SetPixels32(colors[i]);
            m_CompressedObs.AddRange(m_PerceptionTexture.EncodeToPNG());
        }
    
        return m_CompressedObs.ToArray();
    }
    
    protected virtual bool IsDataNormalized()
    {
        return true;
    }


    protected internal virtual ProcessCollidersMethod GetProcessCollidersMethod()
    {
        return ProcessCollidersMethod.ProcessClosestColliders;
    }


    /// <summary>
    /// Collect data from the detected object if a detectable tag is matched.
    /// </summary>

    public void ProcessObjectGridBuffer(GameObject detectedObject, int cellIndex)
    {
        Profiler.BeginSample("GridSensor.ProcessDetectedObject");

        for (var i = 0; i < _labels.Count; i++)
        {
            if (!ReferenceEquals(detectedObject, null) && detectedObject.CompareTag(_labels[i].Name))
            {
                m_GridBuffer.Write(i, cellIndex,1);
                if (_labels[i].maskThis)
                {
                    MaskObjectDetected?.Invoke(cellIndex, i);
                }
            }
        }
        Profiler.EndSample();
    }
    
    /// <inheritdoc/>
    public ObservationSpec GetObservationSpec()
    {
        return m_ObservationSpec;
    }

    /// <inheritdoc/>
    public int Write(ObservationWriter writer)
    {
        int numWritten = 0;
        int w = m_GridBuffer.SizeX;
        int h = m_GridBuffer.SizeZ;
        int n = m_GridBuffer.NumChannels;
    
        for (int c = 0; c < n; c++)
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    writer[y, x, c] = m_GridBuffer.Read(c, x, y);
                    numWritten++;
                }
            }
        }
        return numWritten;
    }

    /// <inheritdoc/>
    public void Update()
    {
        ResetGridBuffer();

        m_BoxOverlapChecker?.Update();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        DestroyTexture();   
    }

    private void DestroyTexture()
    {
        if (ReferenceEquals(null, m_PerceptionTexture)) return;
        
        if (Application.isEditor)
        {
            UnityEngine.Object.DestroyImmediate(m_PerceptionTexture);
        }
        else
        {
            UnityEngine.Object.Destroy(m_PerceptionTexture);
        }
        m_PerceptionTexture = null;
    }
}

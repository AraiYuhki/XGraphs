using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XGraph.Model;
using Xeon.XGraph.Manager;
using Xeon.XGraph.Rendering;
using Xeon.Common;

#if UNITY_EDITOR
using UnityEditor;
using Xeon.XGraph.Editor;
#endif

namespace Xeon.XGraph.View
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class LineChart : ChartBase
    {
        [SerializeField]
        private float thickness = 1f;
        [SerializeField, Tooltip("thicknessの何倍までマイターを許容するか")]
        private float miterLimit = 4f;

        [SerializeField]
        private RangeValue range = new(0f, 100f);

        [SerializeField]
        private ScaleMarkerManager markerManager = new();

        [SerializeField]
        private MultiValueSeries buffer = new();

        private float halfWidth => thickness * 0.5f;

        private readonly UILineRenderer lineRenderer = new();

        private float[,] normalizedValues = new float[0, 0];
        private Vector2[] pointsBuffer = new Vector2[0];

        private bool rangeDirty = false;
        private float scale = 1f;
        private float baseY = 0f;

        private List<UIVertex> vertices;
        private List<int> indices;

        public ScaleMarkerManager MarkerManager => markerManager;

        public void Initialize(MultiValueSeries series, List<ScaleMarkerData> markers = null)
        {
            rangeDirty = true;
            markerManager.Initialize(ValueToHeight);
            if (markers != null)
                markerManager.SetMarkers(markers);
            SetSeries(series);
            if (legendListView != null)
                legendListView.Setup(buffer.GetLegends());
        }

        public void SetMax(float max)
        {
            range.Max = max;
            rangeDirty = true;
            geometryDirty = true;
            markerManager.UpdateMarkers();
            SetVerticesDirty();
        }

        public void SetMin(float min)
        {
            range.Min = min;
            rangeDirty = true;
            geometryDirty = true;
            markerManager.UpdateMarkers();
            SetVerticesDirty();
        }

        public void SetValues(MultiValueSeries newValues) => SetSeries(newValues);

        public void SetSeries(MultiValueSeries newSeries)
        {
            FinalizeSeries(buffer);
            buffer = newSeries;
            InitializeSeries(buffer);
            buffer?.SetColor(color);
            rangeDirty = true;
            geometryDirty = true;
            SetVerticesDirty();
        }

        protected override void OnEnable()
        {
            rangeDirty = true;
            buffer?.SetColor(color);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            rangeDirty = true;
            buffer?.SetColor(color);
            base.OnDisable();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (buffer == null || buffer.Count <= 0 || !buffer.HasSeries)
                return;

            var legendCount = buffer.Count;
            if (legendCount <= 0)
                return;

            var dataCount = GetDataCount();
            if (dataCount <= 1)
                return;

            if (rangeDirty)
                RecalculateRange();

            EnsureCapacity(legendCount, dataCount);

            if (geometryDirty || vertices == null || indices == null || vertices.Count == 0)
            {
                UpdateNormalizedValues(legendCount, dataCount);
                BuildGeometry(legendCount, dataCount);
                geometryDirty = false;
            }

            if (vertices != null && indices != null && vertices.Count > 0 && indices.Count > 0)
                vh.AddUIVertexStream(vertices, indices);
        }

#if UNITY_EDITOR
        private const string MenuName = "Line Chart";
        private const int MenuPriority = ChartMenuUtility.MenuPriorityRoot + 1;

        [MenuItem(ChartMenuUtility.MenuRoot + MenuName, false, MenuPriority)]
        private static void Create(MenuCommand menuCommand)
        {
            ChartMenuUtility.CreateChart<LineChart>(MenuName, menuCommand);
        }
#endif

        private int GetDataCount()
        {
            if (buffer == null || !buffer.HasSeries)
                return 0;

            var count = int.MaxValue;
            var hasSeries = false;
            foreach (var series in buffer)
            {
                if (series == null)
                    return 0;

                hasSeries = true;
                var seriesCount = series.Count;
                count = seriesCount < count ? seriesCount : count;
            }

            if (!hasSeries || count == int.MaxValue)
                return 0;

            return count;
        }

        private void EnsureCapacity(int legendCount, int dataCount)
        {
            var bufferSize = Mathf.Max(0, legendCount * dataCount);
            var vertexCapacity = Mathf.Max(0, bufferSize * 4);
            var indexCapacity = Mathf.Max(0, bufferSize * 6);

            vertices ??= new List<UIVertex>(vertexCapacity);
            if (vertices.Capacity < vertexCapacity)
                vertices.Capacity = vertexCapacity;

            indices ??= new List<int>(indexCapacity);
            if (indices.Capacity < indexCapacity)
                indices.Capacity = indexCapacity;

            if (pointsBuffer.Length < dataCount)
                pointsBuffer = new Vector2[dataCount];
        }

        private void UpdateNormalizedValues(int legendCount, int dataCount)
        {
            if (legendCount <= 0 || dataCount <= 0)
            {
                normalizedValues = new float[0, 0];
                return;
            }

            if (normalizedValues == null ||
                normalizedValues.GetLength(0) != legendCount ||
                normalizedValues.GetLength(1) != dataCount)
            {
                normalizedValues = new float[legendCount, dataCount];
            }

            for (var legendIndex = 0; legendIndex < legendCount; legendIndex++)
            {
                var series = buffer[legendIndex];
                for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
                    normalizedValues[legendIndex, dataIndex] = FastValueToHeight(series[dataIndex]);
            }
        }

        private void BuildGeometry(int legendCount, int dataCount)
        {
            vertices.Clear();
            indices.Clear();

            var rect = rectTransform.rect;
            var offsetX = rect.xMin + padding.left;
            var contentWidth = Mathf.Max(0f, width);
            var stepX = dataCount <= 1 ? 0f : contentWidth / (dataCount - 1f);

            lineRenderer.Thickness = thickness;
            lineRenderer.MiterLimit = miterLimit;

            for (var legendIndex = 0; legendIndex < legendCount; legendIndex++)
            {
                for (var dataIndex = 0; dataIndex < dataCount; dataIndex++)
                    pointsBuffer[dataIndex] = new Vector2(stepX * dataIndex + offsetX, normalizedValues[legendIndex, dataIndex]);

                var seriesColor = buffer.GetSegmentColor(legendIndex, color);
                lineRenderer.AddLine(pointsBuffer, dataCount, seriesColor, vertices, indices);
            }
        }

        protected override void OnChangedColor()
        {
            buffer?.SetColor(color);
            base.OnChangedColor();
        }

        private void RecalculateRange()
        {
            if (range.IsApproximately)
            {
                scale = 0f;
                baseY = rectTransform.rect.yMin + padding.bottom;
            }
            else
            {
                var usableHeight = rectTransform.rect.height - padding.vertical;
                scale = usableHeight / range.Range;
                baseY = rectTransform.rect.yMin + padding.bottom - range.Min * scale;
            }
            rangeDirty = false;
        }

        private float FastValueToHeight(float value)
        {
            if (rangeDirty)
                RecalculateRange();
            if (scale == 0f)
                return rectTransform.rect.yMin + padding.bottom + halfWidth;
            return baseY + value * scale;
        }

        private float ValueToHeight(float value, out bool isInRange)
        {
            isInRange = range.InRange(value);
            return FastValueToHeight(value);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            rangeDirty = true;
            base.OnRectTransformDimensionsChange();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
                return;
            markerManager.Initialize(ValueToHeight);
            buffer?.SetColor(color);
            rangeDirty = true;
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Xeon.Common;
using Xeon.XGraph.Model;
using Xeon.XGraph.View;

namespace Xeon.XGraph.Sample
{
    public class GraphSample : MonoBehaviour
    {
        private const int FrameBufferSize = 400;

        private static readonly List<ScaleMarkerData> FPSMarkerDataList = new List<ScaleMarkerData>()
        {
            new ("15FPS", 0.0667f),
            new ("30FPS", 0.0333f),
            new ("60FPS", 0.0167f),
            new ("90FPS", 0.0111f),
            new ("120FPS", 0.0083f)
        };

        [SerializeField]
        private LineChart lineChart;
        [SerializeField]
        private StackedBarChart stackedBarChart;
        [SerializeField]
        private BarChart barChart;
        [SerializeField]
        private PieChart pieChart;
        [SerializeField]
        private RingChart ringChart;

        private float fps = 0f;
        private double updateDuration;
        private double renderStartTime;
        private double renderDuration;
        private float lastRefreshedTime = 0f;
        private float lastMax = float.NaN;

        private readonly Stopwatch stopwatch = new();
        private Coroutine endOfFrameCoroutineHandler = null;

        private MultiValueSeries timeBuffer;
        private Series fpsData;
        private DoubleCircularBuffer totalTimeBuffer = new(FrameBufferSize);

        private SingleValueSeries infoSeries;
        private SingleValueSeries warningSeries;
        private SingleValueSeries errorSeries;

        private MultiLegendSeries times;

        public void Awake()
        {
#if UNITY_6000_3_OR_NEWER
            timeBuffer = new MultiValueSeries(new List<Series>()
            {
                new Series("UpdateTime", Color.darkSeaGreen, new CircularBuffer<float>(FrameBufferSize, true)),
                new Series("RenderTime", Color.cadetBlue, new CircularBuffer<float>(FrameBufferSize, true)),
                new Series("OtherTime", Color.burlywood, new CircularBuffer<float>(FrameBufferSize, true))
            }, Color.white);
#else
            timeBuffer = new MultiValueSeries(new List<Series>()
            {
                new Series("UpdateTime", Color.green, new CircularBuffer<float>(FrameBufferSize, true)),
                new Series("RenderTime", Color.blue, new CircularBuffer<float>(FrameBufferSize, true)),
                new Series("OtherTime", Color.yellow, new CircularBuffer<float>(FrameBufferSize, true))
            }, Color.white);
#endif

            RenderPipelineManager.beginContextRendering += RenderPipelineOnBeginFrameRendering;
            endOfFrameCoroutineHandler = StartCoroutine(EndOfFrameCoroutine());

            lineChart.Initialize(timeBuffer);
            lineChart.MarkerManager.SetMarkers(FPSMarkerDataList);

            stackedBarChart.Initialize(timeBuffer);
            stackedBarChart.MarkerManager.SetMarkers(FPSMarkerDataList);
#if UNITY_6000_3_OR_NEWER
            fpsData = new Series("FPS", Color.blueViolet, new CircularBuffer<float>(FrameBufferSize, true));
#else
            fpsData = new Series("FPS", Color.magenta, new CircularBuffer<float>(FrameBufferSize, true));
#endif

            barChart.Initialize(new MultiValueSeries(new List<Series>() { fpsData }, Color.white));
            barChart.SetMax(200f);

            infoSeries = new SingleValueSeries("Info count", Color.cyan, 0f);
            warningSeries = new SingleValueSeries("Warning count", Color.gray, 0f);
            errorSeries = new SingleValueSeries("Error count", Color.black, 0f);

#if UNITY_6000_3_OR_NEWER
            times = new MultiLegendSeries(new[] {
                new SingleValueSeries("UpdateTime", Color.darkSeaGreen, 0f),
                new SingleValueSeries("RenderTime", Color.cadetBlue, 0f),
                new SingleValueSeries("OtherTime", Color.burlywood, 0f)
            });
#else
            times = new MultiLegendSeries(new[] {
                new SingleValueSeries("UpdateTime", Color.green, 0f),
                new SingleValueSeries("RenderTime", Color.blue, 0f),
                new SingleValueSeries("OtherTime", Color.yellow, 0f)
            });
#endif
            pieChart.Initialize(new MultiLegendSeries(new[] { infoSeries, warningSeries, errorSeries }));
            ringChart.Initialize(new LayeredSeries(new[] { times }));
        }

        private IEnumerator EndOfFrameCoroutine()
        {
            var endOfFrame = new WaitForEndOfFrame();
            while (true)
            {
                yield return endOfFrame;
                renderDuration = stopwatch.Elapsed.TotalSeconds - renderStartTime;
            }
        }

        private void Update()
        {
            EndFrame();

            fps = 1f / Time.deltaTime;
            fpsData.Add(fps);
            stopwatch.Start();

            if (Time.realtimeSinceStartup - lastRefreshedTime > 1f)
            {
                switch (Random.Range(0, 3))
                {
                    case 0:
                        infoSeries.Value += 1f;
                        break;
                    case 1:
                        warningSeries.Value += 1f;
                        break;
                    case 2:
                        errorSeries.Value += 1f;
                        break;
                }
                lastRefreshedTime = Time.realtimeSinceStartup;
            }
        }

        private void LateUpdate()
        {
            updateDuration = stopwatch.Elapsed.TotalSeconds;
        }

        private void PushFrameData(double totalTime, double updateTime, double renderTime)
        {
            totalTimeBuffer.PushBack(totalTime);
            // 最大値が変化したときだけレンジとマーカーを更新する
            var max = (float)totalTimeBuffer.Max * 1.1f;
            if (!Mathf.Approximately(max, lastMax))
            {
                lineChart.SetMax(max);
                stackedBarChart.SetMax(max);
                lastMax = max;
            }
            timeBuffer.AddValue((float)updateTime, (float)renderTime, (float)(totalTime - updateTime - renderTime));

            times[0].Value = (float)updateTime;
            times[1].Value = (float)renderTime;
            times[2].Value = (float)(totalTime - updateTime - renderTime);
        }

        private void EndFrame()
        {
            if (stopwatch.IsRunning)
            {
                PushFrameData(stopwatch.Elapsed.TotalSeconds, updateDuration, renderDuration);
                stopwatch.Reset();
                stopwatch.Start();
            }
            updateDuration = renderDuration = 0;
        }

        private void RenderPipelineOnBeginFrameRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            renderStartTime = stopwatch.Elapsed.TotalSeconds;
        }
    }
}

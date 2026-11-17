# XGraphs

XGraphs is a collection of Unity uGUI components for drawing various types of charts. It offers data-series abstractions and editor tooling so you can build charts quickly both at runtime and in the editor.

## Features
- Includes uGUI components such as bar charts, stacked bar charts, line charts, pie charts, ring charts, and radar charts.
- Flexible data models like `Series` and `MultiValueSeries` make it easy to represent multiple legends and datasets.
- Supports diverse data sources via adapters for arrays, `List<T>`, `ObservableCollection<T>`, `CircularBuffer<T>`, and more.
- Provides marker management for bar, stacked bar, and line charts so you can place labeled lines at arbitrary values.
- Custom property drawers for multi-value series let you edit legend names, colors, and sample data directly from the inspector.

## Requirements
- Unity 6000.0 or newer (see the `unity` field in `package.json`).
- TextMeshPro is recommended because certain marker UIs rely on it.

## Installation
1. Clone (or download) this repository.
2. In the Unity Editor, open **Window > Package Manager**, then select **Add package from disk...** from the `+` menu.
3. Choose this package's `package.json` to add the `jp.xeon.x-graphs` package to your project.

## Running the Sample
This repository is itself a Unity project, so you can view the sample scene just by cloning it.

1. Clone the repository.
   ```
   git clone https://github.com/AraiYuhki/XGraphs.git
   ```
2. In Unity Hub, choose **Add > Add project from disk** and select the cloned folder (see `ProjectSettings/ProjectVersion.txt` for the Unity version used for development; the first open takes a while to generate Library and import TextMeshPro).
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press Play. `Assets/Scripts/GraphSample.cs` measures frame times (update / render / other) and log counts in real time and shows them as a line chart, stacked bar chart, bar chart, pie chart, and ring chart. The line chart also shows FPS guide lines (15 / 30 / 60 / 90 / 120 FPS) as markers.

The sample code is a useful reference for initializing charts, updating data, and setting markers. The package itself lives in `Packages/jp.xeon.x-graphs/`.

> To install it as a package via Git URL, use `https://github.com/AraiYuhki/XGraphs.git?path=/Packages/jp.xeon.x-graphs`.

## Bundled Components
| Component | Description |
| --- | --- |
| `BarChart` | Multi-legend bar chart. Configure value ranges (`RangeValue`), bar spacing, and markers. |
| `StackedBarChart` | Stacked bar chart that shares range and marker settings across legends. |
| `LineChart` | Polyline chart that supports miter/bevel joins, custom widths, ranges, and multiple series. |
| `PieChart` | Single-layer pie chart with a configurable start angle, rendering a `MultiLegendSeries` of `SingleValueSeries`. |
| `RingChart` | Multi-layer ring chart built from `LayeredSeries` objects. |
| `RaderChart` | Radar chart with support for multiple legends, normalized by start angle and maximum value. |

## Data Models & Adapters
- `Series` represents sequential data for one legend and wraps various containers via adapters such as `ArraySeriesDataAdapter` and `ListSeriesDataAdapter`.
- `MultiValueSeries` holds two-dimensional data for multiple legends and can apply theme colors by multiplying legend colors.
- `SingleValueSeries` provides scalar values for pie and ring charts.
- Combine `MultiLegendSeries` with `LayeredSeries` to build ring-style charts where each layer has its own legends.
- Use `CircularBuffer<T>` or `DoubleCircularBuffer` if you need buffered, rolling data.

## Quick Start
1. Create an empty GameObject under a Canvas and add the `BarChart` component.
2. Construct a `MultiValueSeries` in your script and pass it to `BarChart.Initialize` or `SetSeries`.

```csharp
using System.Collections.Generic;
using UnityEngine;
using Xeon.Common;
using Xeon.XGraph.Model;
using Xeon.XGraph.View;

public class BarChartSample : MonoBehaviour
{
    [SerializeField] private BarChart chart;

    private void Start()
    {
        var sales = new Series("Sales", Color.cyan, new[] { 80f, 120f, 100f });
        var profit = new Series("Profit", Color.green, new[] { 40f, 60f, 55f });
        var multi = new MultiValueSeries(new List<Series> { sales, profit }, Color.white);
        chart.Initialize(multi, new List<ScaleMarkerData> { new("Target", 100f) });
    }
}
```

3. Call `chart.SetMin` / `SetMax` to adjust the value range. Pass markers as the second argument of `Initialize`; on `LineChart` and `StackedBarChart` you can also add them later with `chart.MarkerManager.AddMarker`.

## Editor Tooling
- The `MultiValueSeries` property drawer lets you fold sections, adjust legend counts, and edit sample data directly in the inspector.
- `ScaleMarkerManager` ships with a custom property drawer so you can configure markers from the inspector.

## License
Please contact the repository maintainer regarding the license for this package.

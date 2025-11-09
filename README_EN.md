# XGraphs

XGraphs is a collection of Unity uGUI components for drawing various types of charts. It offers data-series abstractions and editor tooling so you can build charts quickly both at runtime and in the editor.

## Features
- Includes uGUI components such as bar charts, stacked bar charts, line charts, pie charts, ring charts, and radar charts.
- Flexible data models like `Series` and `MultiValueSeries` make it easy to represent multiple legends and datasets.
- Supports diverse data sources via adapters for arrays, `List<T>`, `ObservableCollection<T>`, `CircularBuffer<T>`, and more.
- Provides marker management for bar charts so you can place labeled lines at arbitrary values.
- Custom property drawers for multi-value series let you edit legend names, colors, and sample data directly from the inspector.

## Requirements
- Unity 2022.3.6 or newer (see the `unity` field in `package.json`).
- TextMeshPro is recommended because certain marker UIs rely on it.

## Installation
1. Clone (or download) this repository.
2. In the Unity Editor, open **Window > Package Manager**, then select **Add package from disk...** from the `+` menu.
3. Choose this package's `package.json` to add the `jp.xeon.xgraphs` package to your project.

## Bundled Components
| Component | Description |
| --- | --- |
| `BarChart` | Multi-legend bar chart. Configure value ranges (`RangeValue`), bar spacing, and markers. |
| `StackedBarChart` | Stacked bar chart that shares range and marker settings across legends. |
| `LineChart` | Polyline chart that supports miter/bevel joins, custom widths, ranges, and multiple series. |
| `PieChart` | Single-layer pie chart rendering a `SingleValueSeries` with configurable start angles and segment splitting. |
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
using UnityEngine;
using Xeon.XGraph.Model;
using Xeon.XGraph.View;

public class BarChartSample : MonoBehaviour
{
    [SerializeField] private BarChart chart;

    private void Start()
    {
        var sales = new Series("Sales", Color.cyan, new[] { 80f, 120f, 100f });
        var profit = new Series("Profit", Color.green, new[] { 40f, 60f, 55f });
        var multi = new MultiValueSeries(new System.Collections.Generic.List<Series> { sales, profit }, Color.white);
        chart.Initialize(multi);
    }
}
```

3. Call `chart.SetMin` / `SetMax` to adjust the value range, or use `chart.MarkerManager.AddMarker` to insert markers.

## Editor Tooling
- The `MultiValueSeries` property drawer lets you fold sections, adjust legend counts, and edit sample data directly in the inspector.
- `MarkerManager` ships with a custom property drawer so you can configure markers from the inspector.

## License
Please contact the repository maintainer regarding the license for this package.

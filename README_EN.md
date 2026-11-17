# XGraphs

[日本語](README.md)

![XGraphs sample scene](Documentation/images/XGraphsSampleDemo.gif)

A Unity package for drawing bar, line, pie, and other charts on uGUI. It provides data-series models, adapters for common data sources, and inspector tooling so you can build charts quickly both at runtime and in the editor.

This repository is a Unity project. The package itself lives in [`Packages/jp.xeon.x-graphs/`](Packages/jp.xeon.x-graphs/), alongside a sample scene.

## Features
- Bar, stacked bar, line, pie, ring, and radar charts as uGUI components.
- Data models such as `Series` and `MultiValueSeries` handle multiple legends and series.
- Arrays, `List<T>`, `ObservableCollection<T>`, and `CircularBuffer<T>` can be used directly as data sources through adapters.
- Bar, stacked bar, and line charts can show labeled scale lines (markers) at arbitrary values.
- Create any chart on a Canvas from the **GameObject > UI > XGraphs** menu.

## Requirements
- Unity 6000.0 or newer
- uGUI (`com.unity.ugui`)
- TextMeshPro (used for legend and marker labels)

## Installation
In Unity, open **Window > Package Manager**, choose **Install package from git URL...** from the `+` menu, and enter:

```
https://github.com/AraiYuhki/XGraphs.git?path=/Packages/jp.xeon.x-graphs
```

Or add it to `Packages/manifest.json` directly:

```json
{
  "dependencies": {
    "jp.xeon.x-graphs": "https://github.com/AraiYuhki/XGraphs.git?path=/Packages/jp.xeon.x-graphs"
  }
}
```

## Sample
1. Clone the repository.
   ```
   git clone https://github.com/AraiYuhki/XGraphs.git
   ```
2. In Unity Hub, choose **Add > Add project from disk** and open the cloned folder. The Unity version used for development is listed in `ProjectSettings/ProjectVersion.txt`.
3. Open `Assets/Scenes/SampleScene.unity` and press Play.

[`Assets/Scripts/GraphSample.cs`](Assets/Scripts/GraphSample.cs) measures frame times (update / render / other) and log counts in real time and shows them as line, stacked bar, bar, pie, and ring charts. The line and stacked bar charts show FPS guide lines (15 / 30 / 60 / 90 / 120 FPS) as markers.

## Quick Start
1. Create a bar chart on a Canvas with **GameObject > UI > XGraphs > Bar Chart**.
2. Build a `MultiValueSeries` in a script and pass it to `Initialize`.

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
        var series = new MultiValueSeries(new List<Series> { sales, profit }, Color.white);

        var markers = new List<ScaleMarkerData> { new("Target", 100f) };
        chart.Initialize(series, markers);
        chart.SetMax(150f);
    }
}
```

Add values with `Series.Add` or `MultiValueSeries.AddValue` and the chart redraws automatically. For real-time data, create the `Series` from a `CircularBuffer<float>` so the oldest values are dropped first (see `GraphSample.cs`).

## Components
| Component | Data | Description |
| --- | --- | --- |
| `BarChart` | `MultiValueSeries` | Multi-legend bar chart with configurable range, bar spacing, and markers. |
| `StackedBarChart` | `MultiValueSeries` | Bar chart that stacks values per legend. |
| `LineChart` | `MultiValueSeries` | Multi-series line chart with configurable thickness and miter limit. |
| `PieChart` | `MultiLegendSeries` | Pie chart with a configurable start angle. |
| `RingChart` | `LayeredSeries` | Ring chart that stacks `MultiLegendSeries` as layers. |
| `RaderChart` | `MultiValueSeries` | Multi-legend radar chart with a configurable start angle and maximum value. |

## Data Models
| Class | Purpose |
| --- | --- |
| `Series` | Sequential data for one legend. Wraps arrays, `List<T>`, `ObservableCollection<T>`, or `CircularBuffer<T>`. |
| `MultiValueSeries` | A set of `Series`. Used by bar, stacked bar, line, and radar charts. |
| `SingleValueSeries` | A legend with a name, a color, and a single value. |
| `MultiLegendSeries` | A set of `SingleValueSeries`. Used by pie charts. |
| `LayeredSeries` | A set of `MultiLegendSeries` as layers. Used by ring charts. |

See the package [README](Packages/jp.xeon.x-graphs/README_EN.md) for more details.

## How CircularBuffer Works
Real-time charts show only the latest N values and drop the oldest ones every frame. `CircularBuffer<T>` is a ring buffer built for this. It uses a fixed-size array and just two indices: `start` (the first element) and `end` (the next write position).

- Appending, dropping the oldest value, and reading the i-th value are all O(1). Elements are never shifted.
- The array is allocated once, so adding values every frame causes no GC allocations.
- Once the buffer is full, `start` and `end` move together across the array and wrap back to the beginning. Values never move once written.

Adding values to a buffer with capacity 4 works like this:

```mermaid
%%{init: {"theme": "base", "themeVariables": {"background": "#ffffff", "primaryColor": "#eceff1", "primaryTextColor": "#000000", "primaryBorderColor": "#607d8b", "lineColor": "#455a64", "clusterBkg": "#f5f5f5", "clusterBorder": "#9e9e9e", "titleColor": "#000000", "edgeLabelBackground": "#ffffff"}}}%%
flowchart TB
    subgraph S1["① Empty　Count = 0"]
        direction LR
        a0["[0]<br>　<br>start / end"] --- a1["[1]<br>　"] --- a2["[2]<br>　"] --- a3["[3]<br>　"]
        a3 -.->|wrap| a0
    end
    subgraph S2["② PushBack 10, 20, 30　Count = 3"]
        direction LR
        b0["[0]<br>10<br>start"] --- b1["[1]<br>20"] --- b2["[2]<br>30"] --- b3["[3]<br>　<br>end"]
        b3 -.->|wrap| b0
    end
    subgraph S3["③ PushBack 40 → full. end wraps to 0 and meets start　Count = 4"]
        direction LR
        c0["[0]<br>10<br>start / end"] --- c1["[1]<br>20"] --- c2["[2]<br>30"] --- c3["[3]<br>40"]
        c3 -.->|wrap| c0
    end
    subgraph S4["④ PushBack 50 → drop the oldest (10) and write to [0]. Contents: 20, 30, 40, 50"]
        direction LR
        d0["[0]<br>50"] --- d1["[1]<br>20<br>start / end"] --- d2["[2]<br>30"] --- d3["[3]<br>40"]
        d3 -.->|wrap| d0
    end
    subgraph S5["⑤ PushBack 60 → drop 20 and write to [1]. Contents: 30, 40, 50, 60"]
        direction LR
        e0["[0]<br>50"] --- e1["[1]<br>60"] --- e2["[2]<br>30<br>start / end"] --- e3["[3]<br>40"]
        e3 -.->|wrap| e0
    end
    S1 ~~~ S2 ~~~ S3 ~~~ S4 ~~~ S5


    classDef default fill:#eceff1,stroke:#607d8b,color:#000
    style S1 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    style S2 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    style S3 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    style S4 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    style S5 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    linkStyle default stroke:#455a64
    classDef startEnd fill:#c8e6c9,stroke:#2e7d32,stroke-width:3px,color:#000
    classDef start fill:#bbdefb,stroke:#1565c0,stroke-width:3px,color:#000
    classDef endSlot fill:#ffe0b2,stroke:#ef6c00,stroke-width:3px,color:#000
    class a0,c0,d1,e2 startEnd
    class b0 start
    class b3 endSlot
```

### How DoubleCircularBuffer Tracks the Maximum
`DoubleCircularBuffer` tracks the maximum value with a fixed-size segment tree. It is useful for scaling the vertical axis to the current maximum.

- Each leaf is a position in the array, and the root `tree[1]` holds the overall maximum. Reading `Max` just reads the root, so it is O(1).
- Writing a value updates the tree from the leaf toward the root in O(log n). If a parent's value does not change, the update stops there.
- The tree is a single `double[]` of length capacity × 2, so adding and removing values causes no allocations.

Orange marks the nodes that were rewritten, and the gray dashed border marks where the update stopped.

```mermaid
%%{init: {"theme": "base", "themeVariables": {"background": "#ffffff", "primaryColor": "#eceff1", "primaryTextColor": "#000000", "primaryBorderColor": "#607d8b", "lineColor": "#455a64", "clusterBkg": "#f5f5f5", "clusterBorder": "#9e9e9e", "titleColor": "#000000", "edgeLabelBackground": "#ffffff"}}}%%
flowchart TB
    subgraph T1["① Contents 30, 40, 50, 60 (array is [50, 60, 30, 40])"]
        direction TB
        a1["tree[1]<br><b>60</b><br>= Max"]
        a2["tree[2]<br>60"]
        a3["tree[3]<br>40"]
        a4["tree[4]<br>[0] = 50"]
        a5["tree[5]<br>[1] = 60"]
        a6["tree[6]<br>[2] = 30"]
        a7["tree[7]<br>[3] = 40"]
        a1 --- a2 & a3
        a2 --- a4 & a5
        a3 --- a6 & a7
    end
    subgraph T2["② PushBack 70 → [2] changes from 30 to 70. The update propagates from leaf to root"]
        direction TB
        b1["tree[1]<br><b>70</b><br>= Max"]
        b2["tree[2]<br>60"]
        b3["tree[3]<br>70"]
        b4["tree[4]<br>[0] = 50"]
        b5["tree[5]<br>[1] = 60"]
        b6["tree[6]<br>[2] = 70"]
        b7["tree[7]<br>[3] = 40"]
        b1 --- b2 & b3
        b2 --- b4 & b5
        b3 --- b6 & b7
    end
    subgraph T3["③ PushBack 35 → [3] changes from 40 to 35. tree[3] stays 70, so the update stops there"]
        direction TB
        c1["tree[1]<br><b>70</b><br>= Max"]
        c2["tree[2]<br>60"]
        c3["tree[3]<br>70<br>(unchanged)"]
        c4["tree[4]<br>[0] = 50"]
        c5["tree[5]<br>[1] = 60"]
        c6["tree[6]<br>[2] = 70"]
        c7["tree[7]<br>[3] = 35"]
        c1 --- c2 & c3
        c2 --- c4 & c5
        c3 --- c6 & c7
    end
    T1 ~~~ T2 ~~~ T3


    classDef default fill:#eceff1,stroke:#607d8b,color:#000
    style T1 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    style T2 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    style T3 fill:#f5f5f5,stroke:#9e9e9e,color:#000
    linkStyle default stroke:#455a64
    classDef root fill:#c8e6c9,stroke:#2e7d32,stroke-width:3px,color:#000
    classDef updated fill:#ffe0b2,stroke:#ef6c00,stroke-width:3px,color:#000
    classDef stopped fill:#eeeeee,stroke:#616161,stroke-width:3px,stroke-dasharray:5 5,color:#000
    class a1 root
    class b6,b3,b1 updated
    class c7 updated
    class c3 stopped
```

## Repository Layout
```
Assets/                      Sample scene, sample scripts, and prefabs
Packages/jp.xeon.x-graphs/   The package (Runtime / Editor)
ProjectSettings/             Unity project settings
```

## License
Please contact the repository maintainer regarding the license.

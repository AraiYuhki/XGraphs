# XGraphs

[English](README_EN.md)

![XGraphs のサンプルシーン](Documentation/images/XGraphsSampleDemo.gif)

Unity の uGUI 上で棒グラフ・折れ線グラフ・円グラフなどを描画するためのパッケージです。系列データのモデルとデータソース用アダプタ、インスペクタ拡張を備えており、ランタイムでもエディタでもグラフを素早く構築できます。

このリポジトリは Unity プロジェクトになっており、パッケージ本体は [`Packages/jp.xeon.x-graphs/`](Packages/jp.xeon.x-graphs/) にあります。サンプルシーンもあわせて確認できます。

## 主な特徴
- 棒グラフ、積み上げ棒グラフ、折れ線グラフ、円グラフ、リンググラフ、レーダーチャートを uGUI コンポーネントとして提供します。
- `Series` / `MultiValueSeries` などのデータモデルで、複数凡例・複数系列を扱えます。
- 配列、`List<T>`、`ObservableCollection<T>`、`CircularBuffer<T>` をアダプタ経由でそのままデータソースにできます。
- 棒グラフ・積み上げ棒グラフ・折れ線グラフには、任意の値にラベル付きの目盛り線（マーカー）を表示できます。
- **GameObject > UI > XGraphs** メニューから各グラフを Canvas 上に作成できます。

## 動作環境
- Unity 6000.0 以降
- uGUI（`com.unity.ugui`）
- TextMeshPro（凡例とマーカーのラベル表示に使用）

## インストール
Unity の **Window > Package Manager** で `+` メニューから **Install package from git URL...** を選び、次の URL を入力します。

```
https://github.com/AraiYuhki/XGraphs.git?path=/Packages/jp.xeon.x-graphs
```

`Packages/manifest.json` に直接追記する場合は次のとおりです。

```json
{
  "dependencies": {
    "jp.xeon.x-graphs": "https://github.com/AraiYuhki/XGraphs.git?path=/Packages/jp.xeon.x-graphs"
  }
}
```

## サンプル
1. リポジトリをクローンします。
   ```
   git clone https://github.com/AraiYuhki/XGraphs.git
   ```
2. Unity Hub の **Add > Add project from disk** でクローンしたフォルダを開きます。開発に使用している Unity のバージョンは `ProjectSettings/ProjectVersion.txt` で確認できます。
3. `Assets/Scenes/SampleScene.unity` を開いて Play します。

[`Assets/Scripts/GraphSample.cs`](Assets/Scripts/GraphSample.cs) がフレーム時間（Update / Render / その他）とログ件数をリアルタイムに集計し、折れ線グラフ・積み上げ棒グラフ・棒グラフ・円グラフ・リンググラフで表示します。折れ線グラフと積み上げ棒グラフには FPS の目安線（15 / 30 / 60 / 90 / 120 FPS）がマーカーとして表示されます。

## クイックスタート
1. **GameObject > UI > XGraphs > Bar Chart** で Canvas 上に棒グラフを作成します。
2. スクリプトから `MultiValueSeries` を組み立てて `Initialize` に渡します。

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
        var sales = new Series("売上", Color.cyan, new[] { 80f, 120f, 100f });
        var profit = new Series("利益", Color.green, new[] { 40f, 60f, 55f });
        var series = new MultiValueSeries(new List<Series> { sales, profit }, Color.white);

        var markers = new List<ScaleMarkerData> { new("目標", 100f) };
        chart.Initialize(series, markers);
        chart.SetMax(150f);
    }
}
```

値を追加するときは `Series.Add` または `MultiValueSeries.AddValue` を使うと、グラフが自動で再描画されます。リアルタイムに値を流す場合は、`CircularBuffer<float>` を渡して `Series` を作ると古い値から順に捨てられます（`GraphSample.cs` を参照）。

## 収録コンポーネント
| コンポーネント | データ | 概要 |
| --- | --- | --- |
| `BarChart` | `MultiValueSeries` | 複数凡例の棒グラフ。値域・棒の間隔・マーカーを設定できます。 |
| `StackedBarChart` | `MultiValueSeries` | 凡例ごとに値を積み上げる棒グラフ。 |
| `LineChart` | `MultiValueSeries` | 複数系列の折れ線グラフ。線幅とマイター結合の上限を設定できます。 |
| `PieChart` | `MultiLegendSeries` | 円グラフ。開始角度を設定できます。 |
| `RingChart` | `LayeredSeries` | `MultiLegendSeries` を層として重ねるリンググラフ。 |
| `RaderChart` | `MultiValueSeries` | 複数凡例のレーダーチャート。開始角度と最大値を設定できます。 |

## データモデル
| クラス | 用途 |
| --- | --- |
| `Series` | 1 凡例分の連続データ。配列・`List<T>`・`ObservableCollection<T>`・`CircularBuffer<T>` をラップします。 |
| `MultiValueSeries` | 複数の `Series` を束ねたデータ。棒・積み上げ棒・折れ線・レーダーで使います。 |
| `SingleValueSeries` | 名前・色・単一値を持つ凡例。 |
| `MultiLegendSeries` | 複数の `SingleValueSeries` を束ねたデータ。円グラフで使います。 |
| `LayeredSeries` | 複数の `MultiLegendSeries` を層として束ねたデータ。リンググラフで使います。 |

詳しい使い方はパッケージの [README](Packages/jp.xeon.x-graphs/README.md) を参照してください。

## CircularBuffer の仕組み
リアルタイムのグラフでは「直近 N 件だけを表示し、古い値から捨てていく」処理を毎フレーム行います。`CircularBuffer<T>` はそのためのリングバッファで、固定長の配列と、先頭位置 `start`・次の書き込み位置 `end` の 2 つのインデックスだけで動きます。

- 末尾への追加、先頭の破棄、i 番目の読み取りはすべて O(1) です。要素をずらす処理はありません。
- 配列は最初に一度確保したきりなので、毎フレーム値を追加しても GC アロケーションが発生しません。
- 満杯になってからは `start` と `end` が重なったまま配列の上を進み、末尾まで来ると先頭に戻ります。値は一度書いた位置から動きません。

容量 4 のバッファに値を追加していくと、次のように動きます。

```mermaid
flowchart TB
    subgraph S1["① 空の状態　Count = 0"]
        direction LR
        a0["[0]<br>　<br>start / end"] --- a1["[1]<br>　"] --- a2["[2]<br>　"] --- a3["[3]<br>　"]
        a3 -.->|折り返し| a0
    end
    subgraph S2["② 10, 20, 30 を PushBack　Count = 3"]
        direction LR
        b0["[0]<br>10<br>start"] --- b1["[1]<br>20"] --- b2["[2]<br>30"] --- b3["[3]<br>　<br>end"]
        b3 -.->|折り返し| b0
    end
    subgraph S3["③ 40 を PushBack → 満杯。end が 0 に戻り start と重なる　Count = 4"]
        direction LR
        c0["[0]<br>10<br>start / end"] --- c1["[1]<br>20"] --- c2["[2]<br>30"] --- c3["[3]<br>40"]
        c3 -.->|折り返し| c0
    end
    subgraph S4["④ 50 を PushBack → 一番古い 10 を捨てて [0] に書く。中身は 20, 30, 40, 50"]
        direction LR
        d0["[0]<br>50"] --- d1["[1]<br>20<br>start / end"] --- d2["[2]<br>30"] --- d3["[3]<br>40"]
        d3 -.->|折り返し| d0
    end
    subgraph S5["⑤ 60 を PushBack → 20 を捨てて [1] に書く。中身は 30, 40, 50, 60"]
        direction LR
        e0["[0]<br>50"] --- e1["[1]<br>60"] --- e2["[2]<br>30<br>start / end"] --- e3["[3]<br>40"]
        e3 -.->|折り返し| e0
    end
    S1 ~~~ S2 ~~~ S3 ~~~ S4 ~~~ S5

    classDef startEnd fill:#c8e6c9,stroke:#2e7d32,stroke-width:3px,color:#000
    classDef start fill:#bbdefb,stroke:#1565c0,stroke-width:3px,color:#000
    classDef endSlot fill:#ffe0b2,stroke:#ef6c00,stroke-width:3px,color:#000
    class a0,c0,d1,e2 startEnd
    class b0 start
    class b3 endSlot
```

### DoubleCircularBuffer の最大値管理
`DoubleCircularBuffer` は、バッファ内の最大値を固定長のセグメント木で管理します。縦軸を最大値に合わせて自動調整するときに使います。

- 葉が配列の各位置、根 `tree[1]` が全体の最大値です。`Max` は根を読むだけなので O(1) です。
- 値を書き換えると、葉から根に向かって O(log n) で更新します。親の値が変わらなければ、そこで打ち切ります。
- 木は長さ「容量 × 2」の `double[]` 1 本だけなので、値を出し入れしてもアロケーションが発生しません。

オレンジは書き換わった箇所、灰色の点線は更新を打ち切った箇所です。

```mermaid
flowchart TB
    subgraph T1["① 中身が 30, 40, 50, 60 の状態（配列は [50, 60, 30, 40]）"]
        direction TB
        a1["tree[1]<br><b>60</b><br>＝ Max"]
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
    subgraph T2["② 70 を PushBack → [2] の 30 が 70 に。葉から根まで更新が伝わる"]
        direction TB
        b1["tree[1]<br><b>70</b><br>＝ Max"]
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
    subgraph T3["③ 35 を PushBack → [3] の 40 が 35 に。tree[3] は 70 のまま変わらないので、そこで打ち切る"]
        direction TB
        c1["tree[1]<br><b>70</b><br>＝ Max"]
        c2["tree[2]<br>60"]
        c3["tree[3]<br>70<br>（変化なし）"]
        c4["tree[4]<br>[0] = 50"]
        c5["tree[5]<br>[1] = 60"]
        c6["tree[6]<br>[2] = 70"]
        c7["tree[7]<br>[3] = 35"]
        c1 --- c2 & c3
        c2 --- c4 & c5
        c3 --- c6 & c7
    end
    T1 ~~~ T2 ~~~ T3

    classDef root fill:#c8e6c9,stroke:#2e7d32,stroke-width:3px,color:#000
    classDef updated fill:#ffe0b2,stroke:#ef6c00,stroke-width:3px,color:#000
    classDef stopped fill:#eeeeee,stroke:#616161,stroke-width:3px,stroke-dasharray:5 5,color:#000
    class a1 root
    class b6,b3,b1 updated
    class c7 updated
    class c3 stopped
```

## リポジトリ構成
```
Assets/                      サンプルシーン・サンプルスクリプト・プレハブ
Packages/jp.xeon.x-graphs/   パッケージ本体（Runtime / Editor）
ProjectSettings/             Unity プロジェクト設定
```

## ライセンス
ライセンスについてはリポジトリの管理者にお問い合わせください。

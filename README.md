# XGraphs

Unity の uGUI 上で複数種類のグラフを描画するためのコンポーネント群です。系列データの管理やカスタムエディタによる編集支援を備え、ランタイムでもエディタでも素早くグラフを構築できます。

## 主な特徴
- 棒グラフ、積み上げ棒グラフ、折れ線グラフ、円グラフ、ドーナツグラフ、レーダーチャートなどの uGUI コンポーネントを同梱しています。【F:Runtime/Scripts/Views/BarChart.cs†L8-L108】【F:Runtime/Scripts/Views/StackedBarChart.cs†L8-L118】【F:Runtime/Scripts/Views/LineChart.cs†L8-L86】【F:Runtime/Scripts/Views/PieChart.cs†L8-L71】【F:Runtime/Scripts/Views/RingChart.cs†L8-L63】【F:Runtime/Scripts/Views/RaderChart.cs†L8-L51】
- `Series` や `MultiValueSeries` などのデータモデルを通じて複数系列・複数凡例を柔軟に表現できます。【F:Runtime/Scripts/Model/Series.cs†L14-L122】【F:Runtime/Scripts/Model/MultiValueSeries.cs†L8-L70】
- 配列、`List<T>`、`ObservableCollection<T>`、`CircularBuffer<T>` など多様なデータソースをアダプタ経由で取り込めます。【F:Runtime/Scripts/Model/Series.cs†L63-L108】【F:Runtime/Scripts/Model/DataAdapter/ArraySeriesDataAdapter.cs†L1-L67】【F:Runtime/Scripts/Model/DataAdapter/ListSeriesDataAdapter.cs†L1-L67】【F:Runtime/Scripts/Model/DataAdapter/ObservableCollectionDataAdapter.cs†L1-L83】【F:Runtime/Scripts/Model/DataAdapter/CircularBufferSeriesDataAdapter.cs†L1-L60】
- 棒グラフ向けのマーカー管理機能を備え、任意の値にラベル付きラインを配置できます。【F:Runtime/Scripts/Manager/MarkerManager.cs†L12-L122】【F:Runtime/Scripts/Views/BarGraphMarker.cs†L8-L67】
- マルチバリュー系列用のプロパティドロワーにより、インスペクタから凡例名・色・テストデータをまとめて編集できます。【F:Editor/MultiValueSeriesEditor.cs†L7-L169】

## 動作環境
- Unity 2022.3.6 以降 (package.json の `unity` フィールドを参照)。【F:package.json†L1-L13】
- TextMeshPro を使用するマーカー UI を含むため、プロジェクトに TextMeshPro を導入しておくことを推奨します。【F:Runtime/Scripts/Views/BarGraphMarker.cs†L1-L67】

## インストール
1. このリポジトリを任意の場所にクローン（または zip ダウンロード）します。
2. Unity エディタで **Window > Package Manager** を開き、左上の `+` メニューから **Add package from disk...** を選択します。
3. 本パッケージの `package.json` を指定すると、`jp.xeon.xgraphs` パッケージがプロジェクトに追加されます。【F:package.json†L1-L13】

## 収録コンポーネント
| コンポーネント | 概要 |
| --- | --- |
| `BarChart` | 複数凡例の棒グラフ。値域 (`RangeValue`) や棒間隔、マーカーを制御できます。【F:Runtime/Scripts/Views/BarChart.cs†L12-L118】【F:Runtime/Scripts/Model/RangeValue.cs†L5-L45】 |
| `StackedBarChart` | 凡例単位で積み上げ表示する棒グラフ。値域やマーカー設定を共有できます。【F:Runtime/Scripts/Views/StackedBarChart.cs†L12-L125】 |
| `LineChart` | マイター／ベベル結合に対応した折れ線グラフ。線幅やレンジ、複数系列をサポートします。【F:Runtime/Scripts/Views/LineChart.cs†L8-L210】 |
| `PieChart` | 単一レイヤーの円グラフ。開始角度やセグメント分割を制御しつつ、`SingleValueSeries` を描画します。【F:Runtime/Scripts/Views/PieChart.cs†L12-L106】【F:Runtime/Scripts/Model/SingleValueSeries.cs†L8-L63】 |
| `RingChart` | 多層リング状グラフ。`LayeredSeries` を用いて各レイヤー／凡例を構成します。【F:Runtime/Scripts/Views/RingChart.cs†L12-L83】【F:Runtime/Scripts/Model/LayeredSeries.cs†L8-L65】 |
| `RaderChart` | 複数凡例に対応したレーダーチャート。開始角度と最大値を基準に正規化します。【F:Runtime/Scripts/Views/RaderChart.cs†L10-L54】 |

## データモデルとアダプタ
- `Series` は 1 凡例分の連続データを表し、`ArraySeriesDataAdapter` や `ListSeriesDataAdapter` を通じてさまざまなコンテナをラップできます。【F:Runtime/Scripts/Model/Series.cs†L24-L108】【F:Runtime/Scripts/Model/DataAdapter/ArraySeriesDataAdapter.cs†L1-L67】【F:Runtime/Scripts/Model/DataAdapter/ListSeriesDataAdapter.cs†L1-L67】
- `MultiValueSeries` は複数凡例を束ねた 2 次元データを持ち、色をまとめて乗算することでテーマカラーを適用します。【F:Runtime/Scripts/Model/MultiValueSeries.cs†L8-L66】
- `SingleValueSeries` は単一値を持つ凡例で、円グラフやリンググラフで利用します。【F:Runtime/Scripts/Model/SingleValueSeries.cs†L8-L63】
- `MultiLegendSeries` と `LayeredSeries` を組み合わせることで、リンググラフのようにレイヤー単位で凡例を構成できます。【F:Runtime/Scripts/Model/MultiLegendSeries.cs†L8-L83】【F:Runtime/Scripts/Model/LayeredSeries.cs†L8-L65】
- バッファリングが必要な場合は、`CircularBuffer<T>` や `DoubleCircularBuffer` を用いて新しい値をリングバッファに蓄積できます。【F:Runtime/Scripts/CircularBuffer.cs†L9-L156】【F:Runtime/Scripts/DoubleCircularBuffer.cs†L6-L53】

## クイックスタート
1. シーン内の Canvas 配下に空の GameObject を作成し、`BarChart` コンポーネントを追加します。【F:Runtime/Scripts/Views/BarChart.cs†L12-L118】
2. スクリプトから `MultiValueSeries` を構築し、`BarChart.Initialize` または `SetSeries` に渡します。

```csharp
using UnityEngine;
using Xeon.XGraph.Model;
using Xeon.XGraph.View;

public class BarChartSample : MonoBehaviour
{
    [SerializeField] private BarChart chart;

    private void Start()
    {
        var sales = new Series("売上", Color.cyan, new[] { 80f, 120f, 100f });
        var profit = new Series("利益", Color.green, new[] { 40f, 60f, 55f });
        var multi = new MultiValueSeries(new System.Collections.Generic.List<Series> { sales, profit }, Color.white);
        chart.Initialize(multi);
    }
}
```

3. 値域を調整したい場合は `chart.SetMin` / `SetMax`、マーカーを置きたい場合は `chart.MarkerManager.AddMarker` を呼び出します。【F:Runtime/Scripts/Views/BarChart.cs†L34-L108】【F:Runtime/Scripts/Manager/MarkerManager.cs†L24-L77】

## エディタサポート
- `MultiValueSeries` プロパティは専用のプロパティドロワーによって折りたたみ表示、凡例数の増減、テストデータ編集などが可能です。【F:Editor/MultiValueSeriesEditor.cs†L7-L169】
- `MarkerManager` にはカスタムプロパティドロワーが同梱されており、インスペクタ上からマーカーリストを編集できます。【F:Runtime/Scripts/Manager/MarkerManager.cs†L92-L197】

## ライセンス
本パッケージのライセンス形態はリポジトリの管理者にお問い合わせください。

# XGraphs

Unity の uGUI 上でさまざまな種類のグラフを描画できるコンポーネント群です。系列データを扱うためのデータモデルとエディタ拡張を備え、ランタイムでもエディタでも素早くグラフを構築できます。

## 主な特徴
- 棒グラフ、積み上げ棒グラフ、折れ線グラフ、円グラフ、ドーナツグラフ、レーダーチャートなどの uGUI コンポーネントを同梱しています。
- `Series` や `MultiValueSeries` といったデータモデルにより、複数凡例・複数系列を柔軟に表現できます。
- 配列、`List<T>`、`ObservableCollection<T>`、`CircularBuffer<T>` など多様なデータソースをアダプタ経由で取り込めます。
- 棒グラフ・積み上げ棒グラフ・折れ線グラフ向けのマーカー管理機能を備え、任意の値にラベル付きラインを配置できます。
- マルチバリュー系列用のプロパティドロワーにより、インスペクタから凡例名・色・テストデータをまとめて編集できます。

## 動作環境
- Unity 6000.0 以降（`package.json` の `unity` フィールドを参照）。
- TextMeshPro を使用するマーカー UI が含まれるため、プロジェクトに TextMeshPro を導入しておくことを推奨します。

## インストール
1. このリポジトリを任意の場所にクローン（または zip ダウンロード）します。
2. Unity エディタで **Window > Package Manager** を開き、左上の `+` メニューから **Add package from disk...** を選択します。
3. 本パッケージの `package.json` を指定すると、`jp.xeon.x-graphs` パッケージがプロジェクトに追加されます。

## サンプルの見方
このリポジトリ自体が Unity プロジェクトになっており、クローンするだけでサンプルシーンを確認できます。

1. リポジトリをクローンします。
   ```
   git clone https://github.com/AraiYuhki/XGraphs.git
   ```
2. Unity Hub の **Add > Add project from disk** でクローンしたフォルダを選択して開きます（開発に使用している Unity バージョンは `ProjectSettings/ProjectVersion.txt` を参照。初回は Library の生成と TextMeshPro のインポートに時間がかかります）。
3. `Assets/Scenes/SampleScene.unity` を開きます。
4. Play ボタンを押すと、`Assets/Scripts/GraphSample.cs` が現在のフレーム時間（Update / Render / その他）やログ件数をリアルタイムに集計し、折れ線グラフ・積み上げ棒グラフ・棒グラフ・円グラフ・リンググラフとして表示します。折れ線グラフには FPS の目安線（15 / 30 / 60 / 90 / 120FPS）がマーカーとして表示されます。

サンプルの実装は、各グラフの初期化・データ更新・マーカー設定の参考になります。パッケージ本体は `Packages/jp.xeon.x-graphs/` にあります。

> Git URL でパッケージとして導入する場合は、`https://github.com/AraiYuhki/XGraphs.git?path=/Packages/jp.xeon.x-graphs` を指定してください。

## 収録コンポーネント
| コンポーネント | 概要 |
| --- | --- |
| `BarChart` | 複数凡例の棒グラフ。値域（`RangeValue`）や棒間隔、マーカーを制御できます。 |
| `StackedBarChart` | 凡例単位で積み上げ表示する棒グラフ。値域やマーカー設定を共有できます。 |
| `LineChart` | マイター／ベベル結合に対応した折れ線グラフ。線幅やレンジ、複数系列をサポートします。 |
| `PieChart` | 単一レイヤーの円グラフ。開始角度を設定でき、`SingleValueSeries` を束ねた `MultiLegendSeries` を描画します。 |
| `RingChart` | 多層リング状グラフ。`LayeredSeries` を用いて各レイヤー／凡例を構成します。 |
| `RaderChart` | 複数凡例に対応したレーダーチャート。開始角度と最大値を基準に正規化します。 |

## データモデルとアダプタ
- `Series` は 1 凡例分の連続データを表し、`ArraySeriesDataAdapter` や `ListSeriesDataAdapter` を通じてさまざまなコンテナをラップできます。
- `MultiValueSeries` は複数凡例を束ねた 2 次元データを持ち、色をまとめて乗算することでテーマカラーを適用します。
- `SingleValueSeries` は単一値を持つ凡例で、円グラフやリンググラフで利用します。
- `MultiLegendSeries` と `LayeredSeries` を組み合わせることで、リンググラフのようにレイヤー単位で凡例を構成できます。
- バッファリングが必要な場合は、`CircularBuffer<T>` や `DoubleCircularBuffer` を用いて新しい値をリングバッファに蓄積できます。

## クイックスタート
1. シーン内の Canvas 配下に空の GameObject を作成し、`BarChart` コンポーネントを追加します。
2. スクリプトから `MultiValueSeries` を構築し、`BarChart.Initialize` または `SetSeries` に渡します。

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
        var multi = new MultiValueSeries(new List<Series> { sales, profit }, Color.white);
        chart.Initialize(multi, new List<ScaleMarkerData> { new("目標", 100f) });
    }
}
```

3. 値域を調整したい場合は `chart.SetMin` / `SetMax` を呼び出します。マーカーは `Initialize` の第 2 引数で渡すほか、`LineChart` と `StackedBarChart` では `chart.MarkerManager.AddMarker` で後から追加できます。

## エディタサポート
- `MultiValueSeries` プロパティは専用のプロパティドロワーによって折りたたみ表示、凡例数の増減、テストデータ編集などが可能です。
- `ScaleMarkerManager` にはカスタムプロパティドロワーが同梱されており、インスペクタ上からマーカーリストを編集できます。

## ライセンス
本パッケージのライセンス形態はリポジトリの管理者にお問い合わせください。

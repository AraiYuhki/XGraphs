using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.XGraph.Rendering
{
    /// <summary>
    /// uGUI(CanvasRenderer)上でマイター結合(フォールバックはベベル結合)に対応した、太さのある折れ線のメッシュを生成するクラス
    /// </summary>
    public class UILineRenderer
    {
        private float thickness = 1f;
        private float miterLimit = 4f;

        private float HalfWidth => thickness * 0.5f;

        /// <summary>
        /// 線の太さ
        /// </summary>
        public float Thickness
        {
            get => thickness;
            set => thickness = value;
        }

        /// <summary>
        /// thicknessの何倍までマイターを許容するか(超える場合はベベル結合にフォールバックする)
        /// </summary>
        public float MiterLimit
        {
            get => miterLimit;
            set => miterLimit = value;
        }

        public UILineRenderer() { }

        public UILineRenderer(float thickness, float miterLimit)
        {
            this.thickness = thickness;
            this.miterLimit = miterLimit;
        }

        /// <summary>
        /// 折れ線を構成する頂点・インデックスを追加する
        /// </summary>
        /// <param name="points">線を構成する点列(先頭からcount個を使用する)</param>
        /// <param name="count">points内で使用する点の数(2未満の場合は何もしない)</param>
        /// <param name="color">線の色</param>
        /// <param name="vertices">頂点の追加先</param>
        /// <param name="indices">インデックスの追加先</param>
        public void AddLine(IReadOnlyList<Vector2> points, int count, Color32 color, List<UIVertex> vertices, List<int> indices)
        {
            if (points == null || count <= 1 || vertices == null || indices == null)
                return;

            var vert = UIVertex.simpleVert;
            vert.color = color;

            var vertexIndex = vertices.Count;
            var prev = Vector2.zero;
            var prevBebel = false;

            for (var i = 0; i < count; i++)
            {
                var current = points[i];

                if (i == 0)
                {
                    ProcessStart(current, points[i + 1], ref vert, vertices);
                    prev = current;
                    continue;
                }

                if (i == count - 1)
                {
                    ProcessEnd(current, prev, ref vert, vertices);
                    AddTriangle(indices, vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    AddTriangle(indices, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3);
                    continue;
                }

                var next = points[i + 1];
                var (top, bottom, tmp) = GetJoinVertices(prev, current, next);
                AddVertices(ref vert, top, bottom, vertices);

                var isBebel = false;
                if (tmp.HasValue)
                {
                    AddVertex(tmp.Value, ref vert, vertices);
                    isBebel = true;
                }

                var diffY = current.y - prev.y;

                AddTriangles(indices, vertexIndex, diffY, isBebel, prevBebel);
                vertexIndex += isBebel ? 3 : 2;

                prev = current;
                prevBebel = isBebel;
            }
        }

        /// <summary>
        /// 始点の処理
        /// </summary>
        private void ProcessStart(Vector2 current, Vector2 next, ref UIVertex vert, List<UIVertex> vertices)
        {
            var (top, bottom) = GetEndCap(current, next - current);
            AddVertices(ref vert, top, bottom, vertices);
        }

        /// <summary>
        /// 終点の処理
        /// </summary>
        private void ProcessEnd(Vector2 current, Vector2 prev, ref UIVertex vert, List<UIVertex> vertices)
        {
            var (top, bottom) = GetEndCap(current, current - prev);
            AddVertices(ref vert, top, bottom, vertices);
        }

        /// <summary>
        /// マイター結合の計算をする(フォールバックはベベル結合)
        /// </summary>
        private (Vector2 top, Vector2 bottom, Vector2? nextTop) GetJoinVertices(Vector2 prev, Vector2 current, Vector2 next)
        {
            var prevDirection = (current - prev).normalized;
            var nextDirection = (next - current).normalized;
            var prevNormal = GetNormal(prevDirection);
            var nextNormal = GetNormal(nextDirection);

            var miterDirection = (prevNormal + nextNormal).normalized;
            var dotToNextNormal = Vector2.Dot(miterDirection, nextNormal);

            if (Mathf.Abs(dotToNextNormal) < float.Epsilon)
            {
                return (current + nextNormal * HalfWidth, current - nextNormal * HalfWidth, null);
            }

            var miterLength = HalfWidth / dotToNextNormal;

            if (Mathf.Abs(miterLength) > (HalfWidth * Mathf.Max(1f, miterLimit)))
            {
                var currentNormal = prevDirection.y > 0 ? prevNormal : -prevNormal;
                var top = current + currentNormal * HalfWidth;
                var bottom = current - currentNormal * HalfWidth;
                var diff = top - bottom;
                diff.x *= -1;
                var tmp = bottom + diff;
                return (top, bottom, tmp);
            }

            return (current + miterDirection * miterLength, current - miterDirection * miterLength, null);
        }

        /// <summary>
        /// 始点と終点の頂点位置の計算を行う
        /// </summary>
        private (Vector2 top, Vector2 bottom) GetEndCap(Vector2 point, Vector2 direction)
        {
            var normal = GetNormal(direction.normalized);
            return (point + normal * HalfWidth, point - normal * HalfWidth);
        }

        /// <summary>
        /// 指定したベクトルの法線を取得する(左に90度回転させる)
        /// </summary>
        private Vector2 GetNormal(Vector2 vector) => new(-vector.y, vector.x);

        /// <summary>
        /// 頂点を一括で追加する
        /// </summary>
        /// <remarks>params配列だと毎フレーム大量のGCアロケーションが発生するため固定引数にしている</remarks>
        private void AddVertices(ref UIVertex vert, Vector2 top, Vector2 bottom, List<UIVertex> vertices)
        {
            AddVertex(top, ref vert, vertices);
            AddVertex(bottom, ref vert, vertices);
        }

        /// <summary>
        /// 頂点を追加する
        /// </summary>
        private void AddVertex(Vector2 position, ref UIVertex vert, List<UIVertex> vertices)
        {
            vert.position = new Vector3(position.x, position.y);
            vertices.Add(vert);
        }

        /// <summary>
        /// 面を追加する
        /// </summary>
        private void AddTriangles(List<int> indices, int index, float diffY, bool isBebel, bool prevIsBebel)
        {
            if (isBebel)
            {
                AddTriangle(indices, index, index + 1, index + 2);
                AddTriangle(indices, index + 2, index + 1, index + 3);
                AddTriangle(indices, index + 2, index + 4, index + 3);
                return;
            }

            if (diffY >= 0 || !prevIsBebel)
            {
                AddTriangle(indices, index, index + 1, index + 2);
                AddTriangle(indices, index + 2, index + 1, index + 3);
                return;
            }

            AddTriangle(indices, index, index + 1, index + 2);
            AddTriangle(indices, index + 3, index, index + 2);
        }

        /// <summary>
        /// 三角ポリゴンを追加する
        /// </summary>
        private void AddTriangle(List<int> indices, int a, int b, int c)
        {
            indices.Add(a);
            indices.Add(b);
            indices.Add(c);
        }
    }
}

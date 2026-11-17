using System;

namespace Xeon.XGraph.Model
{
    public class DoubleCircularBuffer : CircularBuffer<double>
    {
        // 配列上の位置ごとの値を葉に持つ最大値のセグメント木（tree[1] が全体の最大値）
        // 葉は tree[Capacity + slot]。固定長なので値を出し入れしてもアロケーションが発生しない
        // 空き位置は NaN で表す。Max2 は NaN を最小として扱うので、NaN 以外の値があればそちらが選ばれる
        private readonly double[] tree;

        public DoubleCircularBuffer(int capacity) : base(capacity)
        {
            tree = CreateTree(capacity);
        }

        public DoubleCircularBuffer(int capacity, double[] items) : base(capacity, items)
        {
            tree = CreateTree(capacity);
            for (var index = 0; index < Count; index++)
            {
                var slot = start + index;
                if (slot >= capacity)
                    slot -= capacity;
                UpdateLeaf(slot, buffer[slot]);
            }
        }

        public double Max => IsEmpty ? 0.0 : tree[1];

        public override void Clear()
        {
            Array.Fill(tree, double.NaN);
            base.Clear();
        }

        protected override void OnSlotWritten(int slot, double value) => UpdateLeaf(slot, value);

        protected override void OnSlotRemoved(int slot) => UpdateLeaf(slot, double.NaN);

        private static double[] CreateTree(int capacity)
        {
            var tree = new double[capacity * 2];
            Array.Fill(tree, double.NaN);
            return tree;
        }

        private void UpdateLeaf(int slot, double value)
        {
            var index = Capacity + slot;
            tree[index] = value;
            for (index >>= 1; index >= 1; index >>= 1)
            {
                var max = Max2(tree[index * 2], tree[index * 2 + 1]);
                // 親の値が変わらなければ、それより上も変わらない
                if (max.Equals(tree[index]))
                    break;
                tree[index] = max;
            }
        }

        private static double Max2(double a, double b) => a >= b || double.IsNaN(b) ? a : b;
    }
}

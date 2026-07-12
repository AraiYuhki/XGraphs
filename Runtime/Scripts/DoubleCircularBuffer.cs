using System;
using System.Collections.Generic;

namespace Xeon.XGraph.Model
{
    public class DoubleCircularBuffer : CircularBuffer<double>
    {
        // SortedSetは重複値を保持できないため、値ごとの個数をDictionaryで管理し、
        // 個数が0になったときだけSortedSetから取り除くマルチセットとして扱う
        private readonly Dictionary<double, int> counts = new();
        private readonly SortedSet<double> sortedValues = new();

        public DoubleCircularBuffer(int capacity) : base(capacity)
        {
        }

        public DoubleCircularBuffer(int capacity, double[] items) : base(capacity, items)
        {
            for (var index = 0; index < Count; index++)
                AddValue(base[index]);
        }

        public double Max => sortedValues.Count == 0 ? 0.0 : sortedValues.Max;

        public override double this[int index]
        {
            get => base[index];
            set
            {
                if (IsEmpty)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty.");
                if (index >= Count)
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {Count}.");

                RemoveValue(base[index]);
                AddValue(value);
                base[index] = value;
            }
        }

        public override void Clear()
        {
            counts.Clear();
            sortedValues.Clear();
            base.Clear();
        }

        public override void PushBack(double item)
        {
            if (IsFull)
                RemoveValue(Front());
            AddValue(item);
            base.PushBack(item);
        }

        public override void PushFront(double item)
        {
            if (IsFull)
                RemoveValue(Back());
            AddValue(item);
            base.PushFront(item);
        }

        public override void PopBack()
        {
            if (!IsEmpty)
                RemoveValue(Back());
            base.PopBack();
        }

        public override void PopFront()
        {
            if (!IsEmpty)
                RemoveValue(Front());
            base.PopFront();
        }

        private void AddValue(double value)
        {
            counts.TryGetValue(value, out var count);
            counts[value] = count + 1;
            if (count == 0)
                sortedValues.Add(value);
        }

        private void RemoveValue(double value)
        {
            if (!counts.TryGetValue(value, out var count))
                return;
            if (count <= 1)
            {
                counts.Remove(value);
                sortedValues.Remove(value);
            }
            else
            {
                counts[value] = count - 1;
            }
        }
    }
}

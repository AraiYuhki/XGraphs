using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xeon.XGraph.Model
{
    public class CircularBuffer<T> : IObservableCollection<T>, IReadOnlyList<T>
    {
        protected T[] buffer;  // バッファ本体

        protected int start;   // 先頭インデックス
        protected int end;     // 次に追加される位置

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Capacity => buffer.Length;
        public bool IsFull => Count == Capacity;
        public bool IsEmpty => Count == 0;
        public int Count { get; protected set; }

        public bool IsReadOnly => false;

        public virtual T this[int index]
        {
            get
            {
                // uint にキャストして負のインデックスも 1 回の比較で弾く
                if ((uint)index >= (uint)Count)
                    ThrowIndexOutOfRange(index);

                return buffer[ToActualIndex(index)];
            }

            set
            {
                if ((uint)index >= (uint)Count)
                    ThrowIndexOutOfRange(index);

                var actualIndex = ToActualIndex(index);
                var oldItem = buffer[actualIndex];
                WriteSlot(actualIndex, value);

                // 要素の置換を通知
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        /// <summary>
        /// items で初期化する。items が容量を超える場合は、末尾側（新しい側）の要素を残す
        /// </summary>
        public CircularBuffer(int capacity, T[] items)
        {
            if (capacity < 1)
                throw new ArgumentException("容量は1以上でなければなりません。", nameof(capacity));

            buffer = new T[capacity];
            if (items == null)
                return;

            var copyCount = Math.Min(items.Length, capacity);
            Array.Copy(items, items.Length - copyCount, buffer, 0, copyCount);

            Count = copyCount;
            start = 0;
            end = copyCount % Capacity;
        }

        public CircularBuffer(int capacity, bool fill = false)
        {
            if (capacity < 1)
                throw new ArgumentException("容量は1以上でなければなりません。", nameof(capacity));

            // 配列は既定値で初期化されるので、fill のときは要素数を満杯にするだけでよい
            buffer = new T[capacity];
            Count = fill ? capacity : 0;
            start = 0;
            end = 0;
        }

        public virtual void Clear()
        {
            // バッファ全体をクリア
            Array.Clear(buffer, 0, buffer.Length);

            Count = start = end = 0;

            // コレクション全体がリセットされたことを通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public T Front()
        {
            ThrowIfEmpty();
            return buffer[start];
        }

        public T Back()
        {
            ThrowIfEmpty();
            var lastIndex = (end == 0) ? Capacity - 1 : end - 1;
            return buffer[lastIndex];
        }

        public virtual void Add(T item)
        {
            PushBack(item);
        }

        /// <summary>
        /// 末尾に要素を追加（満杯の場合は先頭を捨ててから追加）
        /// </summary>
        public virtual void PushBack(T item)
        {
            // 満杯のときは先頭の削除と末尾への追加を順に通知し、各通知の時点で状態と内容が一致するようにする
            if (IsFull)
                RemoveFront();

            WriteSlot(end, item);
            var logicalIndex = Count;
            Increment(ref end);
            Count++;

            // 要素が追加されたことを通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, logicalIndex));
        }

        /// <summary>
        /// 先頭に要素を追加（満杯の場合は末尾を捨ててから追加）
        /// </summary>
        public virtual void PushFront(T item)
        {
            if (IsFull)
                RemoveBack();

            Decrement(ref start);
            WriteSlot(start, item);
            Count++;

            // 要素が先頭に追加されたことを通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, 0));
        }

        /// <summary>
        /// 末尾の要素を削除
        /// </summary>
        public virtual void PopBack()
        {
            ThrowIfEmpty("バッファが空のため、要素を削除できません。");
            RemoveBack();
        }

        /// <summary>
        /// 先頭の要素を削除
        /// </summary>
        public virtual void PopFront()
        {
            ThrowIfEmpty("バッファが空のため、要素を削除できません。");
            RemoveFront();
        }

        /// <summary>
        /// 配列上の slot に値を書き込んだ直後に呼ばれる。派生クラスで集計値の更新に使う
        /// </summary>
        protected virtual void OnSlotWritten(int slot, T value) { }

        /// <summary>
        /// 配列上の slot の要素が取り除かれた直後に呼ばれる。派生クラスで集計値の更新に使う
        /// </summary>
        protected virtual void OnSlotRemoved(int slot) { }

        private void WriteSlot(int slot, T value)
        {
            buffer[slot] = value;
            OnSlotWritten(slot, value);
        }

        private void RemoveBack()
        {
            Decrement(ref end);
            var removedItem = buffer[end];
            buffer[end] = default;
            Count--;
            OnSlotRemoved(end);

            // 要素削除を通知（論理インデックス Count は削除前の最後の要素位置）
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, removedItem, Count));
        }

        private void RemoveFront()
        {
            var removedSlot = start;
            var removedItem = buffer[removedSlot];
            buffer[removedSlot] = default;
            Increment(ref start);
            Count--;
            OnSlotRemoved(removedSlot);

            // 先頭要素削除を通知
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, removedItem, 0));
        }

        /// <summary>
        /// バッファが空の場合に例外を送出
        /// </summary>
        private void ThrowIfEmpty(string message = "バッファが空のためアクセスできません。")
        {
            if (!IsEmpty) return;
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// インデックス範囲外の例外を送出（インデクサをインライン化しやすくするため分離）
        /// </summary>
        private void ThrowIndexOutOfRange(int index)
        {
            if (IsEmpty)
                throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファが空です。");
            throw new IndexOutOfRangeException($"インデックス {index} にアクセスできません。バッファの要素数は {Count} です。");
        }

        /// <summary>
        /// 論理インデックスを配列上の位置に変換
        /// start &lt; Capacity かつ index &lt; Capacity なので、剰余ではなく1回の減算で折り返せる
        /// </summary>
        private int ToActualIndex(int index)
        {
            var actualIndex = start + index;
            return actualIndex >= buffer.Length ? actualIndex - buffer.Length : actualIndex;
        }

        /// <summary>
        /// インデックスを1進める（末尾に達したら0に戻す）
        /// </summary>
        private void Increment(ref int index)
        {
            index++;
            if (index == Capacity)
                index = 0;
        }

        /// <summary>
        /// インデックスを1戻す（0なら末尾に戻す）
        /// </summary>
        private void Decrement(ref int index)
        {
            if (index == 0)
                index = Capacity;
            index--;
        }

        /// <summary>
        /// foreach で直接回したときにアロケーションが起きないよう struct の列挙子を返す
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly CircularBuffer<T> owner;
            private int index;
            private T current;

            internal Enumerator(CircularBuffer<T> owner)
            {
                this.owner = owner;
                index = 0;
                current = default;
            }

            public T Current => current;

            object IEnumerator.Current => current;

            public bool MoveNext()
            {
                if (index >= owner.Count)
                    return false;

                current = owner.buffer[owner.ToActualIndex(index)];
                index++;
                return true;
            }

            public void Reset()
            {
                index = 0;
                current = default;
            }

            public void Dispose() { }
        }
    }
}

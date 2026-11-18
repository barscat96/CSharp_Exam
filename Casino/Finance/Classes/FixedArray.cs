using System.Collections;

namespace CSharp_Exam.Casino.Finance.Classes;

public class FixedArray<T>(uint size) : IEnumerable<T>
    where T : notnull
{
    private readonly T[] array = new T[size];
    private uint index = 0;

    public uint Capacity => size;
    public uint Count => index;
    public bool IsFull => index == array.Length;

    public T this[uint i] => i < index ? array[i] : throw new IndexOutOfRangeException();

    public void Add(T item)
    {
        if (index >= array.Length) throw new IndexOutOfRangeException();
        array[index++] = item;
    }

    public void Remove(T item)
    {
        int i = IndexOf(item);
        if (i == -1) return;

        Array.Copy(array, i + 1, array, i, index - i - 1);
        array[--index] = default!;   // clear the freed slot
    }

    public int IndexOf(T item) => Array.IndexOf(array, item, 0, (int)index);

    public bool Contains(T item) => IndexOf(item) != -1;

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < index; i++)
            yield return array[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
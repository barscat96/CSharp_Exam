using System.Collections;

namespace CSharp_Exam.Casino.Finance.Classes;

public class FixedArray<T>(uint size) : IEnumerable<T>
    where T : notnull
{
    public T[] array { get; } = new T[size];

    private uint index = 0;

    public void Add(T item)
    {
        if (index >= size) throw new IndexOutOfRangeException();

        array[index++] = item;
    }

    public void Remove(T item)
    {
        int i = IndexOf(item);

        if (i == -1) return;

        Array.Copy(array, i + 1, array, i, index - i - 1);

        index--;
    }

    public int IndexOf(T item)
        => Array.IndexOf(array, item);

    public bool Contains(T item)    
        => array.Contains(item);

    public IEnumerator<T> GetEnumerator()
    => ((IEnumerable<T>)array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
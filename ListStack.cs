#nullable enable

using System.Collections.Generic;

public class ListStack<T> : List<T> {
    public void Push(T item) => Add(item);
    public T Peek() => this[Count - 1];
    public bool TryPeek(out T item) {
        item = default!;
        if (IsEmpty) return false;
        item = Peek();
        return true;
    }
    public T? TryPeek() => IsEmpty ? default : Peek();
    public T Pop() {
        T ret = Peek();
        RemoveAt(Count - 1);
        return ret;
    }
    public bool IsEmpty => Count == 0;
}

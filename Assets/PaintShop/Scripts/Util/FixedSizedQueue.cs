using System;
using System.Collections.Concurrent;

/// <summary>
/// A queue with a fixed size.
/// </summary>
[Serializable]
public class FixedSizedQueue<T>
{
    public ConcurrentQueue<T> q = new();
    private object lockObject = new();

    public int Limit { get; set; }

    public void Enqueue(T obj)
    {
        q.Enqueue(obj);
        lock (lockObject)
        {
            T overflow;
            while (q.Count > Limit && q.TryDequeue(out overflow)) ;
        }
    }
}
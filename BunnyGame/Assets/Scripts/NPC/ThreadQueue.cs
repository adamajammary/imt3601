using System.Threading;
using System.Collections.Generic;

// https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=netframework-4.7
// thread safe queues are apparently not allowed in unity ( ͡° ͜ʖ ͡°)
// Based on: https://gamedev.stackexchange.com/questions/139262/blocking-queue-in-unity

public class BlockingQueue<T> {
    private int _count = 0;
    private bool empty = true;

    private Queue<T> _queue = new Queue<T>();

    public T Dequeue() {
        lock (_queue) {
            _count--;
            if (_count <= 0) empty = true;
            return _queue.Dequeue();           
        }
    }

    public void Enqueue(T data) {
        lock (_queue) {
            _count++;
            empty = false;
            _queue.Enqueue(data);            
        }
    }

    public bool isEmpty() {
        lock (_queue)
            return empty;
    }

    public int count() {
        lock (_queue)
            return _count;
    }
}
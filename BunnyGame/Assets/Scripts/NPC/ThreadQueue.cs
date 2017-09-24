using System.Threading;
using System.Collections.Generic;

// https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=netframework-4.7
// thread safe queues are apparently not allowed in unity ( ͡° ͜ʖ ͡°)
// Based on: https://gamedev.stackexchange.com/questions/139262/blocking-queue-in-unity

public class BlockingQueue<T> {
    private int _count = 0;
    private object _countLock = new object();

    private Queue<T> _queue = new Queue<T>();

    public T Dequeue() {
        lock (_queue) {
            // If we have items remaining in the queue, skip over this. 
            while (_count <= 0) {
                // Release the lock and block on this line until someone
                // adds something to the queue, resuming once they 
                // release the lock again.
                Monitor.Wait(_queue);
            }

            lock (_countLock) {
                _count--;
            }

            return _queue.Dequeue();
        }
    }

    public void Enqueue(T data) {
        lock (_queue) {
            _queue.Enqueue(data);

            lock (_countLock) {
                _count++;
            }          

            // If the consumer thread is waiting for an item
            // to be added to the queue, this will move it
            // to a waiting list, to resume execution
            // once we release our lock.
            Monitor.Pulse(_queue);
        }
    }

    public bool isEmpty() {
        lock (_countLock) {
            return !(_count > 0);
        }
    }
}
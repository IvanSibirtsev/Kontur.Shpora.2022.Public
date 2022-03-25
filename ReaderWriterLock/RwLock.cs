using System;
using System.Threading;

namespace ReaderWriterLock;

public class RwLock : IRwLock
{
    private int readers;
    private int writers;
    private readonly object lockObject;
		
    public RwLock()
    {
        readers = 0;
        writers = 0;
        lockObject = new object();
    }
		
    public void ReadLocked(Action action)
    {
        ActionWithLock(() =>
        {
            while (writers > 0)
                Monitor.Wait(lockObject);
            readers++;
        });
			
        action();
			
        ActionWithLock(() =>
        {
            readers--;
            if (readers == 0)
                Monitor.PulseAll(lockObject);
        });
    }

    public void WriteLocked(Action action)
    {
        ActionWithLock(() =>
        {
            while (writers > 0 || readers > 0)
                Monitor.Wait(lockObject);
            writers++;
        });
			
        action();

        ActionWithLock(() =>
        {
            writers--;
            if (readers == 0 && writers == 0)
                Monitor.PulseAll(lockObject);
        });
    }

    private void ActionWithLock(Action action)
    {
        lock (lockObject)
        {
            action();
        }
    }
}
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, Delegate> eventTable = new();

    public static void Subscribe<T>(Action<T> listener)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
            eventTable[typeof(T)] = Delegate.Combine(del, listener);
        else
            eventTable[typeof(T)] = listener;
    }

    public static void Unsubscribe<T>(Action<T> listener)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            var currentDel = Delegate.Remove(del, listener);
            if (currentDel == null) eventTable.Remove(typeof(T));
            else eventTable[typeof(T)] = currentDel;
        }
    }

    public static void Publish<T>(T message)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
            (del as Action<T>)?.Invoke(message);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hook system for registering actions and filters.
/// Inspired by WordPress hooks.
/// </summary>
public static class bl_Hook
{
    // Actions: callbacks that take an array of objects.
    private static readonly Dictionary<string, SortedList<int, List<Action<object[]>>>> actions = new();

    // Filters: callbacks that take and return an object.
    private static readonly Dictionary<string, SortedList<int, List<Func<object, object>>>> filters = new();

    /// <summary>
    /// Registers a callback action to be executed for a specified hook name with an optional priority.
    /// </summary>
    /// <remarks>This method allows multiple callbacks to be registered for the same hook name. Callbacks are
    /// grouped by their priority, and within the same priority level, they are executed in the order they were
    /// added.</remarks>
    /// <param name="hookName">The name of the hook to associate the callback with. Cannot be null or empty.</param>
    /// <param name="callback">The action to execute when the hook is triggered. Cannot be null.</param>
    /// <param name="priority">The priority of the callback within the hook. Callbacks with lower priority values are executed first. Defaults
    /// to <see langword="10"/>.</param>
    public static void AddAction(string hookName, Action<object[]> callback, int priority = 10)
    {
        if (!actions.TryGetValue(hookName, out var priorityList))
        {
            priorityList = new SortedList<int, List<Action<object[]>>>();
            actions[hookName] = priorityList;
        }
        if (!priorityList.TryGetValue(priority, out var callbackList))
        {
            callbackList = new List<Action<object[]>>();
            priorityList.Add(priority, callbackList);
        }
        callbackList.Add(callback);
    }

    /// <summary>
    /// Adds a filter function to be executed for a specified hook, with an optional priority level.
    /// </summary>
    /// <remarks>Filters are grouped by their priority levels and executed in ascending order of priority.
    /// Multiple filters with the same priority are executed in the order they were added.</remarks>
    /// <param name="hookName">The name of the hook to which the filter function will be added. This value cannot be <see langword="null"/> or
    /// empty.</param>
    /// <param name="filter">A function that processes an input object and returns a modified or processed object. This value cannot be <see
    /// langword="null"/>.</param>
    /// <param name="priority">The priority level of the filter. Filters with lower priority values are executed earlier. The default value is
    /// 10.</param>
    public static void AddFilter(string hookName, Func<object, object> filter, int priority = 10)
    {
        if (!filters.TryGetValue(hookName, out var priorityList))
        {
            priorityList = new SortedList<int, List<Func<object, object>>>();
            filters[hookName] = priorityList;
        }
        if (!priorityList.TryGetValue(priority, out var filterList))
        {
            filterList = new List<Func<object, object>>();
            priorityList.Add(priority, filterList);
        }
        filterList.Add(filter);
    }

    /// <summary>
    /// Removes a callback action from the specified hook at the given priority level.
    /// </summary>
    /// <remarks>If the specified priority level or hook does not exist, or if the callback is not registered
    /// at the given priority,  the method returns <see langword="false"/> without making any changes.</remarks>
    /// <param name="hookName">The name of the hook from which the callback should be removed. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="callback">The callback action to remove. Cannot be <see langword="null"/>.</param>
    /// <param name="priority">The priority level of the callback to remove. Defaults to 10.</param>
    /// <returns><see langword="true"/> if the callback was successfully removed; otherwise, <see langword="false"/> if the
    /// callback was not found.</returns>
    public static bool RemoveAction(string hookName, Action<object[]> callback, int priority = 10)
    {
        if (actions.TryGetValue(hookName, out var priorityList) &&
            priorityList.TryGetValue(priority, out var callbackList))
        {
            bool removed = callbackList.Remove(callback);
            if (callbackList.Count == 0)
            {
                priorityList.Remove(priority);
                if (priorityList.Count == 0)
                    actions.Remove(hookName);
            }
            return removed;
        }
        return false;
    }

    /// <summary>
    /// Removes a filter from the specified hook at the given priority level.
    /// </summary>
    /// <remarks>If the specified filter is not found at the given priority level for the specified hook, the
    /// method returns <see langword="false"/>. Removing a filter may also remove the associated priority level or hook
    /// if no other filters remain.</remarks>
    /// <param name="hookName">The name of the hook from which the filter should be removed. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="filter">The filter delegate to remove. Cannot be <see langword="null"/>.</param>
    /// <param name="priority">The priority level at which the filter is registered. Defaults to 10.</param>
    /// <returns><see langword="true"/> if the filter was successfully removed; otherwise, <see langword="false"/>.</returns>
    public static bool RemoveFilter(string hookName, Func<object, object> filter, int priority = 10)
    {
        if (filters.TryGetValue(hookName, out var priorityList) &&
            priorityList.TryGetValue(priority, out var filterList))
        {
            bool removed = filterList.Remove(filter);
            if (filterList.Count == 0)
            {
                priorityList.Remove(priority);
                if (priorityList.Count == 0)
                    filters.Remove(hookName);
            }
            return removed;
        }
        return false;
    }

    /// <summary>
    /// Determines whether an action is registered for the specified hook name.
    /// </summary>
    /// <param name="hookName">The name of the hook to check for a registered action. Cannot be <see langword="null"/> or empty.</param>
    /// <returns><see langword="true"/> if an action is registered for the specified <paramref name="hookName"/>; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool HasAction(string hookName)
    {
        return actions.ContainsKey(hookName);
    }

    /// <summary>
    /// Determines whether a filter is registered for the specified hook name.
    /// </summary>
    /// <param name="hookName">The name of the hook to check for a registered filter. Cannot be <see langword="null"/> or empty.</param>
    /// <returns><see langword="true"/> if a filter is registered for the specified hook name; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool HasFilter(string hookName)
    {
        return filters.ContainsKey(hookName);
    }

    /// <summary>
    /// Executes all registered callbacks associated with the specified hook name, in order of priority.
    /// </summary>
    /// <remarks>Callbacks are executed in the order of their assigned priority. If no callbacks are
    /// registered for the specified hook name, the method performs no action.</remarks>
    /// <param name="hookName">The name of the hook for which callbacks should be executed. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="args">An array of arguments to pass to each callback. Can be empty if no arguments are required.</param>
    public static void DoAction(string hookName, params object[] args)
    {
        if (actions.TryGetValue(hookName, out var priorityList))
        {
            foreach (var pair in priorityList)
            {
                foreach (var callback in pair.Value)
                    callback(args);
            }
        }
    }

    /// <summary>
    /// Executes the first registered callback associated with the specified hook name.
    /// </summary>
    /// <remarks>If multiple callbacks are registered for the specified hook, only the first callback  (based
    /// on priority and registration order) will be executed. If no callbacks are registered  for the given hook name,
    /// the method does nothing.</remarks>
    /// <param name="hookName">The name of the hook to execute the callback for. This cannot be null or empty.</param>
    /// <param name="args">An array of arguments to pass to the callback. Can be empty if no arguments are required.</param>
    public static void DoFirstAction(string hookName, params object[] args)
    {
        if (actions.TryGetValue(hookName, out var priorityList))
        {
            foreach (var pair in priorityList)
            {
                foreach (var callback in pair.Value)
                {
                    callback(args);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Applies a series of filters associated with the specified hook name to the provided value.
    /// </summary>
    /// <remarks>Filters are applied in the order of their priority, with lower priority values being
    /// processed first.  Each filter is a delegate that takes the current value and returns a modified value.</remarks>
    /// <param name="hookName">The name of the hook that determines which filters to apply. This value is case-sensitive.</param>
    /// <param name="value">The initial value to be processed by the filters. Each filter in the sequence modifies this value.</param>
    /// <returns>The final value after all applicable filters have been applied. If no filters are associated with the specified
    /// hook name, the original value is returned.</returns>
    public static object ApplyFilters(string hookName, object value)
    {
        if (filters.TryGetValue(hookName, out var priorityList))
        {
            foreach (var pair in priorityList)
            {
                foreach (var filter in pair.Value)
                    value = filter(value);
            }
        }
        return value;
    }

    /// <summary>
    /// Applies a series of filters associated with the specified hook name to the provided value.
    /// </summary>
    /// <remarks>This method allows dynamic modification of the provided value by applying a sequence of
    /// filters associated with the specified hook name. Filters are typically registered elsewhere in the system and
    /// are executed in the order they were added.</remarks>
    /// <typeparam name="T">The type of the value to which the filters will be applied.</typeparam>
    /// <param name="hookName">The name of the hook that determines which filters to apply. Cannot be null or empty.</param>
    /// <param name="value">The initial value to be processed by the filters.</param>
    /// <returns>The value after all applicable filters have been applied.</returns>
    public static T ApplyFilters<T>(string hookName, T value)
    {
        return (T)ApplyFilters(hookName, (object)value);
    }

    /// <summary>
    /// Applies a series of filters associated with the specified hook name to the given value and returns the first
    /// non-null result.
    /// </summary>
    /// <remarks>Filters are applied in the order of their priority, and within the same priority level,  in
    /// the order they were added. This method stops processing as soon as a non-null result  is produced by a
    /// filter.</remarks>
    /// <param name="hookName">The name of the hook used to retrieve the associated filters.</param>
    /// <param name="value">The initial value to be processed by the filters.</param>
    /// <returns>The first non-null result returned by the filters. If no filters are associated with the  specified hook name or
    /// all filters return null, the original <paramref name="value"/> is returned.</returns>
    public static object ApplyFiltersUntilNonNull(string hookName, object value)
    {
        if (filters.TryGetValue(hookName, out var priorityList))
        {
            foreach (var pair in priorityList)
            {
                foreach (var filter in pair.Value)
                {
                    var result = filter(value);
                    if (result != null)
                        return result;
                }
            }
        }
        return value;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Clear all registered actions and filters when the game starts.
        // required for the editor to work properly when hot-reloading scripts.
        actions.Clear();
        filters.Clear();
    }
}

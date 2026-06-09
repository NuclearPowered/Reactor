using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Reactor.Utilities;

/// <summary>
/// A wrapper for IL2CPP state machine objects to access their parent instance and state.
/// This class is useful for working with IL2CPP state machines across different game versions.
/// To use it, pass the instance object of the state machine into the constructor. To access the parent class,
/// use the Instance property. To access the current state of the state machine, use the State property.
/// </summary>
/// <typeparam name="T">The type of the parent class that owns the state machine.</typeparam>
public class Il2CppStateMachineWrapper<T> : Il2CppCompilerGeneratedObjectWrapper
{
    // normally it is fields, but IL2CPP turns them into properties
    private readonly PropertyInfo _thisProperty;
    private readonly PropertyInfo _stateProperty;

    private T? _parentInstance;

    /// <summary>
    /// Gets the instance of the parent class that owns the state machine.
    /// </summary>
    public T Instance => _parentInstance ??= (T) _thisProperty.GetValue(GeneratedObject)!;

    /// <summary>
    /// Gets or sets the current state of the state machine.
    /// </summary>
    /// <returns>The current state as an integer.</returns>
    public int State
    {
        get => (int) _stateProperty.GetValue(GeneratedObject)!;
        set => _stateProperty.SetValue(GeneratedObject, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Il2CppStateMachineWrapper{T}"/> class.
    /// </summary>
    /// <param name="stateMachine">The instance object of the state machine being wrapped.</param>
    public Il2CppStateMachineWrapper(object stateMachine) : base(stateMachine)
    {
        // The names of these properties are implementation details of the IL compiler used by Unity.
        // They should be stable as long as Unity sticks to mono.
        _thisProperty = AccessTools.Property(GeneratedType, "__4__this");
        _stateProperty = AccessTools.Property(GeneratedType, "__1__state");

        if (_thisProperty == null || _stateProperty == null)
        {
            throw new MissingMemberException($"Could not find required properties in type '{GeneratedType}'.");
        }
    }

    /// <summary>
    /// Gets a parameter from the state machine by its name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to retrieve.</param>
    /// <typeparam name="TField">The type of the parameter to retrieve.</typeparam>
    /// <returns>>The value of the specified parameter.</returns>
    /// <exception cref="MissingFieldException">Thrown if the specified parameter does not exist.</exception>
    public TField GetParameter<TField>(string parameterName)
    {
        return GetField<TField>(parameterName);
    }

    /// <summary>
    /// Sets a parameter in the state machine by its name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to set.</param>
    /// <param name="value">The value to set for the parameter.</param>
    /// <typeparam name="TField">The type of the parameter to set.</typeparam>
    /// <exception cref="MissingFieldException">Thrown if the specified parameter does not exist.</exception>
    public void SetParameter<TField>(string parameterName, TField value)
    {
        SetField(parameterName, value);
    }

    /// <summary>
    /// Attempts to retrieve the MoveNext method of a state machine for the specified method name.
    /// </summary>
    /// <param name="methodName">The name of the method whose state machine MoveNext method is to be retrieved.</param>
    /// <returns>The MoveNext <see cref="MethodBase"/> if found; otherwise, null.</returns>
    public static MethodBase? GetStateMachineMoveNext(string methodName)
    {
        var typeName = typeof(T).FullName;
        var showRoleStateMachine =
            typeof(T)
                .GetNestedTypes()
                .FirstOrDefault(x => x.Name.Contains(methodName));

        if (showRoleStateMachine == null)
        {
            Error($"Failed to find {methodName} state machine for {typeName}");
            return null;
        }

        var moveNext = AccessTools.Method(showRoleStateMachine, "MoveNext");
        if (moveNext == null)
        {
            Error($"Failed to find MoveNext method for {typeName}.{methodName}");
            return null;
        }

        Info($"Found {methodName}.MoveNext");
        return moveNext;
    }
}

using System.Reflection;
using CSharp_Exam.Casino.Attributes;
using CSharp_Exam.Casino.Services.Exceptions;
using CSharp_Exam.Casino.Services.Types;

namespace CSharp_Exam.Casino.Services;

public static class DataService
{

    private static Dictionary<ActionType, Dictionary<string, List<Action>>> _DataMethods = new()
    {
        [ActionType.Save] = new(),
        [ActionType.Load] = new()
    };

    private static void AddMethod(ActionType actionType, string className, Action method)
    {
        var action_methods = _DataMethods[actionType];
        if (action_methods.TryGetValue(className, out var methods))
            methods.Add(method);
        else
            action_methods.Add(className, [method]);
    }

    public static void Register(object obj)
    {
        Type type = obj.GetType();

        foreach (var method in type.GetMethods())
        {
            Attribute? attribute = (Attribute?)method.GetCustomAttribute<SaveAttribute>()
                ?? method.GetCustomAttribute<LoadAttribute>();

            if (attribute is null) continue;

            var action = Delegate.CreateDelegate(typeof(Action), obj, method) as Action;
            if (action is null) continue;

            if (attribute is SaveAttribute saveAttribute)
                AddMethod(ActionType.Save, saveAttribute.ClassName, action);
            else if (attribute is LoadAttribute loadAttribute)
                AddMethod(ActionType.Load, loadAttribute.ClassName, action);
        }
    }

    public static void Unregister(object obj)
    {
        Type type = obj.GetType();
        foreach (var method in type.GetMethods())
        {
            Attribute? attribute = (Attribute?)method.GetCustomAttribute<SaveAttribute>()
                ?? method.GetCustomAttribute<LoadAttribute>();

            if (attribute is null) continue;

            var action = Delegate.CreateDelegate(typeof(Action), obj, method) as Action;
            if (action is null) continue;

            if (attribute is SaveAttribute saveAttribute)
                _DataMethods[ActionType.Save][saveAttribute.ClassName].Remove(action);
            else if (attribute is LoadAttribute loadAttribute)
                _DataMethods[ActionType.Load][loadAttribute.ClassName].Remove(action);
        }
    }

    public static void ExecuteForClass(string className, ActionType actionType)
    {
        if (!_DataMethods[actionType].TryGetValue(className, out var methods) || methods.Count == 0)
            throw new DataOperationFailed(actionType, $"No methods registered for class '{className}'");

        foreach (var method in methods)
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                throw new DataOperationFailed(
                    actionType,
                    $"Method '{method.Method.Name}' failed: {ex.Message}"
                );
            }
        }
    }

    public static void ExecuteAll(ActionType actionType)
    {
        foreach (var classMethods in _DataMethods[actionType])
        {
            foreach (var method in classMethods.Value)
            {
                try
                {
                    method();
                }
                catch (Exception ex)
                {
                    throw new DataOperationFailed(
                        actionType,
                        $"Method '{method.Method.Name}' for class '{classMethods.Key}' failed: {ex.Message}"
                    );
                }
            }
        }
    }
}

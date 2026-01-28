using System;
using System.Collections.Generic;
using NodeEditorLogic.Models;

namespace NodeEditorLogic.Models;

public sealed class LogicComponentDefinition
{
    public LogicComponentDefinition(
        string id,
        string title,
        string category,
        int propagationDelay,
        IReadOnlyList<string> inputs,
        IReadOnlyList<string> outputs,
        Func<IReadOnlyList<LogicValue>, LogicValue[]> evaluate)
    {
        Id = id;
        Title = title;
        Category = category;
        PropagationDelay = propagationDelay;
        Inputs = inputs;
        Outputs = outputs;
        Evaluate = evaluate;
    }

    public string Id { get; }

    public string Title { get; }

    public string Category { get; }

    public int PropagationDelay { get; }

    public IReadOnlyList<string> Inputs { get; }

    public IReadOnlyList<string> Outputs { get; }

    public Func<IReadOnlyList<LogicValue>, LogicValue[]> Evaluate { get; }
}

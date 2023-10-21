﻿using Dorbit.Commands.Abstractions;

namespace Dorbit.Commands;

public abstract class CommandContextBase : ICommandContext
{
    public Dictionary<string, object> Arguments { get; set; } = new();
    public abstract void Error(string message);
    public abstract void Success(string message);
    public abstract void Log(string message);

    public object GetArg(string name)
    {
        return Arguments[name];
    }

    public string GetArgAsString(string name)
    {
        return GetArg(name)?.ToString();
    }
}
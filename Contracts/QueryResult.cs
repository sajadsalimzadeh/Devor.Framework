﻿namespace Dorbit.Framework.Models;

public class QueryResult<T> : CommandResult
{
    public T Data { get; set; }

    public QueryResult()
    {
    }

    public QueryResult(T data)
    {
        Data = data;
    }
}
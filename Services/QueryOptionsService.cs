﻿using Dorbit.Attributes;
using Dorbit.Services.Abstractions;
using Dorbit.Utils.Queries;

namespace Dorbit.Services;

[ServiceRegister]
internal class QueryOptionsService : IQueryOptionsService
{
    private readonly Dictionary<string, QueryOptions> _dict = new();

    public QueryOptionsService()
    {

    }

    public IQueryOptionsService AddOptions(Type type, QueryOptions options)
    {
        _dict[type.FullName] = options;
        return this;
    }

    public IQueryOptionsService AddOptions<T>(QueryOptions options)
    {
        return AddOptions(typeof(T), options);
    }

    public IQueryable<T> ApplyTo<T>(IQueryable<T> query)
    {
        var type = typeof(T);
        if (!_dict.ContainsKey(type.FullName)) return query;
        var options = _dict[type.FullName];
        return options.ApplyTo(query);
    }

    public IQueryable<T> ApplyCountTo<T>(IQueryable<T> query)
    {
        var type = typeof(T);
        if (!_dict.ContainsKey(type.FullName)) return query;
        var options = _dict[type.FullName];
        return options.ApplyCountTo(query);
    }
}
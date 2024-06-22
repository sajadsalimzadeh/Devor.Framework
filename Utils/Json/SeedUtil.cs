﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Entities.Abstractions;
using Dorbit.Framework.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Dorbit.Framework.Utils.Json;

public static class SeedUtil
{
    public static async Task SeedAsync<TEntity, TKey>(this IWriterRepository<TEntity,TKey> repository, string filename, 
        Action<TEntity> beforeInsertAction = default,
        Func<TEntity, TEntity, bool> ignorePredicate = default)
        where TEntity : class, IEntity<TKey>
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
        var content = await File.ReadAllTextAsync(path);
        var items = JsonConvert.DeserializeObject<List<TEntity>>(content);
        var existsItems = await repository.Set().ToListAsync();
        if (ignorePredicate is not null) items = items.Where(x => !existsItems.Any(y => ignorePredicate(x, y))).ToList();
        using var transaction = repository.DbContext.BeginTransaction();
        foreach (var entity in items.Where(x => !existsItems.Contains(x)))
        {
            if (beforeInsertAction is not null) beforeInsertAction(entity);
            await repository.InsertAsync(entity);
        }

        await transaction.CommitAsync();
    }
}
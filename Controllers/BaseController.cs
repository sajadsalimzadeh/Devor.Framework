﻿using AutoMapper;
using Dorbit.Filters;
using Dorbit.Models;
using Dorbit.Services.Abstractions;
using Dorbit.Utils.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Controllers;

[Monitor]
[Route("[controller]"), ApiController]
public abstract class BaseController : ControllerBase
{
    private IServiceProvider _serviceProvider;
    protected IServiceProvider ServiceProvider => _serviceProvider ??= HttpContext.RequestServices;

    private IUserResolver _userResolver;
    protected IUserResolver UserResolver => _userResolver ??= ServiceProvider.GetRequiredService<IUserResolver>();

    protected IMapper Mapper => ServiceProvider.GetService<IMapper>();

    protected Guid? UserId => UserResolver.User?.Id;
    protected QueryOptions QueryOptions => new ODataQueryOptions().Parse(Request);

    protected OperationResult Succeed()
    {
        return new OperationResult(true);
    }

    protected OperationResult Failed(string message)
    {
        return new OperationResult(message);
    }
}
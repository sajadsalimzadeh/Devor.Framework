﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Dorbit.Framework.Commands;

[ServiceRegister(Order = 100)]
public class InstallCommand : Command
{
    private readonly IServiceProvider _serviceProvider;

    public override string Message { get; } = "Install or Update";
    public override int Order { get; } = 100;


    public InstallCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task InvokeAsync(ICommandContext context)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        var entryFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".exe");

        var stopProcess = Process.Start(new ProcessStartInfo("sc", $"stop {assemblyName}")
        {
            Verb = "runas",
            RedirectStandardInput = false,
        });
        await stopProcess?.WaitForExitAsync()!;

        var deleteProcess = Process.Start(new ProcessStartInfo("sc", $"delete {assemblyName}")
        {
            Verb = "runas",
            RedirectStandardInput = false,
        });
        await deleteProcess?.WaitForExitAsync()!;

        var migrationCommandAll = _serviceProvider.GetService<MigrationCommandAll>();
        await migrationCommandAll.InvokeAsync(context);

        var createProcess = Process.Start(new ProcessStartInfo("sc", $"create {assemblyName} binpath= \"{entryFilename} run\" start= auto")
        {
            Verb = "runas",
            RedirectStandardInput = false,
        });
        context.Log($"\n==========Creating Service==========\n");
        await createProcess?.WaitForExitAsync()!;

        var startProcess = Process.Start(new ProcessStartInfo("sc", $"start {assemblyName}")
        {
            Verb = "runas",
            RedirectStandardInput = false,
        });
        context.Log($"\n==========Starting Service==========\n");
        await startProcess?.WaitForExitAsync()!;
    }
}
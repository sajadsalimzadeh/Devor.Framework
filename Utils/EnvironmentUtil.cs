using System;
using Microsoft.Extensions.Hosting;

namespace Dorbit.Framework.Utils;

public static class EnvironmentUtil
{
    public static string GetEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
#if DEBUG
        if (string.IsNullOrEmpty(environment)) environment = Environments.Development;
#else
        if(string.IsNullOrEmpty(environment)) environment = Environments.Production;
#endif
        return environment?.ToLower() ?? "development";
    }

    public static bool IsDevelopment()
    {
        return string.Equals(GetEnvironment(), Environments.Development, StringComparison.CurrentCultureIgnoreCase);
    }
}
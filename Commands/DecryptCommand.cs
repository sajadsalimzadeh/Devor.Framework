﻿using Dorbit.Framework.Attributes;
using Dorbit.Framework.Commands.Abstractions;
using Dorbit.Framework.Models.Commands;
using Dorbit.Framework.Utils.Cryptography;

namespace Dorbit.Framework.Commands;

[ServiceRegister]
public class DecryptCommand : Command
{
    public override string Message => "Decrypt String";

    public override IEnumerable<CommandParameter> GetParameters(ICommandContext context)
    {
        yield return new CommandParameter("Input", "Input");
        yield return new CommandParameter("Key", "Key");
    }

    public override Task Invoke(ICommandContext context)
    {
        var cypherText = new Aes().Decrypt(context.Arguments["Input"].ToString(), context.Arguments["Key"].ToString());
        context.Log($"{cypherText}\n");
        return Task.CompletedTask;
    }
}
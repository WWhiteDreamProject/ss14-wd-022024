﻿using Content.Server.Administration.Managers;
using Content.Server.White.PandaSocket.Interfaces;
using Content.Server.White.PandaSocket.Main;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.White.PandaSocket.Commands;

public sealed class PandaJobBanCommand : IPandaCommand
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Name => "jobban";
    public Type RequestMessageType => typeof(UtkaJobBanRequest);
    public void Execute(IPandaStatusHandlerContext context, PandaBaseMessage baseMessage)
    {
        if (baseMessage is not UtkaJobBanRequest message) return;
        IoCManager.InjectDependencies(this);

        var target = message.Ckey!;
        var job = message.Type!;
        var reason = message.Reason!;
        var minutes = (uint) message.Duration!;
        var isGlobalBan = (bool) message.Global!;
        var admin = message.ACkey!;

        var roleBanManager = IoCManager.Resolve<RoleBanManager>();

        if (_prototypeManager.TryIndex<DepartmentPrototype>(job, out var departmentProto))
            roleBanManager.UtkaCreateDepartmentBan(admin, target, departmentProto, reason, minutes, isGlobalBan, context);

        else
            roleBanManager.UtkaCreateJobBan(admin, target, job, reason, minutes, isGlobalBan, context);
    }

    public void Response(IPandaStatusHandlerContext context, PandaBaseMessage? message = null)
    {
    }
}

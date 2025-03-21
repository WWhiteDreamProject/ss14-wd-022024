﻿using Content.Server.Administration;
using Content.Server.Database;
using Content.Server.White.PandaSocket.Interfaces;
using Content.Server.White.PandaSocket.Main;

namespace Content.Server.White.PandaSocket.Commands;

public sealed class PandaUnbanCommand : IPandaCommand
{
    public string Name => "unban";
    public Type RequestMessageType => typeof(UtkaUnbanRequest);
    public async void Execute(IPandaStatusHandlerContext context, PandaBaseMessage baseMessage)
    {
        if (baseMessage is not UtkaUnbanRequest message) return;

        var dbMan = IoCManager.Resolve<IServerDbManager>();
        var locator = IoCManager.Resolve<IPlayerLocator>();
        IoCManager.InjectDependencies(this);

        var located = await locator.LookupIdByNameOrIdAsync(message.ACkey!);
        if (located == null)
        {
            UtkaSendResponse(false, context);
            return;
        }
        var player = located.UserId;

        var banId = (int) message.Bid!;
        var ban = await dbMan.GetServerBanAsync(banId);

        if (ban == null || ban.Unban != null)
        {
            UtkaSendResponse(false, context);
            return;
        }

        var adminData = await dbMan.GetAdminDataForAsync(player);
        if (adminData?.AdminRank == null || ban.ServerName != "unknown" && adminData.AdminServer is not (null or "unknown") && adminData.AdminServer != ban.ServerName)
        {
            UtkaSendResponse(false, context);
            return;
        }

        await dbMan.AddServerUnbanAsync(new ServerUnbanDef(banId, player, DateTimeOffset.Now));

        UtkaSendResponse(true, context);
    }

    public void Response(IPandaStatusHandlerContext context, PandaBaseMessage? message = null)
    {
        context.RespondJsonAsync(message!);
    }

    private void UtkaSendResponse(bool unbanned, IPandaStatusHandlerContext context)
    {
        var utkaResponse = new UtkaUnbanResponse()
        {
            Unbanned = unbanned
        };

        Response(context, utkaResponse);
    }
}

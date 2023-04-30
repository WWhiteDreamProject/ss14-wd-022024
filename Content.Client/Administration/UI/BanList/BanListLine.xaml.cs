﻿using Content.Shared.Administration.BanList;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Administration.UI.BanList;

[GenerateTypedNameReferences]
public sealed partial class BanListLine : BoxContainer
{
    public readonly SharedServerBan Ban;

    public event Func<BanListLine, bool>? OnIdsClicked;

    public BanListLine(SharedServerBan ban)
    {
        RobustXamlLoader.Load(this);

        Ban = ban;

        IdsHidden.OnPressed += IdsPressed;

        Reason.Text = ban.Reason;
        BanTime.Text = FormatDate(ban.BanTime);
        Expires.Text = ban.ExpirationTime == null
            ? Loc.GetString("ban-list-permanent")
            : FormatDate(ban.ExpirationTime.Value);

        if (ban.Unban is { } unban)
        {
            var unbanned = Loc.GetString("ban-list-unbanned", ("date", FormatDate(unban.UnbanTime)));
            var unbannedBy = unban.UnbanningAdmin == null
                ? string.Empty
                : $"\n{Loc.GetString("ban-list-unbanned-by", ("unbanner", unban.UnbanningAdmin))}";

            Expires.Text += $"\n{unbanned}{unbannedBy}";
        }

        BanningAdmin.Text = ban.BanningAdminName;
        ServerName.Text = ban.ServerName == "unknown" ? "GLOBAL" : ban.ServerName;
    }

    private static string FormatDate(DateTimeOffset date)
    {
        return date.ToString("MM/dd/yyyy h:mm tt");
    }

    private void IdsPressed(BaseButton.ButtonEventArgs buttonEventArgs)
    {
        OnIdsClicked?.Invoke(this);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        IdsHidden.OnPressed -= IdsPressed;
        OnIdsClicked = null;
    }
}

﻿using Content.Shared.White.Cyborg.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.White.Cyborg.CyborgConsole;

[GenerateTypedNameReferences]
public sealed partial class CyborgConsoleUnitInformationContainer : PanelContainer
{
    public CyborgConsoleUnitInformationContainer()
    {
        RobustXamlLoader.Load(this);
    }


    private FormattedMessage _genYesNo(bool boolean)
    {
        var formatted = new FormattedMessage();
        if (boolean)
        {
            formatted.PushColor(Color.Green);
            formatted.AddText(Loc.GetString("cyborg-console-yes"));
        }
        else
        {
            formatted.PushColor(Color.Red);
            formatted.AddText(Loc.GetString("cyborg-console-no"));
        }

        return formatted;
    }

    public void AddButton(ActionData action, Action<BaseButton.ButtonEventArgs> func)
    {
        var button = new Button();
        if (action.Important)
            button.StyleClasses.Add("ButtonColorRed");

        button.Text = action.Name;
        button.OnPressed += func;
        ButtonContainer.AddChild(button);
    }

    public void SetView(string icon)
    {
        BorgRect.TexturePath = icon;
    }

    public void SetName(string name)
    {
        Name.Text = name;
    }

    public void SetEnergyInfo(string text)
    {
        EnergyLabel.Text = text;
    }

    public void SetEnergyBar(float value, float maxValue)
    {
        EnergyBar.Value = value / maxValue;
    }

    public void SetCyborgActiveInfo(bool isActive)
    {
        var isActiveMessage = new FormattedMessage();
        isActiveMessage.AddText(Loc.GetString("cyborg-console-active"));
        isActiveMessage.AddMessage(_genYesNo(isActive));
        CyborgActive.SetMessage(isActiveMessage);
    }

    public void SetCyborgPanelLockedInfo(bool isLocked)
    {
        var isLockedMessage = new FormattedMessage();
        isLockedMessage.AddText(Loc.GetString("cyborg-console-panel-locked"));
        isLockedMessage.AddMessage(_genYesNo(isLocked));
        CyborgPanelLocked.SetMessage(isLockedMessage);
    }

    public void SetCyborgAlive(bool isAlive)
    {
        var isAliveMessage = new FormattedMessage();
        isAliveMessage.AddText(Loc.GetString("cyborg-console-alive"));
        isAliveMessage.AddMessage(_genYesNo(isAlive));
        CyborgPanelLocked.SetMessage(isAliveMessage);
    }

    public void SetCyborgFreeze(bool isFreeze)
    {
        var isFreezeMessage = new FormattedMessage();
        isFreezeMessage.AddText(Loc.GetString("cyborg-console-freeze"));
        isFreezeMessage.AddMessage(_genYesNo(isFreeze));
        CyborgPanelLocked.SetMessage(isFreezeMessage);
    }
}

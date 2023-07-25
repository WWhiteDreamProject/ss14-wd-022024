﻿using System.Linq;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.White.Cyborg.CyborgConsole;

[GenerateTypedNameReferences]
public sealed partial class CyborgConsoleLawsControl : PanelContainer
{
    private readonly CyborgConsoleBoundUserInterface _cyborgConsoleBoundUserInterface;
    private readonly List<string> _laws;
    public CyborgConsoleLawsControl(EntityUid uid,List<string> laws,CyborgConsoleBoundUserInterface cyborgConsoleBoundUserInterface)
    {
        RobustXamlLoader.Load(this);
        _cyborgConsoleBoundUserInterface = cyborgConsoleBoundUserInterface;
        _laws = laws;

        for (var i = 0; i < laws.Count; i++)
        {
            AppendLaws(uid,i);
        }

        AddLawButton.OnPressed += args =>
        {
            if(int.TryParse(IndexInput.Text, out var index))
                _cyborgConsoleBoundUserInterface.SendAddLawMessage(uid,LawInput.Text,index-1);
            else
                _cyborgConsoleBoundUserInterface.SendAddLawMessage(uid,LawInput.Text);
        };
    }

    public void AppendLaws(EntityUid uid,int index)
    {
        var law = _laws[index];

        var container = new CyborgConsoleLawsControlContainer();
        container.SetTitle(Loc.GetString("silicon-laws-law-heading",
            ("lawDisplayNumber", index+1)));
        container.SetDescription(law);

        if(index > 0)
        {
            container.OnRaiseClicked(args =>
                _cyborgConsoleBoundUserInterface.SendReIndexLawMessage(uid,index,index-1)
            );
        }

        if(index < _laws.Count-1)
        {
            container.OnLowClicked(args =>
                _cyborgConsoleBoundUserInterface.SendReIndexLawMessage(uid,index,index+1)
            );
        }

        container.OnRemoveClicked(args =>
            _cyborgConsoleBoundUserInterface.SendRemoveLawMessage(uid,index)
        );

        Laws.AddChild(container);
    }

}

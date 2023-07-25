﻿using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.White.Cult.UI.SummonCultistList;

[GenerateTypedNameReferences]
public partial class SummonCultistListWindow : DefaultWindow
{
    public Action<int, int>? ItemSelected;

    public SummonCultistListWindow()
    {
        RobustXamlLoader.Load(this);
    }

    public void PopulateList(List<int> items, List<string> labels)
    {
        ItemsContainer.RemoveAllChildren();

        var count = Math.Min(items.Count, labels.Count);

        for (var i = 0; i < count; i++)
        {
            var item = items[i];
            var button = new Button();

            button.Text = labels[i];

            button.OnPressed += _ => ItemSelected?.Invoke(item, items.IndexOf(item));

            ItemsContainer.AddChild(button);
        }
    }
}

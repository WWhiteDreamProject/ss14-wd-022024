using System.Numerics;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Client.White.Rules;
using Content.Shared.White;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;

namespace Content.Client.Info
{
    public sealed class RulesAndInfoWindow : DefaultWindow
    {
        [Dependency] private readonly IResourceCache _resourceManager = default!;
        // WD EDIT
        // [Dependency] private readonly RulesManager _rules = default!;
        [Dependency] private readonly IUriOpener _uri = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        // WD EDIT END

        public RulesAndInfoWindow()
        {
            IoCManager.InjectDependencies(this);

            Title = Loc.GetString("ui-info-title");

            var rootContainer = new TabContainer();

            var rulesList = new Info();
            var tutorialList = new Info();

            rootContainer.AddChild(rulesList);
            rootContainer.AddChild(tutorialList);

            TabContainer.SetTabTitle(rulesList, Loc.GetString("ui-info-tab-rules"));
            TabContainer.SetTabTitle(tutorialList, Loc.GetString("ui-info-tab-tutorial"));

            // WD EDIT
            // AddSection(rulesList, _rules.RulesSection());
            var rulesWikiSection = new RulesWikiSection();
            rulesList.InfoContainer.AddChild(rulesWikiSection);
            rulesWikiSection.RulesButton.OnPressed += _ => _uri.OpenUri(_cfg.GetCVar(WhiteCVars.RulesWiki));
            // WD EDIT END
            PopulateTutorial(tutorialList);

            Contents.AddChild(rootContainer);

            SetSize = new Vector2(650, 650);
        }

        private void PopulateTutorial(Info tutorialList)
        {
            AddSection(tutorialList, Loc.GetString("ui-info-header-intro"), "Intro.txt");
            var infoControlSection = new InfoControlsSection();
            tutorialList.InfoContainer.AddChild(infoControlSection);
            AddSection(tutorialList, Loc.GetString("ui-info-header-gameplay"), "Gameplay.txt", true);
            AddSection(tutorialList, Loc.GetString("ui-info-header-sandbox"), "Sandbox.txt", true);

            infoControlSection.ControlsButton.OnPressed += _ => UserInterfaceManager.GetUIController<OptionsUIController>().OpenWindow();
        }

        private static void AddSection(Info info, Control control)
        {
            info.InfoContainer.AddChild(control);
        }

        private void AddSection(Info info, string title, string path, bool markup = false)
        {
            AddSection(info, MakeSection(title, path, markup, _resourceManager));
        }

        private static Control MakeSection(string title, string path, bool markup, IResourceManager res)
        {
            return new InfoSection(title, res.ContentFileReadAllText($"/ServerInfo/{path}"), markup);
        }

    }
}

using System.Linq;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.IoC;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.LineEdit;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Client.Administration.UI.Tabs.AdminTab
{
    [GenerateTypedNameReferences]
    [UsedImplicitly]
    public sealed partial class RoleBanWindow : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        private List<CheckBox> CheckBoxes = new();
        private readonly IClientConsoleHost _clientConsoleHost = IoCManager.Resolve<IClientConsoleHost>();
        public RoleBanWindow()
        {
            RobustXamlLoader.Load(this);
            OnNamesChanged();
            PlayerNameLine.OnTextChanged += _ => OnNamesChanged();
            MinutesLine.OnTextChanged += UpdateButtonsText;
            RoleNameLine.OnTextChanged += _ => OnNamesChanged();
            PlayerList.OnSelectionChanged += OnPlayerSelectionChanged;
            SubmitByNameButton.OnPressed += SubmitByNameButtonOnPressed;
            SubmitListButton.OnPressed += SubmitListButtonOnPressed;
            MinutesLine.OnTextChanged += UpdateButtonsText;
            HourButton.OnPressed += _ => AddMinutes(60);
            DayButton.OnPressed += _ => AddMinutes(1440);
            WeekButton.OnPressed += _ => AddMinutes(10080);
            MonthButton.OnPressed += _ => AddMinutes(43200);
            CacheJobs();
        }
        private void CacheJobs()
        {
            var nameScope = FindNameScope();
            var jobs = _prototypeManager.EnumeratePrototypes<JobPrototype>();
            if (nameScope == null)
                return;
            foreach (var job in jobs)
            {
                if (!job.SetPreference)
                    continue;
                var control = nameScope.Find(job.ID);
                if (control is CheckBox)
                    CheckBoxes.Add(FindControl<CheckBox>(job.ID));
            }
        }

        private bool TryGetMinutes(string str, out uint minutes)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                minutes = 0;
                return true;
            }

            return uint.TryParse(str, out minutes);
        }

        private void AddMinutes(uint add)
        {
            if (!TryGetMinutes(MinutesLine.Text, out var minutes))
                return;

            MinutesLine.Text = $"{minutes + add}";
            UpdateButtons(minutes+add);
        }

        private void UpdateButtonsText(LineEditEventArgs obj)
        {
            if (!TryGetMinutes(obj.Text, out var minutes))
                return;
            UpdateButtons(minutes);
        }

        private void UpdateButtons(uint minutes)
        {
            HourButton.Text = $"+1h ({minutes / 60})";
            DayButton.Text = $"+1d ({minutes / 1440})";
            WeekButton.Text = $"+1w ({minutes / 10080})";
            MonthButton.Text = $"+1M ({minutes / 43200})";
        }

        private void OnNamesChanged()
        {
            if (!string.IsNullOrEmpty(PlayerNameLine.Text) && !string.IsNullOrEmpty(RoleNameLine.Text))
            {
                SubmitByNameButton.Disabled = false;
            }
            else
            {
                SubmitByNameButton.Disabled = true;
            }
            SubmitListButton.Disabled = string.IsNullOrEmpty(PlayerNameLine.Text);
        }
        private void OnPlayerSelectionChanged(PlayerInfo? player)
        {
            PlayerNameLine.Text = player?.Username ?? string.Empty;
            OnNamesChanged();
        }

        private void SubmitByNameButtonOnPressed(BaseButton.ButtonEventArgs obj)
        {
            _clientConsoleHost.ExecuteCommand($"roleban \"{PlayerNameLine.Text}\" \"{RoleNameLine.Text}\" \"{CommandParsing.Escape(ReasonLine.Text)}\" \"{MinutesLine.Text}\" \"{GlobalBan.Pressed}\"");
        }
        private void SubmitListButtonOnPressed(BaseButton.ButtonEventArgs obj)
        {
            var pressedCheckBoxes = CheckBoxes.Where(checkbox => checkbox.Pressed);
            foreach (var checkbox in pressedCheckBoxes)
            {
                _clientConsoleHost.ExecuteCommand($"roleban \"{PlayerNameLine.Text}\" \"{checkbox.Name}\" \"{CommandParsing.Escape(ReasonLine.Text)}\" \"{MinutesLine.Text}\" \"{GlobalBan.Pressed}\"");
            }
        }
    }
}

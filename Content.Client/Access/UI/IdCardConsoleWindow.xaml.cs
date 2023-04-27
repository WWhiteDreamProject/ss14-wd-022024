using System.Linq;
using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using static Content.Shared.Access.Components.IdCardConsoleComponent;

namespace Content.Client.Access.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class IdCardConsoleWindow : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private readonly IdCardConsoleBoundUserInterface _owner;

        private readonly Dictionary<string, Button> _accessButtons = new();
        private readonly List<string> _jobPrototypeIds = new();

        private string? _lastFullName;
        private string? _lastJobTitle;
        private string? _lastJobProto;

        public IdCardConsoleWindow(IdCardConsoleBoundUserInterface owner, IPrototypeManager prototypeManager, List<string> accessLevels)
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            _owner = owner;

            FullNameLineEdit.OnTextEntered += _ => SubmitData();
            FullNameLineEdit.OnTextChanged += _ =>
            {
                FullNameSaveButton.Disabled = FullNameSaveButton.Text == _lastFullName;
            };
            FullNameSaveButton.OnPressed += _ => SubmitData();

            JobTitleLineEdit.OnTextEntered += _ => SubmitData();
            JobTitleLineEdit.OnTextChanged += _ =>
            {
                JobTitleSaveButton.Disabled = JobTitleLineEdit.Text == _lastJobTitle;
            };
            JobTitleSaveButton.OnPressed += _ => SubmitData();

            var jobs = _prototypeManager.EnumeratePrototypes<JobPrototype>().ToList();
            jobs.Sort((x, y) => string.Compare(x.LocalizedName, y.LocalizedName, StringComparison.CurrentCulture));

            List<Button> buttonsToAdd = new();
            foreach (var job in jobs)
            {
                if (!job.SetPreference)
                {
                    continue;
                }

                _jobPrototypeIds.Add(job.ID);
                JobPresetOptionButton.AddItem(Loc.GetString(job.Name), _jobPrototypeIds.Count - 1);
            }

            JobPresetOptionButton.OnItemSelected += SelectJobPreset;

            foreach (var access in accessLevels)
            {
                if (!prototypeManager.TryIndex<AccessLevelPrototype>(access, out var accessLevel))
                {
                    Logger.ErrorS(SharedIdCardConsoleSystem.Sawmill, $"Unable to find accesslevel for {access}");
                    continue;
                }

                var newButton = new Button
                {
                    Text = GetAccessLevelName(accessLevel),
                    ToggleMode = true,
                };
                _accessButtons.Add(accessLevel.ID, newButton);
                newButton.OnPressed += _ => SubmitData();
                buttonsToAdd.Add(newButton);
            }

            buttonsToAdd.Sort((x,y) => string.Compare(x.Text, y.Text, StringComparison.Ordinal));

            foreach (var button in buttonsToAdd)
            {
                AccessLevelGrid.AddChild(button);
            }
        }

        private static string GetAccessLevelName(AccessLevelPrototype prototype)
        {
            if (prototype.Name is { } name)
                return Loc.GetString(name);

            return prototype.ID;
        }

        private void ClearAllAccess()
        {
            foreach (var button in _accessButtons.Values)
            {
                if (button.Pressed)
                {
                    button.Pressed = false;
                }
            }
        }

        private void SelectJobPreset(OptionButton.ItemSelectedEventArgs args)
        {
            if (!_prototypeManager.TryIndex(_jobPrototypeIds[args.Id], out JobPrototype? job))
            {
                return;
            }

            JobTitleLineEdit.Text = Loc.GetString(job.Name);
            args.Button.SelectId(args.Id);

            ClearAllAccess();

            // this is a sussy way to do this
            foreach (var access in job.Access)
            {
                if (_accessButtons.TryGetValue(access, out var button))
                {
                    button.Pressed = true;
                }
            }

            foreach (var group in job.AccessGroups)
            {
                if (!_prototypeManager.TryIndex(group, out AccessGroupPrototype? groupPrototype))
                {
                    continue;
                }

                foreach (var access in groupPrototype.Tags)
                {
                    if (_accessButtons.TryGetValue(access, out var button))
                    {
                        button.Pressed = true;
                    }
                }
            }

            SubmitData();
        }

        public void UpdateState(IdCardConsoleBoundUserInterfaceState state)
        {
            PrivilegedIdButton.Text = state.IsPrivilegedIdPresent
                ? Loc.GetString("id-card-console-window-eject-button")
                : Loc.GetString("id-card-console-window-insert-button");

            PrivilegedIdLabel.Text = state.PrivilegedIdName;

            TargetIdButton.Text = state.IsTargetIdPresent
                ? Loc.GetString("id-card-console-window-eject-button")
                : Loc.GetString("id-card-console-window-insert-button");

            TargetIdLabel.Text = state.TargetIdName;

            var interfaceEnabled =
                state.IsPrivilegedIdPresent && state.IsPrivilegedIdAuthorized && state.IsTargetIdPresent;

            var fullNameDirty = _lastFullName != null && FullNameLineEdit.Text != state.TargetIdFullName;
            var jobTitleDirty = _lastJobTitle != null && JobTitleLineEdit.Text != state.TargetIdJobTitle;

            FullNameLabel.Modulate = interfaceEnabled ? Color.White : Color.Gray;
            FullNameLineEdit.Editable = interfaceEnabled;
            if (!fullNameDirty)
            {
                FullNameLineEdit.Text = state.TargetIdFullName ?? string.Empty;
            }

            FullNameSaveButton.Disabled = !interfaceEnabled || !fullNameDirty;

            JobTitleLabel.Modulate = interfaceEnabled ? Color.White : Color.Gray;
            JobTitleLineEdit.Editable = interfaceEnabled;
            if (!jobTitleDirty)
            {
                JobTitleLineEdit.Text = state.TargetIdJobTitle ?? string.Empty;
            }

            JobTitleSaveButton.Disabled = !interfaceEnabled || !jobTitleDirty;

            JobPresetOptionButton.Disabled = !interfaceEnabled;

            foreach (var (accessName, button) in _accessButtons)
            {
                button.Disabled = !interfaceEnabled;
                if (interfaceEnabled)
                {
                    button.Pressed = state.TargetIdAccessList?.Contains(accessName) ?? false;
                }
            }

            var jobIndex = _jobPrototypeIds.IndexOf(state.TargetIdJobPrototype);
            if (jobIndex >= 0)
            {
                JobPresetOptionButton.SelectId(jobIndex);
            }

            _lastFullName = state.TargetIdFullName;
            _lastJobTitle = state.TargetIdJobTitle;
            _lastJobProto = state.TargetIdJobPrototype;
        }

        private void SubmitData()
        {
            // Don't send this if it isn't dirty.
            var jobProtoDirty = _lastJobProto != null &&
                                _jobPrototypeIds[JobPresetOptionButton.SelectedId] != _lastJobProto;

            _owner.SubmitData(
                FullNameLineEdit.Text,
                JobTitleLineEdit.Text,
                // Iterate over the buttons dictionary, filter by `Pressed`, only get key from the key/value pair
                _accessButtons.Where(x => x.Value.Pressed).Select(x => x.Key).ToList(),
                jobProtoDirty ? _jobPrototypeIds[JobPresetOptionButton.SelectedId] : string.Empty);
        }
    }
}

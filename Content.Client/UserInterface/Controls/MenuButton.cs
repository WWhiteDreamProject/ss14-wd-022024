using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Controls;

public sealed class MenuButton : ContainerButton
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    public const string StyleClassLabelTopButton = "topButtonLabel";
    public const string StyleClassRedTopButton = "topButtonLabel";
    private const float CustomTooltipDelay = 0.4f;

    private static readonly Color ColorNormal = Color.FromHex("#878F9B");
    private static readonly Color ColorRedNormal = Color.FromHex("#aC4D5B");
    private static readonly Color ColorHovered = Color.FromHex("#ffffff");
    private static readonly Color ColorRedHovered = Color.FromHex("#FFFFFF");
    private static readonly Color ColorPressed = Color.FromHex("#788C9B");

    private const float HorPad = 8f;
    private const float VerPad = 4f;
    private Color NormalColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedNormal : ColorNormal;
    private Color HoveredColor => HasStyleClass(StyleClassRedTopButton) ? ColorRedHovered : ColorHovered;

    private BoundKeyFunction _function;
    private readonly BoxContainer _root;
    private readonly TextureRect? _buttonIcon;
    private readonly Label? _buttonLabel;

    public string AppendStyleClass { set => AddStyleClass(value); }
    public Texture? Icon { get => _buttonIcon!.Texture; set => _buttonIcon!.Texture = value; }

    public BoundKeyFunction BoundKey
    {
        get => _function;
        set
        {
            _function = value;
            _buttonLabel!.Text = BoundKeyHelper.ShortKeyName(value);
        }
    }

    public BoxContainer ButtonRoot => _root;

    public MenuButton()
    {
        IoCManager.InjectDependencies(this);
        TooltipDelay = CustomTooltipDelay;
        _buttonIcon = new TextureRect()
        {
            TextureScale = new Vector2(1f, 1f),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            VerticalExpand = true,
            Margin = new Thickness(HorPad, VerPad),
            ModulateSelfOverride = NormalColor,
            Stretch = TextureRect.StretchMode.KeepCentered
        };
        _buttonLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HAlignment.Center,
            ModulateSelfOverride = NormalColor,
            StyleClasses = {StyleClassLabelTopButton}
        };
        _root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Children =
            {
                _buttonIcon,
                _buttonLabel
            }
        };
        AddChild(_root);
        ToggleMode = true;
    }

    protected override void EnteredTree()
    {
        _inputManager.OnKeyBindingAdded += OnKeyBindingChanged;
        _inputManager.OnKeyBindingRemoved += OnKeyBindingChanged;
        _inputManager.OnInputModeChanged += OnKeyBindingChanged;
    }

    protected override void ExitedTree()
    {
        _inputManager.OnKeyBindingAdded -= OnKeyBindingChanged;
        _inputManager.OnKeyBindingRemoved -= OnKeyBindingChanged;
        _inputManager.OnInputModeChanged -= OnKeyBindingChanged;
    }


    private void OnKeyBindingChanged(IKeyBinding obj)
    {
        if(string.IsNullOrEmpty(_function.FunctionName)) return; //WD EDIT

        _buttonLabel!.Text = BoundKeyHelper.ShortKeyName(_function);
    }

    private void OnKeyBindingChanged()
    {
        if(string.IsNullOrEmpty(_function.FunctionName)) return; //WD EDIT

        _buttonLabel!.Text = BoundKeyHelper.ShortKeyName(_function);
    }

    protected override void StylePropertiesChanged()
    {
        // colors of children depend on style, so ensure we update when style is changed
        base.StylePropertiesChanged();
        UpdateChildColors();
    }

    private void UpdateChildColors()
    {
        if (_buttonIcon == null || _buttonLabel == null) return;
        switch (DrawMode)
        {
            case DrawModeEnum.Normal:
                _buttonIcon.ModulateSelfOverride = NormalColor;
                _buttonLabel.ModulateSelfOverride = NormalColor;
                break;

            case DrawModeEnum.Pressed:
                _buttonIcon.ModulateSelfOverride = ColorPressed;
                _buttonLabel.ModulateSelfOverride = ColorPressed;
                break;

            case DrawModeEnum.Hover:
                _buttonIcon.ModulateSelfOverride = HoveredColor;
                _buttonLabel.ModulateSelfOverride = HoveredColor;
                break;

            case DrawModeEnum.Disabled:
                break;
        }
    }


    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        UpdateChildColors();
    }
}

﻿using System.Text.RegularExpressions;
using Content.Shared.CCVar;
using Content.Shared.Popups;
using Content.Shared.White;
using Content.Shared.White.Jukebox;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.White.Jukebox;

[GenerateTypedNameReferences]
public sealed partial class TapeCreatorMenu : DefaultWindow
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IFileDialogManager _fileDialogManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private readonly CheZaHuetaSystem _huetaSystem = default!;
    private readonly SharedPopupSystem _popupSystem = default!;

    private bool _fileDialogOpened;
    private double _maxFileSize;
    private double _currentFileSize;
    private readonly List<byte> _songBytes = new();
    private TapeCreatorComponent _component;

    private const double BytesToMegabytes = 0.000001d;

    public TapeCreatorMenu(TapeCreatorComponent component)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _huetaSystem = _entityManager.System<CheZaHuetaSystem>();
        _popupSystem = _entityManager.System<SharedPopupSystem>();

        _cfg.OnValueChanged(WhiteCVars.MaxJukeboxSongSizeInMB, x => _maxFileSize = x, true);

        _component = component;

        LoadSongButton.OnPressed += TryLoadSong;
        UploadSong.OnPressed += OnUploadButtonPressed;
    }

    private void OnUploadButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        if(!CanUploadSong()) return;

        var input = SongNameField.Text;

        string pattern = @"[^a-zA-Zа-яА-Я ]+";
        string replacement = "";

        var songName = Regex.Replace(input, pattern, replacement);
        songName = Regex.Replace(songName, @"\s+", " ");

        var songBytes = _songBytes;

        var msg = new JukeboxSongUploadRequest()
        {
            SongName = songName,
            SongBytes = songBytes,
            TapeCreatorUid = _component.Owner
        };

        _huetaSystem.SendNetMessage(msg);

        _currentFileSize = 0;
        _songBytes.Clear();
        SongNameField.Clear();

        _popupSystem.PopupEntity("Внимание. Начинается запись мозговой активности.", _component.Owner);
    }

    private async void TryLoadSong(BaseButton.ButtonEventArgs obj)
    {
        var fileFilter = new FileDialogFilters(new FileDialogFilters.Group("ogg"));

        var file = await _fileDialogManager.OpenFile(fileFilter);

        if (Disposed) return;

        if(file == null) return;

        _currentFileSize = file.Length * BytesToMegabytes;

        if (_currentFileSize > _maxFileSize)
        {
            _popupSystem.PopupEntity($"Лимит активности мозговых волн превышен на {_currentFileSize - _maxFileSize} мегахрюков", _component.Owner);
            return;
        }

        //TODO: Песня слишком длинная пиздец

        _songBytes.AddRange(file.CopyToArray());
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        CoinsLabel.Text = _component.CoinBalance.ToString();

        if (CanUploadSong())
        {
            UploadSong.Disabled = false;
        }
        else
        {
            UploadSong.Disabled = true;
        }
    }

    private bool CanUploadSong()
    {
        return SongNameField.Text.Length > 0 && _songBytes.Count > 0 && _component.CoinBalance > 0 && _component.InsertedTape.HasValue && !_component.Recording;
    }
}

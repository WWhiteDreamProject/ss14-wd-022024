﻿using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.White.Ryodan.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.White.Ryodan.Systems;

/// <summary>
/// Handles dashing logic including charge consumption and checking attempt events.
/// </summary>
public sealed class DashAbilitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DashAbilityComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<DashAbilityComponent, DashEvent>(OnDash);
    }

    private void OnGetItemActions(EntityUid uid, DashAbilityComponent comp, GetItemActionsEvent args)
    {
        var ev = new AddDashActionEvent(args.User);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return;

        if (!_prototypeManager.TryIndex<WorldTargetActionPrototype>(comp.DashAction, out var action))
            return;

        args.Actions.Add(new WorldTargetAction(action));
    }

    /// <summary>
    /// Handle charges and teleport to a visible location.
    /// </summary>
    private void OnDash(EntityUid uid, DashAbilityComponent comp, DashEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var user = args.Performer;
        args.Handled = true;

        var ev = new DashAttemptEvent(user);
        RaiseLocalEvent(uid, ev);
        if (ev.Cancelled)
            return;

        if (!_hands.IsHolding(user, uid, out var _))
        {
            _popup.PopupEntity(Loc.GetString("dash-ability-not-held", ("item", uid)), user, user);
            return;
        }

        TryComp<LimitedChargesComponent>(uid, out var charges);
        if (_charges.IsEmpty(uid, charges))
        {
            _popup.PopupEntity(Loc.GetString("dash-ability-no-charges", ("item", uid)), user, user);
            return;
        }

        var origin = Transform(user).MapPosition;
        var target = args.Target.ToMap(EntityManager, _transform);

        // prevent collision with the user duh
        if (!_interaction.InRangeUnobstructed(origin, target, 0f, CollisionGroup.Opaque, entityUid => entityUid == user))
        {
            // can only dash if the destination is visible on screen
            _popup.PopupEntity(Loc.GetString("dash-ability-cant-see", ("item", uid)), user, user);
            return;
        }

        _transform.SetCoordinates(user, args.Target);
        _transform.AttachToGridOrMap(user);
        _audio.PlayPvs(comp.BlinkSound, user, AudioParams.Default);
        if (charges != null)
            _charges.UseCharge(uid, charges);
    }
}

/// <summary>
/// Raised on the item before adding the dash action
/// </summary>
public sealed class AddDashActionEvent : CancellableEntityEventArgs
{
    public EntityUid User;

    public AddDashActionEvent(EntityUid user)
    {
        User = user;
    }
}

/// <summary>
/// Raised on the item before dashing is done.
/// </summary>
public sealed class DashAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User;

    public DashAttemptEvent(EntityUid user)
    {
        User = user;
    }
}

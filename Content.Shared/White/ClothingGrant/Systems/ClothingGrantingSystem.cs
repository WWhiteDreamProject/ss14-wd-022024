using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Tag;
using Content.Shared.White.ClothingGrant.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.White.ClothingGrant.Systems;

public sealed class ClothingGrantingSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly INetManager _net = default!; // WD
    [Dependency] private readonly IGameTiming _timing = default!; // WD

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingGrantComponentComponent, GotEquippedEvent>(OnCompEquip);
        SubscribeLocalEvent<ClothingGrantComponentComponent, GotUnequippedEvent>(OnCompUnequip);
        SubscribeLocalEvent<ClothingGrantTagComponent, GotEquippedEvent>(OnTagEquip);
        SubscribeLocalEvent<ClothingGrantTagComponent, GotUnequippedEvent>(OnTagUnequip);
    }

    private void OnCompEquip(EntityUid uid, ClothingGrantComponentComponent component, GotEquippedEvent args)
    {
        if (_net.IsClient && !_timing.IsFirstTimePredicted) // WD-edit
            return; // WD-edit

        if (!TryComp<ClothingComponent>(uid, out var clothing)) return;

        if (!clothing.Slots.HasFlag(args.SlotFlags)) return;

        if (component.Components.Count > 8)
        {
            Logger.Error("Although a component registry supports multiple components, we cannot bookkeep more than 8 component for ClothingGrantComponent at this time.");
            return;
        }

        foreach (var (name, data) in component.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            if (HasComp(args.Equipee, newComp.GetType()))
                continue;

            newComp.Owner = args.Equipee;

            var temp = (object) newComp;
            _serializationManager.CopyTo(data.Component, ref temp);
            EntityManager.AddComponent(args.Equipee, (Component)temp!);
            Dirty(newComp);
        }

        component.IsActive = true;
        Dirty(component);
    }

    private void OnCompUnequip(EntityUid uid, ClothingGrantComponentComponent component, GotUnequippedEvent args)
    {
        if (_net.IsClient && !_timing.IsFirstTimePredicted) // WD-edit
            return; // WD-edit

        if (!component.IsActive) return;

        foreach (var (name, data) in component.Components)
        {
            var newComp = (Component) _componentFactory.GetComponent(name);

            RemComp(args.Equipee, newComp.GetType());
        }

        component.IsActive = false;
        Dirty(component);
    }

    private void OnTagEquip(EntityUid uid, ClothingGrantTagComponent component, GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(uid, out var clothing)) return;

        if (!clothing.Slots.HasFlag(args.SlotFlags)) return;

        EnsureComp<TagComponent>(args.Equipee);
        _tagSystem.AddTag(args.Equipee, component.Tag);

        component.IsActive = true;
        Dirty(component);
    }

    private void OnTagUnequip(EntityUid uid, ClothingGrantTagComponent component, GotUnequippedEvent args)
    {
        if (!component.IsActive) return;

        _tagSystem.RemoveTag(args.Equipee, component.Tag);

        component.IsActive = false;
        Dirty(component);
    }
}

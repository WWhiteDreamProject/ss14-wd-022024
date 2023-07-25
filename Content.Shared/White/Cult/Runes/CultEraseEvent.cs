using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult.Runes;

[Serializable, NetSerializable]
public sealed class CultEraseEvent : SimpleDoAfterEvent
{
    public EntityUid TargetEntityId;
}

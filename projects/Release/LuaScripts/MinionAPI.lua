-- TODO: Trim all the call braces and cleanup the unrollers

local boolean_functions =
    {
        "GetOriginalCharge", "HasCharge", "HasBattlecry",
        "CanBeTargetedByAbilities", "CanBeTargetedByHeroPowers",
        "IsImmune", "IsPoisonous", "IsEnraged", "IsFreeze",
        "IsFrozen", "IsAsleep", "IsStealthed", "HasTaunt",
        "HasDivineShield", "IsHero", "IsHeroPower", "IsMinion",
        "IsSpell", "IsAbility", "IsWeapon", "IsElite",
        "IsExhausted", "IsSecret", "CanAttack", "CanBeAttacked",
        "CanBeTargetedByOpponents", "HasSpellPower",
        "IsAffectedBySpellPower", "HasSpellPowerDouble", "IsDamaged",
        "HasWindfury", "HasCombo", "HasRecall",
        "HasDeathrattle", "IsSilenced", "CanBeDamaged"
    }

for i,value in ipairs(boolean_functions) do
    _G[value] = function(entity)
        coroutine.yield()
        return __csharp_entity_bool(entity, value)
    end
end

local value_functions =
    {
        "GetOriginalCost", "GetOriginalATK", "GetOriginalHealth",
        "GetOriginalDurability", "GetDamage", "GetNumTurnsInPlay",
        "GetNumAttacksThisTurn", "GetSpellPower", "GetCost",
        "GetATK", "GetDurability", "GetZonePosition", "GetArmor",
        "GetFatigue", "GetHealth", "GetRemainingHP"
    }

for i,value in ipairs(value_functions) do
    _G[value] = function(entity)
        coroutine.yield()
        return __csharp_entity_value(entity, value)
    end
end

-- Attack the attackee with the attacker
function DoAttack(Attacker, Attackee)
    coroutine.yield()

    __csharp_do_attack(Attacker)
    __critical_pause = true
    coroutine.yield()

    __csharp_do_attack(Attackee)
    __critical_pause = true
    coroutine.yield()

    return true
end

function DropCard(card)
    coroutine.yield()
    local dropped = false

    __csharp_drop_card(card, true)
    __critical_pause = true
    coroutine.yield()
    
    dropped = __csharp_drop_card(card, false)
    __critical_pause = true
    coroutine.yield()

    return dropped
end

function ConvertCardToEntity(card)
    coroutine.yield()
    local entity = __csharp_convert_to_entity(card)
    return entity
end

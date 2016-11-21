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
        return __csharp_entity_value(entity, value)
    end
end

function ConvertCardToEntity(card)
    local entity = __csharp_convert_to_entity(card)
    return entity
end

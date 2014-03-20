OurBattlefield    = "OUR_BATTLEFIELD"
EnemyBattlefield  = "ENEMY_BATTLEFIELD"
Hand              = "HAND"

OurHero           = "OUR_HERO"
EnemyHero         = "ENEMY_HERO"

__critical_pause = false
__end_turn = false

-- Return a list of cards in a given region
function getCards(where)
    local cards = nil
    if where == OurBattlefield then
        -- return our battlefield cards
        cards = __csharp_get_our_battlefield_cards()
    elseif where == EnemyBattlefield then
        -- return their battlefield cards
        cards = __csharp_get_enemy_battlefield_cards()
    elseif where == Hand then
        -- return cards on hand
        cards = __csharp_get_hand_cards()
    end
    coroutine.yield()
    return cards
end

-- Return the damage for the provided entity
function getAttack(entity)
    local attack = __csharp_get_attack_of_entity(entity)
    coroutine.yield()
    return attack
end

-- Return the health of the provided entity
function getHealth(entity)
    local health = __csharp_get_health_of_entity(entity)
    coroutine.yield()
    return health
end

function getDamage(entity)
    local damage = __csharp_get_damage_of_entity(entity)
    coroutine.yield()
    return damage
end

-- Check if the provided card is a minion
function isMinion(card)
    local is_minion = __csharp_is_card_a_minion(card)
    coroutine.yield()
    return is_minion
end

-- Get the cost of playing of the provided card
function getCost(card)
    local cost =__csharp_get_card_cost(card)
    coroutine.yield()
    return cost
end

-- Return the number of crystals available
function getCrystals(hero)

    local crystals = 0
    if hero == OurHero then
        -- return our crystals
        crystals = __csharp_our_hero_crystals()
    elseif hero == EnemyHero then
        -- return their crystals
        crystals =__csharp_enemy_hero_crystals()
    end
    coroutine.yield()
    return crystals
end

-- Attack the attackee with the attacker
function doAttack(Attacker, Attackee)
    __csharp_do_attack(Attacker)
    __critical_pause = true
    coroutine.yield()

    __csharp_do_attack(Attackee)
    __critical_pause = true
    coroutine.yield()

    return true
end

function getEnemyHero()
    local hero_card = __csharp_enemy_hero_card()
    coroutine.yield()
    return hero_card
end

function dropCard(card)
    local dropped = false
    
    __csharp_drop_card(card, true)
    __critical_pause = true
    coroutine.yield()
    
    dropped = __csharp_drop_card(card, false)
    __critical_pause = true
    coroutine.yield()

    return dropped
end

function convertCardToEntity(card)
    local entity = __csharp_convert_to_entity(card)
    coroutine.yield()
    return entity
end

function isExhausted(entity)
    local is_exhausted = __csharp_is_entity_exhausted(entity)
    coroutine.yield()
    return is_exhausted
end

function canAttack(entity)
    local can_attack = __csharp_can_entity_attack(entity)
    coroutine.yield()
    return can_attack
end

function canBeAttacked(entity)
    local can_be_attacked = __csharp_can_entity_be_attack(entity)
    coroutine.yield()
    return can_be_attacked
end

function isTank(card)
    local is_tank = __csharp_is_card_tank(card)
    coroutine.yield()
    return is_tank
end

function use_hero_power()
    local succes = __use_hero_power()
    coroutine.yield()
    return succes
end

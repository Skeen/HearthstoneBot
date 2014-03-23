dofile(script_path .. "AI_Config.lua")

--[[
-- Return a list of attack rating
local battlefield_attacks = function()
    local battlefield_cards = GetCards(Battlefield)
    local battlefield_attacks = {}

    for card,value in pairs(battlefield_cards) do
        local card_attack = GetATK(card)
        table.insert(battlefield_attacks, card_attack)
    end
    return battlefield_attacks
end

-- Return the total damage we have in the battle field
local battlefield_damage = function()
    local battlefield_attacks = battlefield_attacks()

    local total_attack = 0
    for i, card_attack in ipairs(battlefield_attacks) do
        total_attack = total_attack + card_attack
    end
    return total_attack
end

-- Return whether we have damage enough to kill the enemy hero
local canWin = function()
    -- TODO: This should check cards too, and hero ability, all direct damage 
    local total_damage = battlefield_damage()
    local enemy_hero_health = GetHealth(EnemyHero)
    return (total_damage > enemy_hero_health)
end
--]]
local canEntityAttack = function(entity)
    local can_attack = CanAttack(entity)
    local is_exhausted = IsExhausted(entity)
    local is_frozen = IsFrozen(entity)
    local attack_dmg = GetATK(entity)

    return can_attack and (is_exhausted == false) and (is_frozen == false) and (attack_dmg > 0)
end

local num_battlefield_minions = function()
    local battlefield_cards = GetCards(OurBattlefield)
    return #battlefield_cards
end

local nuke_hero = function()
    local battlefield_cards = GetCards(OurBattlefield)
    for i,card in ipairs(battlefield_cards) do
        local entity = ConvertCardToEntity(card)
        local can_attack = canEntityAttack(entity)

        if (can_attack) then
            local enemy_hero_card = GetCard(EnemyHero)
            DoAttack(card, enemy_hero_card)
        end
    end
end
--[[
local get_enemy_tanks = function()
    local enemy_tanks = {}

    local enemy_battlefield_cards = GetCards(EnemyBattlefield)
    for i,enemy_card in ipairs(enemy_battlefield_cards) do
        local is_tank = HasTaunt(enemy_card)
        -- if it's a tank
        if is_tank == true then
            table.insert(enemy_tanks, enemy_card)
        end
    end

    return enemy_tanks
end
--]]
local get_our_attackers = function()

    local our_attackers = {}

    local our_battlefield_cards = GetCards(OurBattlefield)
    for i,card in ipairs(our_battlefield_cards) do
        local entity = ConvertCardToEntity(card)
        local can_attack = canEntityAttack(entity)

        if (can_attack) then
            table.insert(our_attackers, card)
        end
    end

    return our_attackers
end

local eliminate_tanks = function()

    local enemy_battlefield_cards = GetCards(EnemyBattlefield)
    for i,enemy_card in ipairs(enemy_battlefield_cards) do
        local enemy_entity = ConvertCardToEntity(enemy_card)
        local is_tank = HasTaunt(enemy_entity)
        -- if it's a tank
        if is_tank == true then
            
            print_to_log("Tank found");

            local tank_entity = ConvertCardToEntity(enemy_card)
            local tank_hp_left = GetHealth(tank_entity) - GetDamage(tank_entity)

            print_to_log("Tank has " .. tank_hp_left .. " hp left");

            local best_attacker = nil
            local best_attacker_atk = 100

            local our_attackers = get_our_attackers()
            for i, our_atk in ipairs(our_attackers) do
                local attack_entity = ConvertCardToEntity(our_atk)
                local attack = GetATK(attack_entity)

                if (attack >= tank_hp_left) and (attack < best_attacker_atk) then
                    best_attacker = our_atk
                    best_attacker_atk = attack
                end
            end

            if (best_attacker ~= nil) then
                DoAttack(best_attacker, enemy_card)
                return true
            elseif (#our_attackers ~= 0) then
                best_attacker = table.remove(our_attackers, 1)
                DoAttack(best_attacker, enemy_card)
                return true
            else
                return false;
            end
        end
    end
    return false
end

local throw_random_minion = function()
    
    local cards_on_hand = GetCards(Hand)
    local minion_cards = {}
    -- Find all minion cards
    for i,card in ipairs(cards_on_hand) do
        local entity = ConvertCardToEntity(card)
        local is_minion = IsMinion(entity)
        if is_minion then
            table.insert(minion_cards, card)
        end
    end
    
    --print_to_log(#minion_cards .. " minions on hand")
    -- Find the most expensive one, we can throw down
    local avaiable_crystals = GetCrystals(OurHero)

    --print_to_log("crystal")
    local most_expensive_card = nil
    local most_expensive_card_cost = 0
    local most_expensive_card_index = 0
    local most_expensive_card_is_tank = false
    for i,card in ipairs(minion_cards) do
        --print_to_log("loop entry")
        local entity = ConvertCardToEntity(card)
        local card_cost = GetCost(entity)
        local is_tank = HasTaunt(entity)
        --print_to_log("card cost = " .. card_cost)
        
        if((card_cost > most_expensive_card_cost) and (card_cost <= avaiable_crystals)) then
            if (prefer_tank_minions == true) and (most_expensive_card_is_tank == true) and (is_tank == false) then
                -- We don't want this, as this card is not a tank, so let's do nothing
            else
                --print_to_log("new expensive card")
                most_expensive_card_cost = card_cost
                most_expensive_card = card
                most_expensive_card_index = i
                most_expensive_card_is_tank = is_tank
            end
        end
    end
    --print_to_log("post loop crystal")
    -- We found a card!
    if most_expensive_card ~= nil then
        print_to_log("playable card cost: " .. most_expensive_card_cost)
        DropCard(most_expensive_card)
        return true
    end
    --print_to_log("no playable card")
    return false
end

-- Only for non-targeted
-- TODO: Add a check for these heroes
-- NOTE: Will require an extension of the Lua API
--[[
local use_hero_power = function()
    local hero_power_card = GetCard(HeroPower)
    local entity = ConvertCardToEntity(hero_power_card)

    local cost = GetCost(entity)
    local avaiable_crystals = GetCrystals(OurHero)
    if (cost <= avaiable_crystals) then
        __csharp_do_attack(hero_power_card)
        __critical_pause = true
        coroutine.yield()
        __csharp_drop_card(hero_power_card, true)
        __critical_pause = true
        coroutine.yield()
    end
end
--]]

local non_target_spell = function()
    local cards_on_hand = GetCards(Hand)
    local spell_cards = {}
    -- Find all minion cards
    for i,card in ipairs(cards_on_hand) do
        local entity = ConvertCardToEntity(card)
        local is_minion = IsSpell(entity)
        if is_minion then
            table.insert(spell_cards, card)
        end
    end

    print_to_log(#spell_cards .. " spells on hand")
    local avaiable_crystals = GetCrystals(OurHero)

    for i,card in ipairs(spell_cards) do
        local entity = ConvertCardToEntity(card)
        local card_cost = GetCost(entity)

        if(card_cost <= avaiable_crystals) then
            DropCard(card)
            return true
        end
    end
    return false
end

-- Do AI, returns nothing, takes nothing
turn_start_function = function()

    -- Try to run the AI 3 times, note this is a hacky fix
    -- TODO: Do it the right way
    for i=0,3 do
        print_to_log("AI started" .. i)

        -- Use all spells / secrets
        while non_target_spell() do
        end
        
        -- Throw all minions
        while num_battlefield_minions() < 7 and throw_random_minion() do
        end

        -- Keep killing tanks, while we're able to
        while eliminate_tanks() do
        end

        nuke_hero();

        print_to_log("AI ended" .. i);
    end
    print_to_log("End of Turn Called");
end

-- Mulligan function, should not call any critical pause functions
-- Returns a list of cards to be replaced
function do_mulligan(cards)

    replace = {}
    
    for i, card in ipairs(cards) do
        local entity = ConvertCardToEntity(card)
        if GetCost(entity) >= 4 then
            table.insert(replace, card)
        end
    end

    return replace
end

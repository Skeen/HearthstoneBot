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
            return {AttackEnemy(card, GetCard(EnemyHero))}
        end
    end
	
	return {}
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
                return {AttackEnemy(best_attacker, enemy_card)}
            elseif (#our_attackers ~= 0) then
                best_attacker = table.remove(our_attackers, 1)
                return {AttackEnemy(best_attacker, enemy_card)}
            end
        end
    end
    return {}
end

local throw_random_minion = function()
    -- Check if the battlefield is full
	if num_battlefield_minions() >= 7 then
	    return {}
	end

    local minion_cards = {}
    -- Find all minion cards
    for i,card in ipairs(GetCards(Hand)) do
        local entity = ConvertCardToEntity(card)
        local is_minion = IsMinion(entity)
        if is_minion then
            table.insert(minion_cards, card)
        end
    end
    
    --print_to_log(#minion_cards .. " minions on hand")
    -- Find the most expensive one, we can throw down
    local num_crystals = GetCrystals(OurHero)

    --print_to_log("crystal")
    local most_expensive_card = nil
    local most_expensive_card_cost = 0
    local most_expensive_card_index = 0
    for i,card in ipairs(minion_cards) do
        --print_to_log("loop entry")
        local entity = ConvertCardToEntity(card)
        local card_cost = GetCost(entity)
        --print_to_log("card cost = " .. card_cost)
        
        if((card_cost > most_expensive_card_cost) and (card_cost <= num_crystals)) then
            --print_to_log("new expensive card")
            most_expensive_card_cost = card_cost
            most_expensive_card = card
            most_expensive_card_index = i
        end
    end

	-- Build list of minions to play
	minions = {}

    --print_to_log("post loop crystal")
    -- We found a card!
    if most_expensive_card ~= nil then
        print_to_log("playable card cost: " .. most_expensive_card_cost)
        table.insert(minions, PlayCard(most_expensive_card))
    end

    return minions
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

    -- Get available crystals
    local num_crystals = GetCrystals(OurHero)

	-- Build list of spells to play
	spells = {}

    -- Find a spell card we can cast
    for i,card in ipairs(GetCards(Hand)) do
        local entity = ConvertCardToEntity(card)
        if IsSpell(entity) then
            local card_cost = GetCost(entity)
            if(card_cost <= num_crystals) then
            	table.insert(spells, PlayCard(card))
				break
            end
        end
    end

    return spells
end

-- Return a list of actions to perform.  Turn is ended when no actions are returned here.
-- If actions are returned, the method will be invoked again after actions are performed.
function choose_turn_actions(cards)

    print_to_log("Determining next turn action")

    -- Check if there is a spell to cast
    spell_cards = non_target_spell()
    if next(spell_cards) ~= nil then
        return spell_cards
    end

    -- Check if there is a minion to drop
    minion_cards = throw_random_minion()
    if next(minion_cards) ~= nil then
        return minion_cards
    end

	-- Destroy minions with taunt
	tank_cards = eliminate_tanks()
	if next(tank_cards) ~= nil then
		return tank_cards
	end

	-- Attack enemy hero
	nuke_actions = nuke_hero()
	if next(nuke_actions) ~= nil then
		return nuke_actions
	end

    -- No more actions to perform
    return {}
end

-- Mulligan function, should not call any critical pause functions
-- Returns a list of cards to be replaced
function choose_mulligan_cards(cards)

    replace = {}
    
    for i, card in ipairs(cards) do
        local entity = ConvertCardToEntity(card)
        if GetCost(entity) >= 4 then
            table.insert(replace, card)
        end
    end

    return replace
end

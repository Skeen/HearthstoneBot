-----------------------------------------------------------
-- ARGUMENT VARIABLES BELOW, DO NOT CHANGE THESE, EVER!! --
-----------------------------------------------------------

-- For the getCards function
Hand              = "HAND"
OurBattlefield    = "OUR_BATTLEFIELD"
EnemyBattlefield  = "ENEMY_BATTLEFIELD"

-- For the getCrystals function
OurHero           = "OUR_HERO"
EnemyHero         = "ENEMY_HERO"
-- The above, plus the below, is for the getCard function
HeroPower         = "HERO_POWER"

-----------------------------------------------------------
-- AI FUNCTIONS BELOW, CALL THESE FOR SERVICE            --
-----------------------------------------------------------
-- Return a list of cards in a given region
function GetCards(where)
    return __csharp_cards(where)
end

-- Return a specific card
function GetCard(where)
    return __csharp_card(where)
end

-- Return the number of crystals available
function GetCrystals(hero)
    return __csharp_crystals(hero)
end

-- Return a list of actions to play a card
function PlayCard(card)
    return __csharp_play_card(card)
end

-- Return a list of actions to attack enemy with friendly minion
function AttackEnemy(friendly, enemy)
    return __csharp_attack_enemy(friendly, enemy)
end

-- Load MinionAPI
dofile(script_path .. "MinionAPI.lua")

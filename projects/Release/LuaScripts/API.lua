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

-- For controlling pauses (2 second pause on next yield, if set)
__critical_pause = false

-----------------------------------------------------------
-- AI FUNCTIONS BELOW, CALL THESE FOR SERVICE            --
-----------------------------------------------------------
-- Return a list of cards in a given region
function GetCards(where)
    coroutine.yield()
    return __csharp_cards(where)
end

-- Return a specific card
function GetCard(where)
    coroutine.yield()
    return __csharp_card(where)
end

-- Return the number of crystals available
function GetCrystals(hero)
    coroutine.yield()
    return __csharp_crystals(hero)
end

-- Load MinionAPI
dofile(script_path .. "MinionAPI.lua")

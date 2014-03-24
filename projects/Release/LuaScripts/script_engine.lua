-- Should not be modified by user

function turn_action(cards)
    local actions = choose_turn_actions(cards)
    return actions
end

function mulligan(cards)
    local replace = choose_mulligan_cards(cards)
    return replace
end

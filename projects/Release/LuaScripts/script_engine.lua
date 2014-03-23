-- Should not be modified by user

function turn_action(cards)
    local actions = do_turn_action(cards)
    return actions
end

function mulligan(cards)
    local replace = do_mulligan(cards)
    return replace
end

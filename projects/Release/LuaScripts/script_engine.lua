local co_routine = coroutine.create(turn_start_function)
function turn_start()

    -- Precondition, co_routine is always runnable before this is called
    local status, err = coroutine.resume(co_routine)
    if status == false then
        return err
    end
    -- If it has run all the way through
    if(coroutine.status(co_routine) == "dead") then
        -- Create a new routine
        __critical_pause = false
        co_routine = coroutine.create(turn_start_function)
    end
    return nil
end

function mulligan(cards)

    local old_function = coroutine.yield
    coroutine.yield = function() end

    __critical_pause = false
    local replace = do_mulligan(cards)

    if __critical_pause == true then
        print_to_log("ERROR: CRITICAL PAUSE FUNCTION CALLED WITHIN MULLIGAN")
    end

    coroutine.yield = old_function

    return replace
end

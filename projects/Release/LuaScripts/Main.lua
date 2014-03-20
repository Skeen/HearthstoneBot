function print_to_log(str)
    __csharp_print_to_log(str)
end

print_to_log("Loading API.lua");
dofile(script_path .. "API.lua")
print_to_log("Loading AI.lua");
dofile(script_path .. "AI.lua")
print_to_log("Loading script_engine.lua");
dofile(script_path .. "script_engine.lua")

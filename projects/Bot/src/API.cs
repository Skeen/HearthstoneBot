using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;

using LuaInterface;

namespace HearthstoneBot
{
    public class API
    {
        private Lua lua;

        public void __csharp_print_to_log(string str)
        {
            Log.log(str, "LuaOutput.log");
        }

        public API()
        {   
            Log.log("API constructed");

            try
            {
                lua = new Lua();

                Log.log("Setting script path...");
                string lua_script_path = Plugin.getBothPath() + "LuaScripts/";
                lua["script_path"] = lua_script_path;
                Log.log("Done");
                
                // Print to screen
                lua.RegisterFunction("__csharp_print_to_log", this, typeof(API).GetMethod("__csharp_print_to_log"));

                // Card functions
                lua.RegisterFunction("__csharp_get_our_battlefield_cards", this, typeof(API).GetMethod("__csharp_get_our_battlefield_cards"));
                lua.RegisterFunction("__csharp_get_enemy_battlefield_cards", this, typeof(API).GetMethod("__csharp_get_enemy_battlefield_cards"));
                lua.RegisterFunction("__csharp_get_hand_cards", this, typeof(API).GetMethod("__csharp_get_hand_cards"));
                lua.RegisterFunction("__csharp_drop_card", this, typeof(API).GetMethod("__csharp_drop_card"));
                lua.RegisterFunction("__csharp_enemy_hero_card", this, typeof(API).GetMethod("__csharp_enemy_hero_card"));
                lua.RegisterFunction("__csharp_is_card_tank", this, typeof(API).GetMethod("__csharp_is_card_tank"));

                // Query about card
                lua.RegisterFunction("__csharp_get_attack_of_entity", this, typeof(API).GetMethod("__csharp_get_attack_of_entity"));
                lua.RegisterFunction("__csharp_get_health_of_entity", this, typeof(API).GetMethod("__csharp_get_health_of_entity"));
                lua.RegisterFunction("__csharp_get_damage_of_entity", this, typeof(API).GetMethod("__csharp_get_damage_of_entity"));
                lua.RegisterFunction("__csharp_can_entity_attack", this, typeof(API).GetMethod("__csharp_can_entity_attack"));
                lua.RegisterFunction("__csharp_can_entity_be_attack", this, typeof(API).GetMethod("__csharp_can_entity_be_attack"));
                lua.RegisterFunction("__csharp_is_card_a_minion", this, typeof(API).GetMethod("__csharp_is_card_a_minion"));
                lua.RegisterFunction("__csharp_get_card_cost", this, typeof(API).GetMethod("__csharp_get_card_cost"));
                lua.RegisterFunction("__csharp_is_entity_exhausted", this, typeof(API).GetMethod("__csharp_is_entity_exhausted"));
                
                lua.RegisterFunction("__csharp_convert_to_entity", this, typeof(API).GetMethod("__csharp_convert_to_entity"));

                // Query about hero
                lua.RegisterFunction("__csharp_get_health_of_our_hero", this, typeof(API).GetMethod("__csharp_get_health_of_our_hero"));
                lua.RegisterFunction("__csharp_our_hero_crystals", this, typeof(API).GetMethod("__csharp_our_hero_crystals"));
                lua.RegisterFunction("__csharp_enemy_hero_crystals", this, typeof(API).GetMethod("__csharp_enemy_hero_crystals"));
                lua.RegisterFunction("__use_hero_power", this, typeof(API).GetMethod("__use_hero_power"));

                // Attack
                lua.RegisterFunction("__csharp_do_attack", this, typeof(API).GetMethod("__csharp_do_attack"));
                
                // End Turn
                lua.RegisterFunction("__csharp_end_turn", this, typeof(API).GetMethod("__csharp_end_turn"));
                
                Log.log("Loading Main.lua...");
                lua.DoFile(lua_script_path + "Main.lua");
                Log.log("Done");
            }
            catch(LuaException e)
            {
                Log.error("EXCEPTION");
                Log.error(e.ToString());
                Log.error(e.Message);
            }
            catch(Exception e)
            {
                Log.error(e.ToString());
            }
            Log.log("Scripts loaded constructed");
        }

        public LuaTable CreateTable()
        {
            return (LuaTable) lua.DoString("return {}")[0];
        }

        private LuaTable __csharp_card_transfer(List<Card> list)
        {
            LuaTable tab = CreateTable();
            int i = 1;
            foreach(Card current in list)
            {
                tab[i] = current;
                i++;
            }
            return tab;
        }

        private List<Card> __csharp_card_construct(LuaTable tab)
        {
            List<Card> list = new List<Card>();
            
            for(int i = 1; ; i++)
            {
                Card c = tab[i] as Card;
                if(c == null)
                {
                    break;
                }
                list.Add(c);
            }

            return list;
        }

        public LuaTable __csharp_get_our_battlefield_cards()
        {
            List<Card> list = getOurPlayer().GetBattlefieldZone().GetCards().ToList<Card>();
            return __csharp_card_transfer(list);
        }

        public LuaTable __csharp_get_enemy_battlefield_cards()
        {
            List<Card> list = getEnemyPlayer().GetBattlefieldZone().GetCards().ToList<Card>();
            return __csharp_card_transfer(list);
        }

        public LuaTable __csharp_get_hand_cards()
        {
            List<Card> list = getOurPlayer().GetHandZone().GetCards().ToList<Card>();
            return __csharp_card_transfer(list);
        }

        public bool __csharp_drop_card(Card c, bool b)
        {
            return drop_card(c, b);
        }
/*
        public int __csharp_get_attack_of_card(Card c)
        {
            Entity entity = c.GetEntity();
            return __csharp_get_attack_of_entity(entity);
        }
*/
        public int __csharp_get_attack_of_entity(Entity entity)
        {
            return entity.GetATK();
        }
/*
        public int __csharp_get_health_of_card(Card c)
        {
            Entity entity = c.GetEntity();
            return __csharp_get_health_of_entity(entity);
        }
*/
        public int __csharp_get_health_of_entity(Entity entity)
        {
            return entity.GetHealth();
        }

        public int __csharp_get_damage_of_entity(Entity entity)
        {
            return entity.GetDamage();
        }

        public bool __csharp_is_card_a_minion(Card c)
        {
            Entity entity = c.GetEntity();
            return (entity.GetCardType() == TAG_CARDTYPE.MINION);
        }

        public int __csharp_get_card_cost(Card c)
        {
            Entity entity = c.GetEntity();
            return entity.GetCost();
        }

        public int __csharp_get_health_of_our_hero()
        {
            Entity hero = getOurPlayer().GetHero();
            return hero.GetHealth();
        }

        public int __csharp_our_hero_crystals()
        {
            return getOurPlayer().GetNumAvailableResources();
        }

        public int __csharp_enemy_hero_crystals()
        {
            return getEnemyPlayer().GetNumAvailableResources();
        }

        public void __csharp_do_attack(Card c)
        {
            attack(c);
        }

        public Card __csharp_enemy_hero_card()
        {
            Card hero_card = getEnemyPlayer().GetHeroCard();
            return hero_card;
        }

        private bool was_critical_pause_requested()
        {
            bool critical_puase_requested = (bool) lua["__critical_pause"];
            lua["__critical_pause"] = false;
            return critical_puase_requested;
        }

        public bool run()
        {
            try
            {
                LuaFunction f = lua.GetFunction("turn_start");
                if(f == null)
                {
                    Log.log("Lua function not found!");
                    return false;
                }
                object[] args = f.Call();
                string error = (string) args[0];
                if(error != null)
                {
                    Log.log("LUA EXCEPTION");
                    Log.log(error);
                    return false;
                }
                
                return was_critical_pause_requested();
            }
            catch(LuaException e)
            {
                Log.error("EXCEPTION");
                Log.error(e.ToString());
                Log.error(e.Message);
            }
            catch(Exception e)
            {
                Log.error(e.ToString());
            }
            return false;
        }

        public List<Card> mulligan(List<Card> cards)
        {
            try
            {
                LuaFunction f = lua.GetFunction("mulligan");
                if(f == null)
                {
                    Log.log("Lua function not found!");
                    return null;
                }
                
                LuaTable argument = __csharp_card_transfer(cards);

                object[] args = f.Call(argument);
                
                LuaTable replace = args[0] as LuaTable;

                if(replace != null)
                {
                    List<Card> replace_list = __csharp_card_construct(replace);
                    return replace_list;
                }
                Log.log("NO VALID RETURN TYPE");
            }
            catch(LuaException e)
            {
                Log.error("EXCEPTION");
                Log.error(e.ToString());
                Log.error(e.Message);
            }
            catch(Exception e)
            {
                Log.error(e.ToString());
            }
            return null;
        }

        public Entity __csharp_convert_to_entity(Card c)
        {
            return c.GetEntity();
        }

        public bool __csharp_is_entity_exhausted(Entity entity)
        {
            return entity.IsExhausted();
        }

        public bool __csharp_can_entity_attack(Entity entity)
        {
            return entity.CanAttack();
        }

        public bool __csharp_can_entity_be_attack(Entity entity)
        {
            return entity.CanBeAttacked();
        }
        
        public bool __csharp_is_card_tank(Card c)
        {
            Entity entity = c.GetEntity();
            return entity.HasTaunt();
        }

        public void __csharp_end_turn()
        {
            end_turn();
        }

        // TODO: Needs a fix
        public bool __use_hero_power()
        {
            return false;
        }

        public static Player getOurPlayer()
        {
            return GameState.Get().GetLocalPlayer();
        }

        public static Player getEnemyPlayer()
        {
            return GameState.Get().GetFirstOpponentPlayer(getOurPlayer());
        }

		public static void attack(Card c)
		{
			Log.log("Attack: " + c.GetEntity().GetName());

            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("HandleClickOnCardInBattlefield", BindingFlags.NonPublic | BindingFlags.Instance);

            Entity e = c.GetEntity();

            dynMethod.Invoke(input_mgr, new object[] { e });
		}

		public static bool drop_card(Card c, bool pickup)
		{
			Log.log("Dropped card: " + c.GetEntity().GetName());

            InputManager input_mgr = InputManager.Get();
            if (pickup)
            {
                GameObject ob = c.gameObject;

                MethodInfo dynMethod = input_mgr.GetType().GetMethod("GrabCard", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(input_mgr, new object[] { ob });
            }
            else
            {
                MethodInfo dynMethod = input_mgr.GetType().GetMethod("DropHeldCard", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]{}, null);
                return (bool) dynMethod.Invoke(input_mgr, null);
            }
            return false;
		}

		public static void end_turn()
		{
			InputManager.Get().DoEndTurnButton();
		}
    }
}

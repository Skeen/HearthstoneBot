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

        public bool __use_hero_power()
        {
            Card c1 = getOurPlayer().GetHeroPowerCard();
            Log.log("GetHeroPowerCard : " + c1.GetActorName());
            Log.log("GetHeroPowerCard : " + c1.GetEntity().GetName());

            
            Card c2 = getOurPlayer().GetHeroCard();
            Log.log("GetHeroCard : " + c2.GetActorName());
            Log.log("GetHeroCard : " + c2.GetEntity().GetName());

            if(c1 == null)
            {
                return false;
            }
            else
            {
                // TODO: Figure out which of these lines work, one of them does
                attack(c1);

                drop_card(c1, true);

                drop_card(c1, false);
                return true;
            }
        }

        public static Player getOurPlayer()
        {
            return GameState.Get().GetLocalPlayer();
        }

        public static Player getEnemyPlayer()
        {
            return GameState.Get().GetFirstOpponentPlayer(getOurPlayer());
        }

		public void attack(Card c)
		{
			Log.log("Attack: " + c.GetEntity().GetName());

            PrivateHacker.HandleClickOnCardInBattlefield(c);
		}

        public bool drop_held_card(int requested_zone_position = 1) 
        {
            try
            {
                return drop_held_card_worker(requested_zone_position);
            }
            catch(Exception e)
            {
                Log.error("Exception within drop_held_card_worker");
                Log.error(e.ToString());
            }
            return false;
        }

        public bool drop_held_card_worker(int requested_zone_position)
        {
            PegCursor.Get().SetMode(PegCursor.Mode.STOPDRAG);

            InputManager input_man = InputManager.Get();
            if (input_man.heldObject == null)
            {
                Log.log("Nothing held, when trying to drop");
                return false;
            }
            Card component = input_man.heldObject.GetComponent<Card>();

            ZonePlay m_myPlayZone = PrivateHacker.get_m_myPlayZone();
            ZoneHand m_myHandZone = PrivateHacker.get_m_myHandZone();
            
            component.SetDoNotSort(false);
            iTween.Stop(input_man.heldObject);
            Entity entity = component.GetEntity();
            component.NotifyLeftPlayfield();
            GameState.Get().GetGameEntity().NotifyOfCardDropped(entity);
            m_myPlayZone.UnHighlightBattlefield();
            DragCardSoundEffects component2 = component.GetComponent<DragCardSoundEffects>();
            if (component2)
            {
                component2.Disable();
            }
            UnityEngine.Object.Destroy(input_man.heldObject.GetComponent<DragRotator>());
            input_man.heldObject = null;
            ProjectedShadow componentInChildren = component.GetActor().GetComponentInChildren<ProjectedShadow>();
            if (componentInChildren != null)
            {
                componentInChildren.DisableShadow();
            }
            
            // Check that the card is on the hand
            Zone card_zone = component.GetZone();
            if ((card_zone == null) || card_zone.m_ServerTag != TAG_ZONE.HAND)
            {
                return false;
            }
            
            bool does_target = false;

            bool is_minion = entity.IsMinion();
            bool is_weapon = entity.IsWeapon();

            if (is_minion || is_weapon)
            {
                Zone zone = (!is_weapon) ? (Zone) m_myPlayZone : (Zone) PrivateHacker.get_m_myWeaponZone();
                if (zone)
                {
                    GameState gameState = GameState.Get();
                    int card_position = Network.NoPosition;
                    if (is_minion)
                    {
                        card_position = ZoneMgr.Get().PredictZonePosition(zone, requested_zone_position);
                        gameState.SetSelectedOptionPosition(card_position);
                    }
                    if (input_man.DoNetworkResponse(entity))
                    {
                        if (is_weapon)
                        {
                            PrivateHacker.set_m_lastZoneChangeList(ZoneMgr.Get().AddLocalZoneChange(component, zone, zone.GetLastPos()));
                        }
                        else
                        {
                            PrivateHacker.set_m_lastZoneChangeList(ZoneMgr.Get().AddPredictedLocalZoneChange(component, zone, requested_zone_position, card_position));
                        }
                        PrivateHacker.ForceManaUpdate(entity);
                        if (is_minion && gameState.EntityHasTargets(entity))
                        {
                            does_target = true;
                            if (TargetReticleManager.Get())
                            {
                                bool showArrow = true;
                                TargetReticleManager.Get().CreateFriendlyTargetArrow(entity, entity, true, showArrow, null);
                            }
                            PrivateHacker.set_m_battlecrySourceCard(component);
                        }
                    }
                    else
                    {
                        gameState.SetSelectedOptionPosition(Network.NoPosition);
                    }
                }
            }
            /* // Spell support
               else
               {
               if (entity.IsSpell())
               {
               if (GameState.Get().EntityHasTargets(entity))
               {
               input_man.DropCanceledHeldCard(entity.GetCard());
               return true;
               }
               RaycastHit raycastHit2;
               if (UniversalInputManager.Get().GetInputHitInfo(GameLayer.InvisibleHitBox2.LayerBit(), out raycastHit2))
               {
               if (!GameState.Get().HasResponse(entity))
               {
               PlayErrors.DisplayPlayError(PlayErrors.GetPlayEntityError(entity), entity);
               }
               else
               {
               input_man.DoNetworkResponse(entity);
               if (entity.IsSecret())
               {
               input_man.m_lastZoneChangeList = ZoneMgr.Get().AddLocalZoneChange(component, input_man.m_mySecretZone, input_man.m_mySecretZone.GetLastPos());
               }
               else
               {
               input_man.m_lastZoneChangeList = ZoneMgr.Get().AddLocalZoneChange(component, TAG_ZONE.PLAY);
               }
               PrivateHacker.ForceManaUpdate(entity);
               input_man.PlayPowerUpSpell(component);
               input_man.PlayPlaySpell(component);
               }
               }
               }
               }
               */
            m_myHandZone.UpdateLayout(-1, true);
            m_myPlayZone.SortWithSpotForHeldCard(-1);
            if (does_target)
            {
                if (EnemyActionHandler.Get())
                {
                    EnemyActionHandler.Get().NotifyOpponentOfTargetModeBegin(component);
                }
            }
            else
            {
                if (GameState.Get().GetResponseMode() != GameState.ResponseMode.SUB_OPTION)
                {
                    EnemyActionHandler.Get().NotifyOpponentOfCardDropped();
                }
            }
            return true;
        }

		public bool drop_card(Card c, bool pickup)
		{
			Log.log("Dropped card: " + c.GetEntity().GetName());

            if (pickup)
            {
                PrivateHacker.GrabCard(c);
            }
            else
            {
                return drop_held_card();
            }
            return false;
		}

		public static void end_turn()
		{
			InputManager.Get().DoEndTurnButton();
            // TODO: Delay for 10 seconds, to avoid running AI out of sync
		}
    }
}

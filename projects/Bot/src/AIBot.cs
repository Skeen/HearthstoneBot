using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using System.Threading;
using System.Reflection;

namespace HearthstoneBot
{
    public class AIBot
    {
        public enum Mode
        {
            TOURNAMENT_RANKED,
            TOURNAMENT_UNRANKED,
            PRATICE_NORMAL,
            PRATICE_EXPERT
        };

        public void setMode(Mode m)
        {
            Log.log("GameMode changed to : " + m.ToString());
            game_mode = m;
        }

        private volatile Mode game_mode = Mode.TOURNAMENT_RANKED;

        private API api = null;
        private readonly System.Random random = null;
        
        private DateTime delay_start = DateTime.Now;
        private long delay_length = 0;

        // Queued actions
        private List<API.Action> queuedActions = new List<API.Action>();

        // Keep track of the last mode
        private SceneMgr.Mode last_scene_mode = SceneMgr.Mode.STARTUP;

        public AIBot()
        {
            random = new System.Random();
            api = new API();
        }

        public void ReloadScripts()
        {
            api = new API();
        }

        // Delay the next Mainloop entry, by msec
        private void Delay(long msec)
        {
            delay_start = DateTime.Now;
            delay_length = msec;
        }

        // Do a single AI tick
        public void tick()
        {
            // Handle any required delays
            // Get current time
            DateTime current_time = DateTime.Now;
            // Figure the time passed, since delay was requested
            TimeSpan time_since_delay = current_time - delay_start;
            // If the time passed, is less than the required delay
            if (time_since_delay.TotalMilliseconds < delay_length)
            {
                // Simply return, for more time to pass
                return;
            }
            // Delay has been waited out, when we get here
            //Delay(500);

            // Try to run the main loop
			try
			{
                // Only do this, if the bot is running
				if (Plugin.isRunning())
				{
                    update();
				}
			}
			catch(Exception e)
			{
                Log.error("Exception, in AI.update()");
                Log.error(e.ToString());
			}
        }

        // Get a random pratice AI mission
        private MissionID getRandomAIMissionID(bool expert)
        {
            // List of normal AI mission IDs
            ReadOnlyCollection<MissionID> AI_Normal =
                new ReadOnlyCollection<MissionID>(new []
            {
                MissionID.AI_NORMAL_MAGE,   MissionID.AI_NORMAL_WARLOCK,
                MissionID.AI_NORMAL_HUNTER, MissionID.AI_NORMAL_ROGUE,
                MissionID.AI_NORMAL_PRIEST, MissionID.AI_NORMAL_WARRIOR,
                MissionID.AI_NORMAL_DRUID,  MissionID.AI_NORMAL_PALADIN,
                MissionID.AI_NORMAL_SHAMAN
            });

            // List of expert AI mission IDs
            ReadOnlyCollection<MissionID> AI_Expert =
                new ReadOnlyCollection<MissionID>(new []
            {
                MissionID.AI_EXPERT_MAGE,   MissionID.AI_EXPERT_WARLOCK,
                MissionID.AI_EXPERT_HUNTER, MissionID.AI_EXPERT_ROGUE,
                MissionID.AI_EXPERT_PRIEST, MissionID.AI_EXPERT_WARRIOR,
                MissionID.AI_EXPERT_DRUID,  MissionID.AI_EXPERT_PALADIN,
                MissionID.AI_EXPERT_SHAMAN
            });

            // Select the requested AI type
            ReadOnlyCollection<MissionID> AI_Selected =
                (expert) ? AI_Expert : AI_Normal;

            // Pick a random index
            int index = random.Next(AI_Selected.Count);
            // Return the corresponding ID
            return AI_Selected[index];
        }
        
        // Return whether Mulligan was done
		private bool mulligan()
		{
            // Check that the button exists and is enabled
			if (GameState.Get().IsMulliganManagerActive() == false ||
                MulliganManager.Get() == null /*||
                MulliganManager.Get().GetMulliganButton() == null ||
                MulliganManager.Get().GetMulliganButton().IsEnabled() == false*/)
			{
				return false;
			}
            
            // Get hand cards
            List<Card> cards = API.getOurPlayer().GetHandZone().GetCards().ToList<Card>();
            // Ask the AI scripting system, to figure which cards to replace
            List<Card> replace = api.mulligan(cards);

            // Toggle them as replaced
            foreach(Card current in replace)
            {
                MulliganManager.Get().ToggleHoldState(current);
            }

            // End mulligan
            MulliganManager.Get().EndMulligan();
			end_turn();

            // Report progress
			Log.say("Mulligan Ended : " + replace.Count + " cards changed");
            // Delay 5 seconds
            Delay(5000);
			return true;
		}

        // Welcome / login screen
        private void login_mode()
        {
            // If there are any welcome quests on the screen
            if (WelcomeQuests.Get() != null)
            {
                Log.say("Entering to main menu");
                // Emulate a next click
                WelcomeQuests.Get().m_clickCatcher.TriggerRelease();
            }
        }

        bool just_joined = false;

        // Found at: DeckPickerTrayDisplay search for RankedMatch
        private void tournament_mode(bool ranked)
        {
            if (just_joined)
                return;

            // Don't do this, if we're currently in a game, or matching a game
            // TODO: Change to an assertion
            if (SceneMgr.Get().IsInGame() || Network.IsMatching())
            {
                return;
            }
            // Delay 5 seconds for loading and such
            // TODO: Smarter delaying
            Delay(5000);

            // If we're not set to the right mode, now is the time to do so
            // Note; This does not update the GUI, only the internal state
            bool is_ranked = Options.Get().GetBool(Option.IN_RANKED_PLAY_MODE);
            if(is_ranked != ranked)
            {
                Options.Get().SetBool(Option.IN_RANKED_PLAY_MODE, ranked);
                return;
            }

            Log.log("Joining game in tournament mode, ranked = " + ranked);

            // Get the ID of the current Deck
            long selectedDeckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
            // We want to play vs other players
            MissionID missionID = MissionID.MULTIPLAYER_1v1;
            // Ranked or unranked?
            GameMode mode = ranked ? GameMode.RANKED_PLAY : GameMode.UNRANKED_PLAY;
            // Setup up the game
            GameMgr.Get().SetNextGame(mode, missionID);
            // Do network join
            if(ranked)
            {
                Network.TrackClient(Network.TrackLevel.LEVEL_INFO,
                        Network.TrackWhat.TRACK_PLAY_TOURNAMENT_WITH_CUSTOM_DECK);
                Network.RankedMatch(selectedDeckID);
            }
            else
            {
                Network.TrackClient(Network.TrackLevel.LEVEL_INFO,
                        Network.TrackWhat.TRACK_PLAY_CASUAL_WITH_CUSTOM_DECK);
                Network.UnrankedMatch(selectedDeckID);
            }
            // Set status
            FriendChallengeMgr.Get().OnEnteredMatchmakerQueue();
            GameMgr.Get().UpdatePresence();

            just_joined = true;
        }

        // Play against AI
        // Found at: PracticePickerTrayDisplay search for StartGame
        private void pratice_mode(bool expert)
        {
            if (just_joined)
                return;

            // Don't do this, if we're currently in a game
            // TODO: Change to an assertion
            if (SceneMgr.Get().IsInGame())
            {
                return;
            }
            // Delay 5 seconds for loading and such
            // TODO: Smarter delaying
            Delay(5000);

            Log.log("Joining game in practice mode, expert = " + expert);

            // Get the ID of the current Deck
            long selectedDeckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
            // Get a random mission, of selected difficulty
            MissionID missionID = getRandomAIMissionID(expert);
            // Start up the game
            GameMgr.Get().StartGame(GameMode.PRACTICE, missionID, selectedDeckID);
            // Set status
            GameMgr.Get().UpdatePresence();
            
            just_joined = true;
        }

        // Called when a game is in mulligan state
        private void do_mulligan()
        {
            // Delay 10 seconds
            Delay(5000);

            try
            {
                mulligan();
            }
            catch(Exception e)
            {
                Log.error("Exception: In mulligan function");
                Log.error(e.ToString());
            }
        }

        // Called when a game is ended
        private void game_over()
        {
            // Delay 10 seconds
            Delay(10000);
            // Try to move on
            try
            {
                // Write why the game ended
                if (API.getEnemyPlayer().GetHero().GetRemainingHP() <= 0)
                {
                    Log.say("Victory!");
                }
                else if (API.getOurPlayer().GetHero().GetRemainingHP() <= 0)
                {
                    Log.say("Defeat...");
                }
                else
                {
                    Log.say("Draw..?");
                }

                // Click through end screen info (rewards, and such)
                if (EndGameScreen.Get() != null)
                {
                    EndGameScreen.Get().m_hitbox.TriggerRelease();

                    //EndGameScreen.Get().ContinueEvents();
                }
            }
            catch(Exception e)
            {
                Log.error("Exception: In endgame function");
                Log.error(e.ToString());
            }
        }

        private void end_turn()
		{
			InputManager.Get().DoEndTurnButton();
            Delay(10000);
		}

        // Called to invoke AI
        private void run_ai()
        {
            try
            {
                Log.log("There are " + queuedActions.Count + " queued actions.");

                // Perform queued actions first
                if(queuedActions.Count > 0)
                {
                    // Dequeue first execution and perform it
                    API.Action action = queuedActions[0];
                    queuedActions.RemoveAt(0);
                    api.performAction(action);

                    // Delay between each action
                    Delay(2000);
                    return;
                }

                // Get hand cards
                var cards = API.getOurPlayer().GetHandZone().GetCards().ToList<Card>();

                Log.log("There are " + cards.Count + " cards in our hand.");

                // Get initial actions to perform
                Log.log("Calling turn_action lua function...");
                var actions = api.turn_action(cards);
                Log.log("The turn_action function returned " + actions.Count + " actions.");

                // Queue up these actions
                queuedActions.AddRange(actions);

                if (queuedActions.Count == 0)
                {
                    // Done with turn actions
                    end_turn();
                }
            }
            catch (Exception e)
            {
                Log.error("Exception in run_ai: " + e.Message);
                Log.error(e.ToString());
            }
        }

        private void gameplay_mode()
        {
            GameState gs = GameState.Get();
            // If we're in mulligan
            if (gs.IsMulliganPhase())
            {
                do_mulligan();
            }
            // If the game is over
            else if (gs.IsGameOver())
            {
                game_over();
            }
            // If it's not our turn
            else if (gs.IsLocalPlayerTurn() == true)
            {
                run_ai();
            }
        }

        // Run a single AI tick
        private void update()
        {
            // Get current scene mode
            SceneMgr.Mode scene_mode = SceneMgr.Get().GetMode();

            // If scene changes let's wait a few seconds
            if (scene_mode != last_scene_mode)
            {
                Delay(5000);
                last_scene_mode = scene_mode;
                return;
            }

            // Switch upon the mode
            switch (scene_mode)
            {
                // Unsupported modes
                case SceneMgr.Mode.STARTUP:
                case SceneMgr.Mode.COLLECTIONMANAGER:
                case SceneMgr.Mode.PACKOPENING:
                case SceneMgr.Mode.FRIENDLY:
                case SceneMgr.Mode.DRAFT:
                case SceneMgr.Mode.CREDITS:
                    // Enter MainMenu
                    SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                    // Delay 5 seconds for loading and such
                    // TODO: Smarter delaying
                    Delay(5000);
                    return;

                // Errors, nothing to do
                case SceneMgr.Mode.INVALID:
                case SceneMgr.Mode.FATAL_ERROR:
                case SceneMgr.Mode.RESET:
                    Log.say("Fatal Error, in AI.tick()", true);
                    Log.say("Force closing game!", true);
                    Plugin.destroy();
                    // Kill it the bad way
                    Environment.FailFast(null);
                    //Plugin.setRunning(false);
                    break;

                // Login screen
                case SceneMgr.Mode.LOGIN:
                    Delay(500);
                    // Click through quests
                    login_mode();
                    break;

                // Main Menu
                case SceneMgr.Mode.HUB:
                    switch(game_mode)
                    {
                        case Mode.PRATICE_NORMAL:
                        case Mode.PRATICE_EXPERT:
                            // Enter Pratice Mode
                            SceneMgr.Get().SetNextMode(SceneMgr.Mode.PRACTICE);
                            break;

                        case Mode.TOURNAMENT_RANKED:
                        case Mode.TOURNAMENT_UNRANKED:
                            // Enter Turnament Mode
                            SceneMgr.Get().SetNextMode(SceneMgr.Mode.TOURNAMENT);
                            Tournament.Get().NotifyOfBoxTransitionStart();
                            break;

                        default:
                            Log.say("Unknown Game Mode!", true);
                            return;
                    }
                    // Delay 5 seconds for loading and such
                    // TODO: Smarter delaying
                    Delay(5000);
                    break;

                // In game
                case SceneMgr.Mode.GAMEPLAY:
                    // Handle Gamplay
                    gameplay_mode();
                    just_joined = false;
                    break; 

                // In Pratice Sub Menu
                case SceneMgr.Mode.PRACTICE:
                    bool expert = false;
                    switch(game_mode)
                    {
                        case Mode.PRATICE_NORMAL:
                            expert = false;
                            break;

                        case Mode.PRATICE_EXPERT:
                            expert = true;
                            break;

                        case Mode.TOURNAMENT_RANKED:
                        case Mode.TOURNAMENT_UNRANKED:
                            // Leave to the Hub
                            Log.say("Inside wrong sub-menu!");
                            SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                            return;

                        default:
                            Log.say("Unknown Game Mode!", true);
                            return;
                    }

                    // Play against AI
                    pratice_mode(expert);
                    break;

                // In Play Sub Menu
                case SceneMgr.Mode.TOURNAMENT:
                    bool ranked = false;
                    switch(game_mode)
                    {
                        case Mode.PRATICE_NORMAL:
                        case Mode.PRATICE_EXPERT:
                            // Leave to the Hub
                            Log.say("Inside wrong sub-menu!");
                            SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
                            return;

                        case Mode.TOURNAMENT_RANKED:
                            ranked = true;
                            break;

                        case Mode.TOURNAMENT_UNRANKED:
                            ranked = false;
                            break;

                        default:
                            Log.say("Unknown Game Mode!", true);
                            return;
                    }

                    // Play against humans (or bots)
                    tournament_mode(ranked);
                    break;

                default:
                    Log.say("Unknown SceneMgr State!", true);
                    return;
            }
        }
    }
}

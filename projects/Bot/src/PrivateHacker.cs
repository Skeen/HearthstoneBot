using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;

namespace HearthstoneBot
{
    public class PrivateHacker
    {
        public static void ForceManaUpdate(Entity entity)
        {
            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("ForceManaUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(input_mgr, new object[] { entity });
        }

        public static void HandleClickOnCardInBattlefield(Card c)
        {
            Entity e = c.GetEntity();

            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("HandleClickOnCardInBattlefield", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(input_mgr, new object[] { e });
        }

        public static void GrabCard(Card c)
        {
            GameObject ob = c.gameObject;
            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("GrabCard", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(input_mgr, new object[] { ob });
        }

        public static void HandleMouseOverCard(Card c)
        {
            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("HandleMouseOverCard", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(input_mgr, new object[] { c });
        }

        public static void HandleMouseOffCard()
        {
            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("HandleMouseOffCard", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(input_mgr, new object[] { });
        }

        public static bool PlayPowerUpSpell(Card c)
        {
            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("PlayPowerUpSpell", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool) dynMethod.Invoke(input_mgr, new object[] { c });
        }

        public static bool PlayPlaySpell(Card c)
        {
            InputManager input_mgr = InputManager.Get();
            MethodInfo dynMethod = input_mgr.GetType().GetMethod("PlayPlaySpell", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool) dynMethod.Invoke(input_mgr, new object[] { c });
        }

        public static ZonePlay get_m_myPlayZone()
        {
            InputManager input_mgr = InputManager.Get();
            FieldInfo myFieldInfo = input_mgr.GetType().GetField("m_myPlayZone", BindingFlags.NonPublic | BindingFlags.Instance); 
            return (ZonePlay) myFieldInfo.GetValue(input_mgr); 
        }

        public static ZoneHand get_m_myHandZone()
        {
            InputManager input_mgr = InputManager.Get();
            FieldInfo myFieldInfo = input_mgr.GetType().GetField("m_myHandZone", BindingFlags.NonPublic | BindingFlags.Instance); 
            return (ZoneHand) myFieldInfo.GetValue(input_mgr); 
        }

        public static ZoneWeapon get_m_myWeaponZone()
        {
            InputManager input_mgr = InputManager.Get();
            FieldInfo myFieldInfo = input_mgr.GetType().GetField("m_myWeaponZone", BindingFlags.NonPublic | BindingFlags.Instance); 
            return (ZoneWeapon) myFieldInfo.GetValue(input_mgr); 
        }

        public static ZoneSecret get_m_mySecretZone()
        {
            InputManager input_mgr = InputManager.Get();
            FieldInfo myFieldInfo = input_mgr.GetType().GetField("m_mySecretZone", BindingFlags.NonPublic | BindingFlags.Instance); 
            return (ZoneSecret) myFieldInfo.GetValue(input_mgr);
        }

        public static void set_m_battlecrySourceCard(Card val)
        {
            InputManager input_mgr = InputManager.Get();
            FieldInfo myFieldInfo = input_mgr.GetType().GetField("m_battlecrySourceCard", BindingFlags.NonPublic | BindingFlags.Instance); 
            myFieldInfo.SetValue(input_mgr, val); 
        }

        public static void set_m_lastZoneChangeList(ZoneChangeList val)
        {
            InputManager input_mgr = InputManager.Get();
            FieldInfo myFieldInfo = input_mgr.GetType().GetField("m_lastZoneChangeList", BindingFlags.NonPublic | BindingFlags.Instance); 
            myFieldInfo.SetValue(input_mgr, val); 
        }
    }
}

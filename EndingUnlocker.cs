using MelonLoader;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace EndingUnlockerNotACheatTho
{
    public class EndingUnlocker : MelonMod
    {
        public override void OnApplicationLateStart()
        {
            GameDataManager.powerPrefs.dontUploadToLeaderboard = true;
            PatchGame();

            Game game = Singleton<Game>.Instance;

            if (game == null)
                return;

            game.OnLevelLoadComplete += OnLevelLoadComplete;

            if (RM.drifter)
                OnLevelLoadComplete();
        }

        private void OnLevelLoadComplete()
        {
            if (SceneManager.GetActiveScene().name.Equals("Heaven_Environment"))
                return;
        }

        private void PatchGame()
        {
            HarmonyLib.Harmony harmony = new("de.MOPSKATER.BoofOfMemes");

            MethodInfo target = typeof(LevelStats).GetMethod("UpdateTimeMicroseconds");
            HarmonyMethod patch = new(typeof(EndingUnlocker).GetMethod("PreventNewScore"));
            harmony.Patch(target, patch);

            target = typeof(Game).GetMethod("OnLevelWin");
            patch = new(typeof(EndingUnlocker).GetMethod("PreventNewGhost"));
            harmony.Patch(target, patch);

            target = typeof(LevelRush).GetMethod("IsCurrentLevelRushScoreBetter", BindingFlags.NonPublic | BindingFlags.Static);
            patch = new(typeof(EndingUnlocker).GetMethod("PreventNewBestLevelRush"));
            harmony.Patch(target, patch);

            target = typeof(LevelGate).GetMethod("SetUnlocked");
            patch = new(typeof(EndingUnlocker).GetMethod("UnlockGate"));
            harmony.Patch(target, patch);

        }

        public static bool PreventNewScore(LevelStats __instance, ref long newTime)
        {
            if (newTime < __instance._timeBestMicroseconds)
            {
                if (__instance._timeBestMicroseconds == 999999999999L)
                    __instance._timeBestMicroseconds = 600000000;
                __instance._newBest = true;
            }
            else
                __instance._newBest = false;
            __instance._timeLastMicroseconds = newTime;
            return false;
        }

        public static bool PreventNewGhost(Game __instance)
        {
            __instance.winAction = null;
            return true;
        }

        public static bool PreventNewBestLevelRush(ref bool __result)
        {
            __result = false;
            return false;
        }

        public static bool UnlockGate(LevelGate __instance, ref bool u)
        {
            if (__instance.Unlocked)
                return false;
            u = true;
            return true;
        }
    }
}
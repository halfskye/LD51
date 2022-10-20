﻿using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OldManAndTheSea.Utilities
{
    public static class DebugLogUtilities
    {
        private const string DefaultDebugDefine = "DEBUG";

        /// <summary>
        /// Add new log types here, and then enable/disable them in DebugLogTypeEnabledMap below.
        /// </summary>
        public enum DebugLogType
        {
            CORE = -1,
            UTILITIES = 0,
            SEA_MANAGER = 1,
            WORLD_MANAGER = 2,
            SHIP = 3,
            PLAYER_INPUT = 4,
            WEAPONS = 5,
            LOOT = 6,
            SPAWN = 7,
        }

        private delegate void DebugLogFunction(string message, Object context);

        /// <summary>
        /// Specifies whether a given DebugLogType is enabled/disabled.
        /// </summary>
        private static readonly Dictionary<DebugLogType, bool> DebugLogTypeEnabledMap = new Dictionary<DebugLogType, bool>()
        {
            { DebugLogType.CORE, true }
            , { DebugLogType.UTILITIES, false }
            , { DebugLogType.SEA_MANAGER, false }
            , { DebugLogType.WORLD_MANAGER, false }
            , { DebugLogType.SHIP, false }
            , { DebugLogType.PLAYER_INPUT, false }
            , { DebugLogType.WEAPONS, false }
            , { DebugLogType.LOOT, false }
            , { DebugLogType.SPAWN, false }
        };

        [Conditional(DefaultDebugDefine)]
        public static void Log(DebugLogType debugLogType, string message, Object context=null)
        {
            Log_Internal(Debug.Log, debugLogType, message, context);
        }

        [Conditional(DefaultDebugDefine)]
        public static void LogWarning(DebugLogType debugLogType, string message, Object context=null)
        {
            Log_Internal(Debug.LogWarning, debugLogType, message, context);
        }

        [Conditional(DefaultDebugDefine)]
        public static void LogError(DebugLogType debugLogType, string message, Object context=null)
        {
            Log_Internal(Debug.LogError, debugLogType, message, context);
        }

        [Conditional(DefaultDebugDefine)]
        private static void Log_Internal(DebugLogFunction debugLogFunction, DebugLogType debugLogType, string message, Object context)
        {
            if (DebugLogTypeEnabledMap.TryGetValue(debugLogType, out var isEnabled))
            {
                if (isEnabled)
                {
                    debugLogFunction(message, context);
                }
            }
        }
    }
}
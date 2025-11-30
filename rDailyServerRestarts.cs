using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using ConVar;

using Newtonsoft.Json;

using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

using UnityEngine;

namespace Oxide.Plugins
{
    [Info("rDailyServerRestarts", "Ftuoil Xelrash", "0.0.1")]
    [Description("Daily scheduled server restarts with countdown announcements")]
    public class rDailyServerRestarts : RustPlugin
    {
        #region Fields

        private Configuration _config;
        private DailyRestartComponent _restartComponent;
        private static rDailyServerRestarts Instance;

        #endregion

        #region Permission

        private static class Permission
        {
            public const string Admin = "rdailyserverrestarts.admin";
        }

        #endregion

        #region Configuration

        private sealed class Configuration
        {
            [JsonProperty(PropertyName = "Enable daily restarts")]
            public bool EnableDailyRestart { get; set; }

            [JsonProperty(PropertyName = "Daily restart time (HH:mm:ss format, 24-hour UTC)")]
            public string DailyRestartTime { get; set; }

            [JsonProperty(PropertyName = "Enable server save before restart")]
            public bool EnableServerSave { get; set; }

            [JsonProperty(PropertyName = "Enable server backup before restart")]
            public bool EnableServerBackup { get; set; }

            [JsonProperty(PropertyName = "Countdown duration in minutes")]
            public int CountdownMinutes { get; set; }
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                EnableDailyRestart = true,
                DailyRestartTime = "04:00:00",
                EnableServerSave = true,
                EnableServerBackup = true,
                CountdownMinutes = 15
            };
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Instance = this;
            try
            {
                _config = Config.ReadObject<Configuration>();
            }
            catch (Exception exception)
            {
                PrintError(exception.ToString());
                Puts("Configuration has been reset to default!");
                _config = GetDefaultConfig();
            }
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        #endregion

        #region Oxide Hooks

        void Init()
        {
            permission.RegisterPermission(Permission.Admin, this);
        }

        void OnServerInitialized()
        {
            _restartComponent = ServerMgr.Instance.gameObject.AddComponent<DailyRestartComponent>();
            cmd.AddConsoleCommand("rdsr.status", this, nameof(StatusCommand));
            cmd.AddConsoleCommand("rdsr.cancel", this, nameof(CancelCommand));
            cmd.AddConsoleCommand("rdsr.now", this, nameof(NowCommand));
            cmd.AddConsoleCommand("rdsr.schedule", this, nameof(ScheduleCommand));
            cmd.AddConsoleCommand("rdsr.test", this, nameof(TestCommand));
        }

        void Unload()
        {
            if (_restartComponent != null)
            {
                UnityEngine.Object.Destroy(_restartComponent);
            }
            Instance = null;
        }

        #endregion

        #region Commands

        private void StatusCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
            {
                arg.ReplyWith("This command can only be run from server console");
                return;
            }

            if (!_config.EnableDailyRestart)
            {
                Puts("Daily restarts are disabled");
                return;
            }

            if (_restartComponent.IsRestarting)
            {
                var secondsLeft = (int)(_restartComponent.RestartTime - DateTime.Now).TotalSeconds;
                Puts($"Restart countdown active: {FormatTime(secondsLeft)} remaining");
            }
            else if (_restartComponent.ScheduledRestartTime < DateTime.MaxValue)
            {
                var secondsLeft = (int)(_restartComponent.ScheduledRestartTime - DateTime.Now).TotalSeconds;
                if (secondsLeft > 0)
                    Puts($"Daily restart scheduled for {_restartComponent.ScheduledRestartTime:HH:mm:ss} UTC ({FormatTime(secondsLeft)} from now)");
                else
                    Puts("Scheduled restart time has passed");
            }
            else
            {
                Puts("No restart scheduled");
            }
        }

        private void CancelCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
            {
                arg.ReplyWith("This command can only be run from server console");
                return;
            }

            if (!_restartComponent.IsRestarting)
            {
                Puts("There is no restart scheduled to cancel");
                return;
            }

            _restartComponent.CancelRestart();
            BroadcastMessage("Server restart has been cancelled");
            Puts("Restart cancelled successfully");
        }

        private void NowCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
            {
                arg.ReplyWith("This command can only be run from server console");
                return;
            }

            _restartComponent.DoRestart(DateTime.Now.AddSeconds(10));
            BroadcastMessage("Server restarting in 10 seconds");
            Puts("Restart scheduled for 10 seconds from now");
        }

        private void ScheduleCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
            {
                arg.ReplyWith("This command can only be run from server console");
                return;
            }

            // If no arguments, schedule for next day at normal restart time
            if (arg.Args == null || arg.Args.Length == 0)
            {
                var parsedTime = ParseTime(_config.DailyRestartTime);
                if (parsedTime == null)
                {
                    Puts("Error: Invalid configured restart time");
                    return;
                }

                // Schedule for tomorrow at the configured time
                var nextRestart = parsedTime.Value.AddDays(1);
                _restartComponent.ScheduleManualRestart(nextRestart);

                var secondsUntil = (int)(nextRestart - DateTime.Now).TotalSeconds;
                Puts($"Restart scheduled for tomorrow at {nextRestart:HH:mm:ss} UTC ({FormatTime(secondsUntil)} from now)");
                return;
            }

            if (!int.TryParse(arg.Args[0], out int seconds))
            {
                Puts("Invalid number. Usage: rdsr.schedule <seconds> or rdsr.schedule (for next day)");
                return;
            }

            if (seconds < 900)
            {
                Puts("Minimum restart time is 900 seconds (15 minutes)");
                return;
            }

            DateTime restartTime = DateTime.Now.AddSeconds(seconds);
            _restartComponent.ScheduleManualRestart(restartTime);
            Puts($"Restart scheduled for {restartTime:HH:mm:ss} UTC ({FormatTime(seconds)} from now)");
        }

        private void TestCommand(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null)
            {
                arg.ReplyWith("This command can only be run from server console");
                return;
            }

            Puts("[DEV TEST] Initiating restart with 60-second countdown...");
            Puts("[DEV TEST] Watch server messages and player chat");
            _restartComponent.DoRestart(DateTime.Now.AddSeconds(60));
        }

        #endregion

        #region Helpers

        private void BroadcastMessage(string message)
        {
            Puts(message);
            Server.Broadcast(message);
        }

        private string GetCustomMessage(string key, string playerId, params object[] args)
        {
            try
            {
                var template = lang.GetMessage(key, this, playerId);
                return string.Format(template, args);
            }
            catch (Exception ex)
            {
                PrintError($"Message formatting error for key '{key}': {ex.Message}");
                return key;
            }
        }

        private string GetMessage(string key, string playerId = null)
        {
            return lang.GetMessage(key, this, playerId);
        }

        private static string FormatTime(int? seconds)
        {
            if (!seconds.HasValue || seconds < 0) return "N/A";

            var hours = seconds / 3600;
            var minutes = (seconds % 3600) / 60;
            var secs = seconds % 60;

            var result = new StringBuilder();

            if (hours > 0)
            {
                result.Append(hours).Append("h");
                if (minutes > 0 || secs > 0) result.Append(" ");
            }

            if (minutes > 0)
            {
                result.Append(minutes).Append("m");
                if (secs > 0) result.Append(" ");
            }

            if (secs > 0 || (hours == 0 && minutes == 0))
                result.Append(secs).Append("s");

            return result.ToString();
        }

        private static DateTime? ParseTime(string timeStr)
        {
            if (!TimeSpan.TryParseExact(timeStr, @"hh\:mm\:ss", CultureInfo.InvariantCulture, TimeSpanStyles.None, out var time))
                return null;

            var today = DateTime.Now.TimeOfDay;
            return time < today ? DateTime.Now.Date.AddDays(1).Add(time) : DateTime.Now.Date.Add(time);
        }

        #endregion

        #region Restart Component

        private sealed class DailyRestartComponent : MonoBehaviour
        {
            private Coroutine _restartCoroutine = null;
            private DateTime _scheduledRestartTime = DateTime.MaxValue;
            private bool _shouldCancel = false;
            private DateTime _lastRestartAttempt = DateTime.MinValue;

            public DateTime RestartTime { get; private set; }
            public bool IsRestarting { get; private set; }
            public DateTime ScheduledRestartTime => _scheduledRestartTime;

            void Start()
            {
                if (!Instance._config.EnableDailyRestart) return;

                var restartTime = ParseTime(Instance._config.DailyRestartTime);
                if (restartTime == null)
                {
                    Instance.PrintError($"Invalid restart time format: {Instance._config.DailyRestartTime}");
                    return;
                }

                // Safety check: if restart time is in the past, schedule for tomorrow instead
                if (DateTime.Now > restartTime.Value)
                {
                    Instance.Puts($"Restart time {Instance._config.DailyRestartTime} has already passed today, scheduling for tomorrow");
                    restartTime = restartTime.Value.AddDays(1);
                }

                _scheduledRestartTime = restartTime.Value;
                ScheduleRestartCheck(restartTime.Value);

                // Check once per second instead of every frame (60x per second)
                InvokeRepeating(nameof(CheckRestartTime), 1f, 1f);
            }

            private void CheckRestartTime()
            {
                if (!Instance._config.EnableDailyRestart || IsRestarting) return;

                var secondsUntil = (int)(_scheduledRestartTime - DateTime.Now).TotalSeconds;

                // Don't restart again if we just cancelled one (wait 2+ minutes before next attempt)
                var secsSinceLastAttempt = (int)(DateTime.Now - _lastRestartAttempt).TotalSeconds;
                if (secsSinceLastAttempt < 120) return;

                // Announce 15, 10, and 5 minutes before restart time
                if (secondsUntil == 900)
                {
                    Instance.Puts("Scheduled Daily Restart in 15 minutes");
                }
                else if (secondsUntil == 600)
                {
                    Instance.Puts("Scheduled Daily Restart in 10 minutes");
                }
                else if (secondsUntil == 300)
                {
                    Instance.Puts("Scheduled Daily Restart in 5 minutes");
                }

                // Start countdown when within 90 seconds (1.5 minutes) of restart time
                if (secondsUntil <= 90 && secondsUntil > 0)
                {
                    _lastRestartAttempt = DateTime.Now;
                    DoRestart(_scheduledRestartTime);
                }
                else if (secondsUntil <= 0)
                {
                    // Safety check: if time has passed, trigger immediately
                    _lastRestartAttempt = DateTime.Now;
                    DoRestart(DateTime.Now.AddSeconds(1));
                }
            }

            public void ScheduleManualRestart(DateTime restartTime)
            {
                _scheduledRestartTime = restartTime;
                _lastRestartAttempt = DateTime.MinValue;
            }

            public void DoRestart(DateTime restartTime)
            {
                if (IsRestarting) return;

                RestartTime = restartTime;
                IsRestarting = true;
                _shouldCancel = false;

                var secondsLeft = (int)(restartTime - DateTime.Now).TotalSeconds;
                if (secondsLeft < 1) secondsLeft = 1;

                // Broadcast initial message with time remaining
                Instance.Server.Broadcast($"Scheduled Daily Restart in {FormatTime(secondsLeft)}");

                _restartCoroutine = StartCoroutine(RestartRoutine(secondsLeft));
            }

            public void CancelRestart()
            {
                // Allow cancellation of both active countdown and scheduled restart
                if (!IsRestarting && _scheduledRestartTime >= DateTime.Now)
                {
                    // Scheduled but not yet counting down - just clear the scheduled time
                    _scheduledRestartTime = DateTime.MaxValue;
                    _lastRestartAttempt = DateTime.Now;
                    return;
                }

                if (!IsRestarting) return;

                _shouldCancel = true;
                if (_restartCoroutine != null)
                {
                    StopCoroutine(_restartCoroutine);
                }
                Cleanup();
            }

            private IEnumerator RestartRoutine(int totalSecondsLeft)
            {
                // All countdown announcements: 15m, 10m, 5m, 1m, 30s, 10s, 5s, Now!
                int[] allCountdowns = { 900, 600, 300, 60, 30, 10, 5, 0 };

                foreach (var countdown in allCountdowns)
                {
                    if (_shouldCancel) { Cleanup(); yield break; }

                    // Recalculate time remaining based on actual restart time
                    int secondsRemaining = (int)(RestartTime - DateTime.Now).TotalSeconds;
                    if (secondsRemaining < 0) secondsRemaining = 0;

                    // Skip if we've already passed this countdown point
                    if (secondsRemaining < countdown) continue;

                    // Wait until we reach this countdown point
                    if (secondsRemaining > countdown)
                    {
                        yield return new WaitForSecondsRealtime(secondsRemaining - countdown);
                    }

                    if (_shouldCancel) { Cleanup(); yield break; }

                    // Broadcast message
                    string message = countdown == 900 ? "Scheduled Daily Restart in 15 minutes" :
                                   countdown == 600 ? "Scheduled Daily Restart in 10 minutes" :
                                   countdown == 300 ? "Scheduled Daily Restart in 5 minutes" :
                                   countdown == 60 ? "Scheduled Daily Restart in 1 minute" :
                                   countdown == 30 ? "Scheduled Daily Restart in 30 seconds" :
                                   countdown == 10 ? "Scheduled Daily Restart in 10 seconds" :
                                   countdown == 5 ? "Scheduled Daily Restart in 5 seconds" :
                                   "Scheduled Daily Restart NOW!";

                    Instance.Server.Broadcast(message);
                }

                if (_shouldCancel) { Cleanup(); yield break; }

                // Execute restart sequence
                yield return new WaitForSecondsRealtime(1);

                if (_shouldCancel) { Cleanup(); yield break; }

                // Save
                if (Instance._config.EnableServerSave)
                {
                    Instance.Puts("Saving server...");
                    yield return new WaitForSecondsRealtime(1);
                    ConsoleSystem.Run(ConsoleSystem.Option.Server, "save");
                    yield return new WaitForSecondsRealtime(10);
                }

                if (_shouldCancel) { Cleanup(); yield break; }

                // Backup
                if (Instance._config.EnableServerBackup)
                {
                    Instance.Puts("Backing up server...");
                    yield return new WaitForSecondsRealtime(1);
                    ConsoleSystem.Run(ConsoleSystem.Option.Server, "backup");
                    yield return new WaitForSecondsRealtime(10);
                }

                if (_shouldCancel) { Cleanup(); yield break; }

                // Kick players
                foreach (var player in BasePlayer.allPlayerList.ToList())
                {
                    player.Kick(Instance.GetMessage("KickReason", player.UserIDString));
                }

                Cleanup();
            }

            private void ScheduleRestartCheck(DateTime restartTime)
            {
                var secondsUntilRestart = (int)(restartTime - DateTime.Now).TotalSeconds;
                if (secondsUntilRestart > 0)
                {
                    Instance.Puts($"Daily restart scheduled for {restartTime:HH:mm:ss} UTC ({FormatTime(secondsUntilRestart)} from now)");
                }
            }

            private void Cleanup()
            {
                _restartCoroutine = null;
                IsRestarting = false;
            }

            void OnDestroy()
            {
                CancelInvoke(nameof(CheckRestartTime));
                if (IsRestarting)
                {
                    CancelRestart();
                }
            }
        }

        #endregion

        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["KickReason"] = "Server is restarting",
            }, this);
        }

        #endregion
    }
}

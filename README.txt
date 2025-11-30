================================================================================
                    rDailyServerRestarts
================================================================================

Version:        0.0.42
Author:         Ftuoil Xelrash
License:        MIT / Open Source
Last Updated:   2025-11-30

A Rust server plugin for automated daily restarts with customizable countdown
announcements.

================================================================================
FEATURES
================================================================================

- Daily Scheduled Restarts - Automatically restart your server at a configurable
  time each day
- Countdown Announcements - Broadcast countdown messages to players at
  customizable intervals
- Server Save & Backup - Optionally save and backup the server before restarting
- Admin Commands - Full control over restart scheduling and cancellation (console-only)
- Graceful Shutdown - Kicks all players with notification before shutting down

================================================================================
INSTALLATION
================================================================================

1. Place rDailyServerRestarts.cs in your oxide/plugins directory
2. Reload plugins or restart the server
3. A default configuration file will be created at
   oxide/config/rDailyServerRestarts.json

================================================================================
CONFIGURATION
================================================================================

Edit oxide/config/rDailyServerRestarts.json:

{
  "Enable daily restarts": true,
  "Daily restart time (HH:mm:ss format, 24-hour UTC)": "04:00:00",
  "Enable server save before restart": true,
  "Enable server backup before restart": true,
  "Countdown duration in minutes": 15,
  "Enable debug logging": false
}

CONFIGURATION OPTIONS:

- Enable daily restarts
  Turn daily automatic restarts on/off (boolean)

- Daily restart time
  Time to restart in HH:mm:ss format using 24-hour UTC (string)

- Enable server save before restart
  Run 'save' command before shutdown (boolean)

- Enable server backup before restart
  Run 'backup' command before shutdown (boolean)

- Countdown duration in minutes
  Used for future scheduling features (int, default: 15)

- Enable debug logging
  Show detailed debug messages in console (boolean, default: false)

================================================================================
COMMANDS
================================================================================

All commands are SERVER CONSOLE ONLY - they cannot be run from in-game chat.

rdsr.status
  Displays the current restart status and time remaining if a restart is
  scheduled.

rdsr.cancel
  Cancels the currently scheduled restart and broadcasts a cancellation message
  to all players. Shows when the next restart is scheduled.

rdsr.now
  Schedules an immediate restart in 10 seconds.

rdsr.schedule [seconds]
  Schedules a restart in X seconds (minimum 900 seconds / 15 minutes). If no
  argument provided, schedules for next day at configured daily restart time.

rdsr.test
  Dev command - tests the countdown sequence with 60-second timer.

================================================================================
HOW IT WORKS
================================================================================

DAILY SCHEDULING:
1. Time Check - Plugin checks once per second if the configured restart time
   has arrived
2. Restart Triggered - When DateTime.Now >= DailyRestartTime, the restart
   sequence begins
3. Countdown Starts - The countdown coroutine launches immediately

COUNTDOWN & SHUTDOWN SEQUENCE:
1. Silent Wait Period - Wait until "Countdown duration in minutes" before
   actual restart time
2. 5-Minute Interval Announcements - Console announcements at 15m, 10m, 5m
   before restart (server logs only)
3. Final Player Countdown - Broadcast to connected players only: 1 minute →
   30 seconds → 10 seconds → 5 seconds → NOW
4. Pre-Restart Actions - Execute server save (if enabled), then server
   backup (if enabled)
5. Player Kickoff - Kick all players with notification message
6. Graceful Exit - Plugin exits; external process manager handles server
   restart

IMPORTANT TIMING NOTE:
The restart time you set is when the actual shutdown happens, not when the
countdown begins.

If you set DailyRestartTime = "04:00:00":
  - Countdown window opens 15 minutes before (at 03:45:00)
  - Final announcements begin at 03:59:00
  - Actual shutdown occurs at 04:00:00 UTC

If you previously used a script that started at 03:45:00 to restart at 04:00:00,
set the plugin to:
  - DailyRestartTime: 04:00:00
  - Countdown duration in minutes: 15

================================================================================
RESTART SEQUENCE EXAMPLE
================================================================================

Example: Restart at 04:23:00 UTC with 15-minute countdown

CONSOLE MESSAGES (Server Logs):
03:08:00 UTC - Enter countdown mode
04:07:00 UTC - "Server restarting in 15 minutes"
04:12:00 UTC - "Server restarting in 10 minutes"
04:17:00 UTC - "Server restarting in 5 minutes"

PLAYER MESSAGES (In-Game Chat):
04:22:00 UTC - "Server restarting in 1 minute"
04:22:30 UTC - "Server restarting in 30 seconds"
04:22:50 UTC - "Server restarting in 10 seconds"
04:22:55 UTC - "Server restarting in 5 seconds"
04:23:00 UTC - "Server is restarting NOW!"
             - Execute server save (if enabled)
             - Execute server backup (if enabled)
             - Kick all players
             - Exit plugin (external manager restarts server)

MESSAGE ROUTING:
- Console Only (5-minute intervals): 15m, 10m, 5m announcements in server logs
- Players Only (final countdown): 1m, 30s, 10s, 5s, NOW in-game chat
- Players see exactly 5 messages during the final minute before shutdown

================================================================================
TROUBLESHOOTING
================================================================================

Restart not happening at scheduled time:
  - Check that 'Enable daily restarts' is set to 'true' in the config
  - Verify the 'Daily restart time' is in correct HH:mm:ss 24-hour UTC format
  - Check the server console for error messages
  - Reload the plugin with 'oxide.reload rDailyServerRestarts'

Announcements not showing:
  - Ensure 'Countdown announcement messages' array contains valid second values
  - Check that the values are in descending order for best results
  - Verify players have chat enabled

Server not quitting:
  - Ensure your server process manager can handle the 'quit' command
  - Check server logs for any errors during shutdown sequence
  - Verify save/backup commands are completing successfully if enabled

================================================================================
CREDITS
================================================================================

Based on examples from:
- RebootScheduler.cs - Restart scheduling implementation
- SmoothRestarter.cs - Countdown management patterns
- daily-restart.au3 - Original script functionality recreation

================================================================================
LICENSE
================================================================================

This plugin is provided under the MIT / Open Source license for use on Rust
servers.

================================================================================
SUPPORT & BUG REPORTS
================================================================================

For issues, feature requests, or contributions, visit:
https://github.com/FtuoilXelrash/rDailyServerRestarts

================================================================================

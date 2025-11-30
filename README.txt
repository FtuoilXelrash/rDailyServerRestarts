================================================================================
                    rDailyServerRestarts
================================================================================

Version:        0.0.1
Author:         Ftuoil Xelrash
License:        MIT / Open Source
Last Updated:   2025-11-29

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
- Admin Commands - Full control over restart scheduling and cancellation
- Graceful Shutdown - Kicks all players with notification before shutting down
- Permission System - Restrict restart commands to admins only

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
  "Countdown announcement messages": [
    3600,
    900,
    600,
    300,
    120,
    60,
    30,
    10,
    5,
    4,
    3,
    2,
    1
  ]
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

- Countdown announcement messages
  Array of seconds at which to announce countdown (int array)

================================================================================
COMMANDS
================================================================================

All commands require the rdailyserverrestarts.admin permission.

/restart status
  Displays the current restart status and time remaining if a restart is
  scheduled.

/restart cancel
  Cancels the currently scheduled restart and broadcasts a cancellation message
  to all players.

/restart now
  Schedules an immediate restart in 10 seconds.

================================================================================
PERMISSIONS
================================================================================

- rdailyserverrestarts.admin
  Allows use of all restart commands

================================================================================
HOW IT WORKS
================================================================================

1. Daily Check - Every frame, the plugin checks if it's time for the scheduled
   restart
2. Countdown Sequence - When restart time arrives, the plugin announces
   countdowns at configured intervals
3. Pre-Restart Actions - Optionally saves and backs up the server
4. Graceful Shutdown - Kicks all players with a notification message
5. Server Quit - Executes the 'quit' command to shut down the server

The server will automatically restart when the quit command completes (typically
handled by an external process manager or systemd).

================================================================================
RESTART SEQUENCE EXAMPLE
================================================================================

With default settings, at 04:00:00 UTC, the server will:

 1. Announce "Server restarting in 1h" (1 hour before)
 2. Announce "Server restarting in 15m" (15 minutes before)
 3. Announce "Server restarting in 10m" (10 minutes before)
 4. Announce "Server restarting in 5m" (5 minutes before)
 5. Announce "Server restarting in 2m" (2 minutes before)
 6. Announce "Server restarting in 1m" (1 minute before)
 7. Announce "Server restarting in 30s" (30 seconds before)
 8. Announce "Server restarting in 10s" (10 seconds before)
 9. Announce "Server restarting in 5s" (5 seconds before)
10. Announce individual second countdowns (4s, 3s, 2s, 1s)
11. Announce "Server is restarting now!"
12. Execute server save (if enabled)
13. Execute server backup (if enabled)
14. Kick all players
15. Execute server quit

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

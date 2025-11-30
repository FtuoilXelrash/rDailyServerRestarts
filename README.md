# rDailyServerRestarts

**Version:** 0.0.1
**Author:** Ftuoil Xelrash
**License:** MIT / Open Source
**Last Updated:** 2025-11-29

A Rust server plugin for automated daily restarts with customizable countdown announcements.

## Features

- **Daily Scheduled Restarts** - Automatically restart your server at a configurable time each day
- **Countdown Announcements** - Broadcast countdown messages to players at customizable intervals
- **Server Save & Backup** - Optionally save and backup the server before restarting
- **Admin Commands** - Full control over restart scheduling and cancellation
- **Graceful Shutdown** - Kicks all players with notification before shutting down
- **Permission System** - Restrict restart commands to admins only

## Installation

1. Place `rDailyServerRestarts.cs` in your `oxide/plugins` directory
2. Reload plugins or restart the server
3. A default configuration file will be created at `oxide/config/rDailyServerRestarts.json`

## Configuration

Edit `oxide/config/rDailyServerRestarts.json`:

```json
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
```

### Configuration Options

- **Enable daily restarts** - Turn daily automatic restarts on/off (boolean)
- **Daily restart time** - Time to restart in HH:mm:ss format using 24-hour UTC (string)
- **Enable server save before restart** - Run `save` command before shutdown (boolean)
- **Enable server backup before restart** - Run `backup` command before shutdown (boolean)
- **Countdown announcement messages** - Array of seconds at which to announce countdown (int array)

## Commands

All commands require the `rdailyserverrestarts.admin` permission.

### `/restart status`
Displays the current restart status and time remaining if a restart is scheduled.

### `/restart cancel`
Cancels the currently scheduled restart and broadcasts a cancellation message to all players.

### `/restart now`
Schedules an immediate restart in 10 seconds.

## Permissions

- `rdailyserverrestarts.admin` - Allows use of all restart commands

## How It Works

### Daily Scheduling
1. **Time Check** - Plugin checks once per second if the configured restart time has arrived
2. **Restart Triggered** - When `DateTime.Now >= DailyRestartTime`, the restart sequence begins
3. **Countdown Starts** - The countdown coroutine launches immediately

### Countdown & Shutdown Sequence
1. **Silent Wait Period** - Wait until "Countdown duration in minutes" before actual restart time
2. **5-Minute Interval Announcements** - Console announcements at 15m, 10m, 5m before restart (server logs only)
3. **Final Player Countdown** - Broadcast to connected players only: 1 minute → 30 seconds → 10 seconds → 5 seconds → NOW
4. **Pre-Restart Actions** - Execute server save (if enabled), then server backup (if enabled)
5. **Player Kickoff** - Kick all players with notification message
6. **Graceful Exit** - Plugin exits; external process manager handles server restart

### Important Timing Note
**The restart time you set is when the actual shutdown happens, not when the countdown begins.**

If you set `DailyRestartTime = "04:00:00"`:
- Countdown window opens 15 minutes before (at 03:45:00)
- Final announcements begin at 03:59:00
- **Actual shutdown occurs at 04:00:00 UTC**

If you previously used a script that started at 03:45:00 to restart at 04:00:00, set the plugin to:
- `DailyRestartTime`: 04:00:00
- `Countdown duration in minutes`: 15

## Restart Sequence Example

### Example 1: Restart at 04:23:00 UTC with 15-minute countdown

**Console Messages (Server Logs):**
- **03:08:00 UTC** - Enter countdown mode
- **04:07:00 UTC** - "Server restarting in 15 minutes"
- **04:12:00 UTC** - "Server restarting in 10 minutes"
- **04:17:00 UTC** - "Server restarting in 5 minutes"

**Player Messages (In-Game Chat):**
- **04:22:00 UTC** - "Server restarting in 1 minute"
- **04:22:30 UTC** - "Server restarting in 30 seconds"
- **04:22:50 UTC** - "Server restarting in 10 seconds"
- **04:22:55 UTC** - "Server restarting in 5 seconds"
- **04:23:00 UTC** - "Server is restarting NOW!"
  - Execute server save (if enabled)
  - Execute server backup (if enabled)
  - Kick all players
  - Exit plugin (external manager restarts server)

### Message Routing
- **Console Only** (5-minute intervals): 15m, 10m, 5m announcements appear in server logs
- **Players Only** (final countdown): 1m, 30s, 10s, 5s, NOW messages appear in-game chat to connected players
- Players see exactly 5 messages during the final minute before shutdown

## Troubleshooting

### Restart not happening at scheduled time
- Check that `Enable daily restarts` is set to `true` in the config
- Verify the `Daily restart time` is in correct HH:mm:ss 24-hour UTC format
- Check the server console for error messages
- Reload the plugin with `oxide.reload rDailyServerRestarts`

### Announcements not showing
- Ensure `Countdown announcement messages` array contains valid second values
- Check that the values are in descending order for best results
- Verify players have chat enabled

### Server not quitting
- Ensure your server process manager can handle the `quit` command
- Check server logs for any errors during shutdown sequence
- Verify save/backup commands are completing successfully if enabled

## Credits

Based on examples from:
- **RebootScheduler.cs** - Restart scheduling implementation
- **SmoothRestarter.cs** - Countdown management patterns
- **daily-restart.au3** - Original script functionality recreation

## Support & Bug Reports

For issues, feature requests, or contributions, visit:
https://github.com/FtuoilXelrash/rDailyServerRestarts

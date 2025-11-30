# rDailyServerRestarts

**Version:** 0.0.42
**Author:** Ftuoil Xelrash
**License:** MIT / Open Source
**Last Updated:** 2025-11-30

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
  "Countdown duration in minutes": 15,
  "Enable debug logging": false
}
```

### Configuration Options

- **Enable daily restarts** - Turn daily automatic restarts on/off (boolean)
- **Daily restart time** - Time to restart in HH:mm:ss format using 24-hour UTC (string)
- **Enable server save before restart** - Run `save` command before shutdown (boolean)
- **Enable server backup before restart** - Run `backup` command before shutdown (boolean)
- **Countdown duration in minutes** - Used for future scheduling features (int, default: 15)
- **Enable debug logging** - Show detailed debug messages in console (boolean, default: false)

## Commands

All commands are **server console only** - they cannot be run from in-game chat.

### `rdsr.status`
Displays the current restart status and time remaining if a restart is scheduled.

### `rdsr.cancel`
Cancels the currently scheduled restart and broadcasts a cancellation message to all players. Shows when the next restart is scheduled.

### `rdsr.now`
Schedules an immediate restart in 10 seconds.

### `rdsr.schedule [seconds]`
Schedules a restart in X seconds (minimum 900 seconds / 15 minutes). If no argument provided, schedules for next day at configured daily restart time.

### `rdsr.test`
Dev command - tests the countdown sequence with 60-second timer.

## Permissions

- `rdailyserverrestarts.admin` - Allows use of all restart commands

## How It Works

### Daily Scheduling
1. **Time Check** - Plugin checks once per second if the configured restart time has arrived
2. **Restart Triggered** - When `DateTime.Now >= DailyRestartTime`, the restart sequence begins
3. **Countdown Starts** - The countdown coroutine launches immediately

### Countdown & Shutdown Sequence
When within 15 minutes of restart time, the countdown begins:

1. **Initial Message** - Broadcast current time remaining (e.g., "Scheduled Daily Restart in 2m 15s")
2. **Multi-Stage Countdown** - Announce at: 15m, 10m, 5m, 3m, 1m, 30s, 10s, 5s, NOW (both console and in-game)
3. **Pre-Restart Delay** - Wait 2 seconds after "NOW!" message
4. **Server Save** - Execute `save` command (if enabled), wait 10 seconds
5. **Server Backup** - Execute `backup` command (if enabled), wait 10 seconds
6. **Player Kickoff** - Kick all connected players with notification message, wait 5 seconds
7. **Final Notice** - Print "Restarting server..." and wait 5 seconds
8. **Server Shutdown** - Execute `quit` command (external process manager restarts server)

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

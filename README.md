# rDailyServerRestarts

**Version:** 1.0.0
**Author:** Ftuoil Xelrash
**License:** MIT / Open Source
**Last Updated:** 2025-11-30

A Rust server plugin for automated daily restarts with customizable countdown announcements.

## Features

- **Daily Scheduled Restarts** - Automatically restart your server at a configurable time each day
- **Countdown Announcements** - Broadcast countdown messages to players at customizable intervals
- **Server Save & Backup** - Optionally save and backup the server before restarting
- **Admin Commands** - Full control over restart scheduling and cancellation (console-only)
- **Graceful Shutdown** - Kicks all players with notification before shutting down

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

### Player Commands

#### `!restart`
Shows the next scheduled restart time. Available to all players in-game chat with a 5-minute global cooldown on announcements.

**Usage:**
```
!restart
```

**Output Examples:**
- If restart active: `Server is restarting in X minutes Y seconds`
- If restart scheduled: `Next scheduled restart: HH:mm:ss UTC (X hours Y minutes)`
- If no restart scheduled: `No restart currently scheduled`
- If used too recently: `Restart info was just announced, check chat` (private message to player)

### Admin Commands (Server Console Only)

#### `rdsr.status`
Displays the current restart status and time remaining if a restart is scheduled.

#### `rdsr.cancel`
Cancels the currently scheduled restart and broadcasts a cancellation message to all players. Shows when the next restart is scheduled.

#### `rdsr.now`
Schedules an immediate admin-initiated restart in 5 minutes with "ADMIN INITIATED" notification to all players.

#### `rdsr.schedule [seconds]`
Schedules a restart in X seconds (minimum 900 seconds / 15 minutes). If no argument provided, schedules for next day at configured daily restart time.


## How It Works

### Daily Scheduling
1. **Time Check** - Plugin checks once per second if the configured restart time has arrived
2. **Restart Triggered** - When within the configured countdown minutes of restart time, sequence begins

### Countdown & Shutdown Sequence
When within configured countdown minutes of restart time, the countdown begins:

1. **Initial Message** - Broadcast current time remaining (e.g., "Scheduled Daily Restart in 22m 15s")
2. **Multi-Stage Countdown** - Announce at: 5-minute intervals down to 1m, then 30s, 10s, 5s, NOW (both console and in-game)
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

### Message Details
- All countdown announcements are broadcast to both console and in-game chat
- Countdown triggered when within configured minutes of restart time
- Final announcements at: 5-minute intervals down to 1m, then 30s, 10s, 5s, NOW

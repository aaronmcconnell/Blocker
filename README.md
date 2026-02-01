# Blocker

**Blocker** is a lightweight background service that restricts access to selected hosts (domains or IP addresses) according to a configurable schedule. It is designed to provide *time-of-day–based* blocking as a practical productivity aid, rather than an all-or-nothing solution.

Unlike browser extensions or permanent blocking tools, Blocker allows websites to be accessible outside defined time windows, making it suitable for focused work periods without removing access entirely.

## How it works

Blocker operates by:

1. Reading a schedule and list of domains from configuration
2. Updating the system `hosts` file during active blocking windows
3. Flushing the system DNS cache so changes take effect immediately
4. Restoring the original `hosts` entries when the blocking window ends

The service runs continuously in the background and evaluates whether blocking should be active based on the current time and day.

> #### ℹ️ Subdomains  
> Hosts-file entries apply to **exact hostnames only** and do not support wildcards. Blocking a root domain such as: 
> - `site-to-block.com` 
>
> will **not** automatically block subdomains such as:
> - `www.site-to-block.com`
> - `m.site-to-block.com`
> 
> To fully block access to a site, you may need to explicitly include common subdomains (for example, both `site-to-block.com` and `www.site-to-block.com`).

## Features

- Time-of-day based website blocking
- Per-day scheduling
- Multiple domains per schedule
- Automatic DNS cache flushing
- Designed to run unattended as a background service
- Simple, explicit configuration

## OS Support

|Operating System|Supported|
|---|---|
|**Linux**| ✅ Yes |
|**Windows**| ✅ Yes |
|**macOS**| ❌ No |

> ⚠️ **Administrator / root privileges are required**, as the service modifies the system `hosts` file.

## Pre-Requisites
Before you can run Blocker, the [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) must be installed on your system.

## Configuration

Blocker is configured using an `appsettings.json` file.

An example configuration file `appsettings.json.example` is included in the repository. Create a copy with the correct filename using the command below;

**Linux**
```bash
cp appsettings.json.example appsettings.json
```

**Windows**
```cmd
copy "appsettings.json.example" "appsettings.json"
```

You can then edit the `appsettings.json` file to specify the sites you wish to block, and the schedule for blocking them.

### Example configuration

```json
{
  "BlockerService": {
    "HostsFilePath": "/etc/hosts", 
    "UrisToBlock": [
      {
        "UriToBlock": "example.com",
        "ActiveDays": [ "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" ],
        "BlockFrom": "09:00",
        "BlockTo": "17:00"
      },
      {
        "UriToBlock": "example-2.com",
        "ActiveDays": [ "Saturday", "Sunday" ],
        "BlockFrom": "06:00",
        "BlockTo": "12:00"
      }
    ]
  }
}
```

The `HostsFilePath` on Linux systems is usually located at `/etc/hosts` (as per the above example). On Windows systems it is usually located at `C:\Windows\System32\drivers\etc\hosts`.

### Configuration fields

| Field | Description |
|------|------------|
| `HostsFilePath` | Path to the system hosts file |
| `UriToBlock` | Domain / IP Address to be blocked |
| `ActiveDays` | Days on which the block applies |
| `BlockFrom` | Start time (24-hour clock) |
| `BlockTo` | End time (24-hour clock) |

> ℹ️ Blocking windows are currently assumed to occur within a single day (e.g. `09:00 → 17:00`). Overnight windows such as `22:00 → 02:00` are not yet supported.

## Running the service

### Development

```bash
dotnet run
```

### Publish a standalone build

```bash
dotnet publish -c Release
```

The output binary can be run directly on the target system.

## Running as a background service

Blocker is intended to run unattended.

### Linux (systemd example)

```ini
[Unit]
Description=Blocker website scheduling service

[Service]
ExecStart=/path/to/Blocker
Restart=always
User=root

[Install]
WantedBy=multi-user.target
```

> The service must run with sufficient permissions to modify the hosts file.

### Windows

Blocker can be run:
- via **Task Scheduler**, or
- wrapped as a Windows service using tools such as `sc.exe` or NSSM

## Safety notes

- Blocker modifies the system `hosts` file  
- Always ensure you have a backup `hosts` file before first use
- Misconfiguration may temporarily affect networking until corrected

## Design decisions

- **Hosts-file based blocking** was chosen for reliability and OS-level enforcement
- **Explicit configuration** over UI to keep behaviour predictable and auditable
- **Background service model** to avoid reliance on user interaction or browser state
- **Partial blocking** to support behavioural change without permanent restriction

## Limitations / Future Improvements
- Currently **macOS** is not supported
- Currently **overnight blocking windows** (crossing midnight) are not supported

## License

This project is released under **The Unlicense**.

You are free to use, modify, and redistribute it without restriction.

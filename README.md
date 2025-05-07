# DiffScribe

DiffScribe is a CLI tool that scans your staged Git changes and generates a straightforward, well-structured commit message in one command.
It helps turn diffs into clear commit messages quickly and easily.

---

## 1. Requirements
| Requirement        | Why it is needed                                                                                                       |
|--------------------|------------------------------------------------------------------------------------------------------------------------|
| **Git**            | DiffScribe inspects your staged diffs to craft the message and can optionally automatically commit for you.            |
| **OpenAI API Key** | Commit messages are produced by the selected OpenAI model. You must provide a valid key once via the `config` command. |

---

## 2. Installation

1. Download the latest release from [here](https://github.com/sudosys/diffscribe/releases).
2. Extract it and `cd` into the extracted folder.
3. Run the platform-specific installer script from inside that folder:

```powershell
# Windows
   .\install.ps1
```


```shell script
# macOS/Linux
   ./install.sh
```

The script adds the executable called `dsc` to your PATH and copies the necessary files to the installation directory.
<br />After running the script, restart your terminal session and execute `dsc` to confirm the installation is successful:

---

## 3. Usage Overview

```plain text
dsc <command> [options]
```

Instead of the full command name, only the first letter of the command can be used instead.

```shell script
# e.g.
dsc g
# Instead of
dsc generate
```

Use `dsc help` or `dsc` at any time for an interactive command list.

---

## 4. Command Reference

### 4.1 `generate`

Crafts a commit message from **staged** changes.

| Syntax                         | Description |
|--------------------------------|-------------|
| `dsc generate [--auto-commit]` | Generates and prints the message. Optionally commits it. |

| Argument | Type | Optional | Meaning                                                                                                                                                                           |
|----------|------|----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--auto-commit` | `bool` | Yes | After generation, DiffScribe prompts you for confirmation; if you approve, it commits the staged changes automatically.<br/> If omitted, the message is copied to your clipboard. |

Examples
```shell script
# Generate message, copy it to clipboard
dsc generate

# Generate message and commit automatically after confirmation
dsc generate --auto-commit
```

---

### 4.2 `config`

Displays or edits the tool configuration.

| Syntax                 | Description |
|------------------------|-------------|
| `dsc config`           | Show the current configuration in a table. |
| `dsc config <options>` | Update one or more settings. |

| Option | Value               | Optional | Purpose                                                                        |
|--------|---------------------|----------|--------------------------------------------------------------------------------|
| `--commit-style` | _N/A (interactive)_ | Yes | Choose between *Minimal*, *Standard*, *Detailed*.<br />(see below for details.) |
| `--api-key` | `<OPENAI_API_KEY>`  | Yes | Store or replace your OpenAI API key.                                          |
| `--llm` | _N/A (interactive)_ | Yes | Select the OpenAI model to be used for generation.                             |
| `--auto-commit` | `true` / `false`    | Yes | Set the default for `generate --auto-commit`.|

Examples
```shell script
# Review current settings
dsc config

# One-time API key setup
dsc config --api-key <OPENAI_API_KEY>

# Switch to detailed commit messages
dsc config --commit-style
```

#### 4.2.1 Commit style options

| Style     | Level of Detail                                                           |
|-----------|---------------------------------------------------------------------------|
| Minimal   | A very short, one-line summary. No body or footers.                       |
| Standard  | A clear summary line without body or footers.                             |
| Detailed  | A summary followed by a descriptive body and/or footers when appropriate. |


---

### 4.3 `reset`

Restores factory defaults (including removal of your API key).

| Syntax      | Description |
|-------------|-------------|
| `dsc reset` | Prompts for confirmation, then wipes the config file. |

Example
```shell script
dsc reset
```

---

### 4.4 `help`

Shows command information.

| Syntax                 | Description |
|------------------------|-------------|
| `dsc help`             | List all commands. |
| `dsc help --<command>` | Show detailed help for a single command. |

Example
```shell script
# Learn about the generate command
dsc help --generate
```

---

### 4.5 `update`

Checks for available updates and downloads the latest version if one is found.

| Syntax         | Description                                        |
|----------------|----------------------------------------------------|
| `dsc update`   | Checks if an update is available and downloads it. |

When run, this command will check for a newer version of DiffScribe.
If an update is detected, the required files will be downloaded and the user informed.
If the tool is already up to date, it will notify you accordingly.

---

### 4.6 `version`

Displays the current version of the CLI tool.

| Syntax         | Description                                   |
|----------------|-----------------------------------------------|
| `dsc version`  | Prints the current version to the console.    |

---

### 4.7 `uninstall`

Uninstalls DiffScribe from your system.

| Syntax           | Description                                      |
|------------------|--------------------------------------------------|
| `dsc uninstall`  | Prompts for confirmation and removes DiffScribe. |

Running this command will prompt you to confirm whether you wish to uninstall DiffScribe.
<br/>Upon confirmation, the tool will run the appropriate uninstallation script for your operating system, remove the application files, and provide feedback on the process.

---

## 5. First-time Quick-start

1. `dsc config --api-key <OPENAI_API_KEY>`
2. Stage some changes: `git add .`
3. `dsc generate` – voilà! Your clipboard now holds a commit message tailored to the diff.

Happy, effortless committing!
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

1. Download the latest release from [here](https://github.com/sudosys/diffscribe/releases) and extract it.
2. Open up a terminal on Linux/macOS or PowerShell (as admin) on Windows.
3. Change the working directory to the extracted folder.
4. Run the platform-specific installer script from inside that folder:

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

| Syntax                     | Description |
|----------------------------|-------------|
| `dsc generate [<options>]` | Generates and prints the message. Optionally commits or amends it. |

| Argument | Type | Optional | Meaning                                                                                                                                                                                                                    |
|----------|------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--auto-commit` | `bool` | Yes | After generation, DiffScribe prompts you for confirmation; if you approve, it commits the staged changes automatically.<br/> If omitted, the message is copied to your clipboard.                                            |
| `--amend` | `bool` | Yes | Regenerates the message of the latest commit from its own changes **plus** the extra staged changes, then amends the commit with it after confirmation. Cannot be combined with `--auto-commit`.                             |
| `--commit-style` | _N/A (interactive)_ | Yes | Opens the commit style menu, then generates with the style you pick. Applies to this generation only and never touches your configuration.                                                                                   |
| `--commit-len` | `string` | Yes | **Maximum** subject line length for this generation only. A preset (*Short*, *Standard*, *Long*) or a custom character count.<br />(see [commit length options](#422-commit-length-options).)                                 |
| `--steer` | `string` | Yes | Extra instruction in natural language that steers the generation.                                                                                                                                                          |

Examples
```shell script
# Generate message, copy it to clipboard
dsc generate

# Generate message and commit automatically after confirmation
dsc generate --auto-commit

# Stage the files you forgot and fold them into the latest commit
git add src/forgotten-file.cs
dsc generate --amend

# Pick a style from the menu and cap the subject line, for this run only
dsc generate --commit-style --commit-len 50

# Steer the wording of the message
dsc generate --steer "mention the ticket id DSC-42 in the scope"
```

---

### 4.2 `config`

Displays or edits the tool configuration.

| Syntax                 | Description |
|------------------------|-------------|
| `dsc config`           | Show the current configuration in a table. |
| `dsc config <options>` | Update one or more settings. |

| Option | Value               | Optional | Purpose                                                                                                    |
|--------|---------------------|----------|------------------------------------------------------------------------------------------------------------|
| `--commit-style` | _N/A (interactive)_ | Yes | Choose between *Minimal*, *Standard*, *Detailed*.<br />(see [commit style options](#421-commit-style-options).) |
| `--commit-len` | _N/A (interactive)_ | Yes | Choose a subject line length preset or set a custom one.<br />(see [commit length options](#422-commit-length-options).) |
| `--api-key` | `<OPENAI_API_KEY>`  | Yes | Store or replace your OpenAI API key.                                                                      |
| `--llm` | _N/A (interactive)_ | Yes | Select the OpenAI model to be used for generation.<br />(see [model options](#423-model-options).)          |
| `--auto-commit` | `true` / `false`    | Yes | Set the default for `generate --auto-commit`.|

Examples
```shell script
# Review current settings
dsc config

# One-time API key setup
dsc config --api-key <OPENAI_API_KEY>

# Switch to detailed commit messages
dsc config --commit-style

# Pick a subject line length
dsc config --commit-len
```

#### 4.2.1 Commit style options

| Style     | Level of Detail                                                           |
|-----------|---------------------------------------------------------------------------|
| Minimal   | A very short, one-line summary. No body or footers.                       |
| Standard  | A clear summary line without body or footers.                             |
| Detailed  | A summary followed by a descriptive body and/or footers when appropriate. |

#### 4.2.2 Commit length options

The commit length is an **upper limit** on the **subject line** (the first line) of the generated
message — not a target. A short change still gets a short subject; the limit only stops the model
from running long.

| Preset   | Maximum subject line | Why                                                       |
|----------|----------------------|-----------------------------------------------------------|
| Short    | 50 characters        | Compact subject line that stays readable in every git client. |
| Standard | 72 characters        | Subject line length recommended by git.                   |
| Long     | 100 characters       | Roomy subject line for changes that need more wording.    |

A custom length is accepted as a character count between **20** and **120**.
Anything outside that range is rejected: below 20 characters a conventional commit subject cannot
carry a type, a scope and a description, and above 120 characters the subject gets cut off by
common git tooling anyway.

```shell script
# Preset, for this generation only
dsc generate --commit-len short

# Custom length, for this generation only
dsc generate --commit-len 64
```

#### 4.2.3 Model options

| Model         | Profile                                                                  |
|---------------|--------------------------------------------------------------------------|
| GPT-5.6 Terra | Balances intelligence and cost. Pick it for large or subtle changes.     |
| GPT-5.6 Luna  | Fast and cheap. Default, and plenty for everyday commit messages.        |
| GPT-5.4 mini  | Lightweight model of the previous generation.                            |
| GPT-5.4 nano  | Smallest and quickest option.                                            |

DiffScribe sends every request with `reasoning_effort: none`, so no model spends time thinking
before it answers. Commit messages are a summarisation job, and keeping reasoning off makes the
generation noticeably faster and cheaper on all four models.


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

Mistyped a command or an argument? DiffScribe recommends the closest match instead of just failing.

Happy, effortless committing!
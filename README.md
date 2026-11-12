# snap

A tiny git clone written in C#. Mostly built to actually understand what `git add`/`commit`/`checkout` are doing under the hood instead of just trusting the magic.


## Requirements

- .NET 10.0 SDK

## Build

```
dotnet build
```

## Usage

```
snap init                   # set up a .snap folder here
snap add <path>              # stage a file, or a whole folder recursively (also stages deleted files)
snap rm <path>               # stop tracking a file and delete it
snap rm -r <folder>          # same, for a whole folder
snap rm --cached <path>      # stop tracking, but keep the file on disk
snap commit -m "message"     # commit whatever's staged
snap log                     # walk through commit history
snap checkout <branch>       # switch to an existing branch
snap checkout -b <branch>    # create a new branch and switch to it
snap version                 # print current version
snap help                    # list all commands
snap branch                  # shows all branches
snap branch   -d <branch>    # deletes the branch
snap reset <commit-hash>     # resets current branch hash to the targeted hash.
snap author                  # show who commits are attributed to
snap author -set <name>      # set the author for this repo
```

`init` asks for a new password (twice). `add`, `commit`, `merge`, `log`, `checkout` and `reset` ask for it every time they run. There's no way to recover a repo if the password is lost.

## How it stores things

Everything lives under `.snap/objects`. Every object (blob, tree, or commit) gets hashed with HMAC-SHA256, then split into a 2-character folder plus the rest as the filename, same trick real git uses so you don't end up with thousands of files dumped into one folder. Blobs are just raw file bytes. Trees and commits are JSON with a small header in front so you can tell what you're looking at when you read the file back.

Objects are encrypted on disk with AES-256-GCM. The key comes from your password via PBKDF2-SHA256 (600,000 iterations, random salt in `.snap/salt`), and HKDF splits it into one key for naming objects and one for encrypting them. The key itself is never written to disk; `.snap/keycheck` only holds an HMAC that proves a password is correct. `index`, `HEAD` and `refs/` stay plain text.

Branches live at `.snap/refs/heads/<name>` and hold a single commit hash. `HEAD` says which branch you're currently sitting on. Full history comes from each commit pointing back at its parent, not from the ref itself.


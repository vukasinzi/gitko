# gitko

A tiny git clone written in C#. Mostly built to actually understand what `git add`/`commit`/`checkout` are doing under the hood instead of just trusting the magic.


## Requirements

- .NET 10.0 SDK

## Build

```
dotnet build
```

## Usage

```
gitko init                   # set up a .gitko folder here
gitko add <path>              # stage a file, or a whole folder recursively
gitko commit -m "message"     # commit whatever's staged
gitko log                     # walk through commit history
gitko checkout <branch>       # switch to an existing branch
gitko checkout -b <branch>    # create a new branch and switch to it
gitko version                 # print current version
gitko branch                  # shows all branches
gitko branch   -d <branch>    # deletes the branch
git reset <commit-hash>       # resets current branch hash to the targeted hash.
```

## How it stores things

Everything lives under `.gitko/objects`. Every object (blob, tree, or commit) gets hashed with SHA-256, then split into a 2-character folder plus the rest as the filename, same trick real git uses so you don't end up with thousands of files dumped into one folder. Blobs are just raw file bytes. Trees and commits are JSON with a small header in front so you can tell what you're looking at when you read the file back.

Branches live at `.gitko/refs/heads/<name>` and hold a single commit hash. `HEAD` says which branch you're currently sitting on. Full history comes from each commit pointing back at its parent, not from the ref itself.


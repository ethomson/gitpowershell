GitPowerShell
=============

A Git PowerShell module that exposes LibGit2Sharp objects.  This is
useful for scripting git repositories via PowerShell.

For interactive use, might I recommend the exceptional
[posh git](https://github.com/dahlbyk/posh-git) instead?

Commands
========

Add-GitIndexEntry
-----------------
Adds a file or files into the staging area (index) to be committed.

Commit-GitRepository
--------------------
Commits the staged changes.

Get-GitBranches
---------------
Lists the branches in a repository.

Get-GitStatus
-------------
Displays the status of the repository: the staged changes in the index
and the changes in the working directory.

Initialize-GitRepository
------------------------
Creates a new Git repository.

Open-GitRepository
------------------
Opens a Git repository, returning an object that can be further
manipulated with GitPowerShell commands.

Remove-GitIndexEntry
--------------------
Removes a staged change from the index, undoing the changes.

Copyright
=========

Available under an MIT license.  Copyrights are of the respective authors.
All rights reserved.


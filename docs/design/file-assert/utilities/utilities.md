# Utilities Subsystem Design

## Overview

The Utilities subsystem provides shared helper functionality used by other subsystems. It
contains security-sensitive or otherwise reusable operations that do not belong to any specific
domain subsystem.

## Subsystem Contents

| Unit          | File             | Responsibility                                                |
| :------------ | :--------------- | :------------------------------------------------------------ |
| `PathHelpers` | `PathHelpers.cs` | Safe path-combination utility with path-traversal protection. |

## Subsystem Responsibilities

- Provide path utilities that safely combine paths while preventing path-traversal attacks.
- Reject relative paths containing `..` or absolute paths when a relative path is expected.

## Interactions with Other Subsystems

| Consumer  | Usage                                                                    |
| :-------- | :----------------------------------------------------------------------- |
| SelfTest  | Uses `PathHelpers.SafePathCombine` when creating temporary log files.    |

## Design Decisions

- **Static class**: `PathHelpers` is a static utility class with no instance state, suitable
  for use anywhere in the codebase without injection.
- **Defense-in-depth validation**: Path safety is validated both before and after combining
  paths, guarding against edge cases that might bypass the initial checks.

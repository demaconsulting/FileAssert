---
title: Getting started
summary: Install FileAssert, write a first test suite, and run it.
aliases:
  - start
  - quickstart
  - examples
tags:
  - tutorial
order: 1
see-also:
  - command-line
  - configuration
  - troubleshooting
examples:
  - name: version
    kind: command
    language: shell
    description: Print the version and exit.
    content: '{{tool}} --version'
  - name: no-config
    kind: command
    language: shell
    description: Without a configuration file, the tool says how to start. Exits 0.
    content: '{{tool}}'
    expect: 'No configuration file found'
  - name: first
    kind: source
    language: yaml
    save-as: fileassert.yaml
    description: A first test suite, which checks a build report.
    content: |
      tests:
        - name: Report
          tags:
            - docs
          files:
            - pattern: report.md
              count: 1
              text:
                - contains: 'Status: Passed'
  - name: run
    kind: command
    language: shell
    description: Run the test suite. Exits 0 when every assertion passes.
    content: '{{tool}} --config fileassert.yaml'
  - name: topic
    kind: command
    language: shell
    description: Print a help topic, here the format of the configuration file.
    content: '{{tool}} help configuration'
    expect: 'A test suite is a YAML file'
---
{{tool}} {{version}} is a .NET tool.
It checks files against a test suite written in YAML.

1. **Install it.** Install the tool from NuGet, globally or in a local
   tool manifest:

   ```bash
   dotnet tool install -g DemaConsulting.FileAssert
   ```

2. **Write a test suite.** A test suite is a YAML file with a list of
   tests. Each test names file patterns and what the matching files must
   hold. [[#first]] prints a small one, and
   [[configuration]] describes the format.
3. **Run it.** The tool reads `.fileassert.yaml` in the current folder,
   or the file that `--config` names, as the command that [[#run]]
   prints does. Patterns are relative to the folder of that file. Without
   a configuration file the tool only says so, and exits 0.
4. **Use the result.** The tool exits 0 when every assertion passes and 1
   when one fails, so a build step can fail on it. A passing run prints
   only the banner; every failure prints one line. `--results` also
   writes the outcome of each test as a TRX or JUnit file.

Print any help topic with `{{tool}} help <topic>`, as in the command that
[[#topic]] prints. Add `--search <words>` to find a topic, and `--json` to
get any help answer as JSON.

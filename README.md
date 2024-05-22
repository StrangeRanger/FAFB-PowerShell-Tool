# Active Directory Querier

[![Project Tracker](https://img.shields.io/badge/repo%20status-Project%20Tracker-lightgrey)](https://wiki.hthompson.dev/en/project-tracker)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
[![Style Guide](https://img.shields.io/badge/code%20style-Style%20Guide-blueviolet)](https://gist.github.com/StrangeRanger/f7f87dd884760f3127adda98d3d4ab14)

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/StrangeRanger/FAFB-PowerShell-Tool/build-test-ci.yml)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/ce2ddca07a934a5f85e8061e295f3324)](https://app.codacy.com/gh/StrangeRanger/FAFB-PowerShell-Tool/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
![Static Badge](https://img.shields.io/badge/state-Beta-orange)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/StrangeRanger/FAFB-PowerShell-Tool)

Active Directory Querier is a Windows GUI for creating, saving, and executing one or more PowerShell commands against a Local Active Directory.

## Getting Started

### Installing

To begin using Active Directory Querier, no additional software installation is required. Simply go to the [latest releases tab](https://github.com/StrangeRanger/FAFB-PowerShell-Tool/releases) and select the version that matches your architecture.

## Development

If you are interested in pursuing or expanding this project, here are the requirements you will need to begin:

- One or more of the following IDEs:
  - [JetBrain's Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio](https://visualstudio.microsoft.com/)
  - [Visual Studio Code](https://code.visualstudio.com/)
- On the system you'll be doing your development on, install the following:
  - [.Net 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is the language we have used to program the application.
  - [Remote Server Administration Tools](https://activedirectorypro.com/install-rsat-remote-server-administration-tools-windows-10/#rsat-powershell) will make all Active Directory commands available to the program.
- Set up your dev/test environment:
  - [Windows Server 2019](https://www.microsoft.com/en-us/evalcenter/download-windows-server-2019) – Will act as your Active Directory Domain host.
  - [Windows Enterprise](https://www.microsoft.com/en-us/evalcenter/evaluate-windows-10-enterprise) – Will act as a single system on the Active Directory Domain.
  - [Setting up an AD dev environment](https://activedirectorypro.com/create-active-directory-test-environment/) – Instruction on how to get a working test environment set up.

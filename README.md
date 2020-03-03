# dfc-composite-healthmonitor

## Introduction

The purpose of this repo is to provide health checking functionality for Composite UI applications

Details of the Composite UI application may be found here https://github.com/SkillsFundingAgency/dfc-composite-shell

## Getting Started

Composite Shell will mark a region as unhealthy when it's repeatedly (based on Polly retries) unable to connect to a child app.
The solution consists of an Azure function app which is to run on a timer trigger to periodically re-check the child app and make composite page regions Healthy again. It does this by updating the appropriate regions Cosmos registration document.

## How to run locally
Clone the project and open the solution in Visual Studio 2019.
Clone and run the Paths function app found at the following repo: https://github.com/SkillsFundingAgency/dfc-composite-paths
Clone and run the Regions function app found at the following repo: https://github.com/SkillsFundingAgency/dfc-composite-regions
Ensure Cosmos Emulator is running locally and contains registration documents

## Local Config Files

Once you have cloned the public repo you need to remove the -template part from the configuration file names listed below.

| Location | Repo Filename | Rename to |
|-------|-------|-------|
| DFC.Composite.HealthMonitor | local.settings-template.json | local.settings.json |
| DFC.Composite.HealthMonitor.IntegrationTests | appsettings-template.json | appsettings.json |

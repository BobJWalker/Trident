# Trident

An application to skewer the news you care about from across the web by loading it in the dumbest way possible.

## Introduction

Welcome to the Trident Application! This application is written in .NET with the goal of demonstrating how to build an application using GitHub Actions and deploy it using Octopus Deploy.  

## Application Components

The components of this application are:

- Web front-end (single application)
  - ASP.NET MVC Application
  - Background job processor
- Database backend for SQL Server

## Key Software Delivery Functionality

The purpose of this application is to demonstrate common software delivery techniques using artifacts built by GitHub Actions and deployed via Octopus Deploy.

- Same application, multiple destinations
  - [Kubernetes Deployments](https://octopus.com/docs/kubernetes)
    - Both an ARM and x86-64 container is created during the build
    - The `k8s` folder contains
      - Helm Chart in `k8s/chart`
      - Manifest files in `k8s/base`
      - Kustomize overlays in `k8s/overlays`
      - [Argo CD](https://octopus.com/docs/argo-cd) application definitions in `k8s/argo-apps`
  - Windows / Linux Deployments via a package `Trident.Web.zip`
- [Database Deployments](https://octopus.com/docs/deployments/databases) using scripts found in `/src/Trident.Database.DbUp`
  - The CLI tool uses [DBUp](https://dbup.github.io) to run migration scripts as part of the deployment.
  - The CLI database deployment tool can generate an HTML report of the migration scripts that will be run.  
- [Multi-Tenant Deployments](https://octopus.com/docs/tenants)
  - The application uses configuration files found in `src/Trident.Web/wwwroot/data` to load data specific to that location.
  - The environment variable `TRIDENT_FILE_NAME` is used to specify a specific file on startup.
- [Feature Flags](https://octopus.com/docs/feature-flags)
  - Implements two [OpenFeature](https://openfeature.dev) providers
    - [Octopus Deploy](https://octopus.com/docs/feature-flags) when the `TRIDENT_OPEN_FEATURE_CLIENT_ID` environment variable is set
    - Environment Variable Provider for testing new features locally
  - See `/src/Trident.Web/Core/Configuration/TridentFeatureFlag.cs` for a complete list of feature flags available
- [Security Scanning, SBOM and Attestation verification](https://octopus.com/devops/security/supply-chain-security-with-github-actions-and-octopus-deploy/)
  - Build process
    - [Trivy](https://trivy.dev) is used to scan for third-party libraries with known vulnerabilities, which is uploaded to GitHub
    - [Trivy](https://trivy.dev) also builds an SBOM which is packaged and sent to Octopus Deploy
    - A hash is created of all the build artifacts and included in the Attestation
  - Deploy process
    - You can use [Trivy](https://trivy.dev) to scan the SBOM for any new critical vulnerabilities since the artifacts were built
    - Use can use `gh attestation verify` to verify the attestations of the build artifacts
- [Build information](https://octopus.com/docs/packaging-applications/build-servers/build-information/)
  - The GitHub action publishes build information to Octopus Deploy for the SBOM package only.
  - This allows you to create release notes listing all the commits and issues included in the change.
- [Ephemeral Environments](https://octopus.com/docs/projects/ephemeral-environments/)
  - The application is simple enough that all it needs is a database and a K8s namespace with a host name to create an ephemeral environment.

## Running Locally

The application can run directly using the .NET runtime or as a container.

### Create Database

The application requires a database running on SQL Server.  

1. Download and install SQL Server Developer 2022+ or SQL Server Express 2022+ using [one of these quick links](https://www.mssqltips.com/sqlservertip/6429/sql-server-download-quick-links/)
2. Launch the .NET IDE of your choice (VS Code or Visual Studio)
   1. Modify the connection string in `/src/Trident.Database.DbUp/Properties/launchsettings.json` to point to your database.
   2. Build and Run the `/src/Trident.Database.DbUp/` application in your IDE of choice.

### Run the application

1. Launch the .NET IDE of your choice
   1. Modify the connection string in `/src/Trident.Web/Properties/launchSettings.json` to point to your database.
   2. Run `/src/Trident.Web` application

#### Understanding Launch Settings

The file `/src/Trident.Web/Properties/launchSettings.json` is used to control the application locally.  It makes it easy to pass in environment variables that would've been passed in when launching the application in Production.

```json
{
  "profiles": {
    "Trident.Web": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "TRIDENT_CONNECTION_STRING": "Server=127.0.0.1;Integrated Security=True;Database=Trident;trustServerCertificate=true;Encrypt=True;",        
        "TRIDENT_OPEN_FEATURE_CLIENT_ID": "blah",
        "TRIDENT_FILE_NAME": "utah",
        "FeatureToggle_view-feed-list": "True",
        "FeatureToggle_FeatureBackgroundSyncJob": "True"
      },
      "applicationUrl": "https://localhost:57512;http://localhost:57513"
    }
  }
}
```

- Items prefixed with `FeatureToggle_` are how you can test feature toggles locally, they hae a simple setting of True or False.  
- `blah` is the default values for items such as Open Feature Client Id, SumoLogic URL, and more.  That phrase was chosen as it is unique and stands out.
- `TRIDENT_FILE_NAME` indicates the configuration file to load from `src/Trident.Web/wwwroot/data`


## Forking

This application is designed for anyone to fork.  Once you fork this application you'll need to set the following repository secrets for the build.yml to work correctly.

1. `DOCKERHUB_PAT` - The PAT to log into Docker Hub
2. `DOCKERHUB_USERNAME` - The Username to log into Docker Hub
3. `OCTOPUS_OIDC_SERVICE_ACCOUNT_ID` - The [Service Account Unique ID](https://octopus.com/docs/api/authentication/openid-connect) to login via OIDC

You'll need to set the following repo variables for the build.yml to work correctly.

1. `DOCKER_HUB_REPO` - The dockerhub repo to push the container to, IE `bobjwalker99/trident`.
2. `OCTOPUS_SERVER_URL` - The URL of the Octopus instance, IE `https://bobjwalker.octopus.app`.
3. `OCTOPUS_SPACE` - The space name that hosts the Octopus Project, IE `Default`.
4. `OCTOPUS_PROJECT_NAME` - The Octopus Deploy project name, IE `Trident`.
5. `OCTOPUS_FEATURE_BRANCH_CHANNEL` - The channel to use when running the build on branches or PRs, IE `Branches`.
6. `OCTOPUS_RELEASE_CHANNEL` - The channel to use when running the build on the main branch, IE `Release`.

The `build.yml` file expects you to have two channels in your project in Octopus Deploy.

- **Default**: Deploys to the `Ephemeral Environments` or `Development` environment only for feature branch work
- **Release**: Deploys to `Test` -> `Staging` -> `Production`.  This is what the main branch uses.

## Contributing

Please see the [Contribution Guidelines](https://github.com/bobjwalker/trident?tab=contributing-ov-file) on how to contribute to this project.

## License

The Trident Application is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Contact

If you have any questions or feedback, please create an issue.

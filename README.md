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

Follow these steps to run locally:

1. Clone the repository: `git clone https://github.com/bobjwalker/Trident.git`
2. Open the project in your preferred .NET IDE.
3. You can run the application locally, or as a container.  
4. You will need to create the database.  
   1. You can do this by running `/src/Trident.Database.DbUp` and providing a connection string in the `/src/Trident.Database.DbUp/properties/launchSettings.json` file. 
   2. OR, you can go to `/src/Trident.Database.DbUp/DeploymentScripts` and running them all in order
5. Once the database is created, you'll need to set the connection string in the environment variable `TRIDENT_CONNECTION_STRING`.

## Forking

If you want to fork this repository, please feel free to do so!  

Once you fork this application you'll need to set the following repository secrets for the build.yml to work correctly.

1. `DOCKERHUB_PAT`
2. `DOCKERHUB_USERNAME`
3. `OCTOPUSSERVERAPIKEY`
4. `OCTOPUS_SERVER_URL`

You'll need to set the following repo variables for the build.yml to work correctly.

1. `DOCKER_HUB_REPO`
2. `OCTOPUS_PROJECT_NAME`
3. `OCTOPUS_SPACE`

The `build.yml` file expects you to have two channels in your project in Octopus Deploy.

- **Default**: Deploys to the `Ephemeral Environments` or `Development` environment only for feature branch work
- **Release**: Deploys to `Test` -> `Staging` -> `Production`.  This is what the main branch uses.

## Contributing

Please see the [Contribution Guidelines](https://github.com/bobjwalker/trident?tab=contributing-ov-file)

## License

The Trident Application is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Contact

If you have any questions or feedback, please create an issue.

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
      - Helm Chart
      - Manifest files
      - Kustomize overlays
      - [Argo CD](https://octopus.com/docs/argo-cd) application definitions
  - Windows / Linux Deployments via a package `Trident.Web.zip`
- [Database Deployments](https://octopus.com/docs/deployments/databases) using scripts found in `/src/Trident.Database.DbUp`
  - Generates a delta script to review during the deployment
  - Deploys the database as part of the deployment
- [Feature Flags](https://octopus.com/docs/feature-flags)
  - See `\src\Trident.Web\Core\Configuration\TridentFeatureFlag.cs` for a complete list of feature flags available
- [Security Scanning, SBOM and Attestation verification](https://octopus.com/devops/security/supply-chain-security-with-github-actions-and-octopus-deploy/)
  - Build process
    - Trivy is used to scan for third-party libraries with known vulnerabilities, which is uploaded to GitHub
    - Trivy also builds an SBOM which is packaged and sent to Octopus Deploy
    - A hash is created of all the build artifacts and included in the Attestation
  - Deploy process
    - Use Trivy to scan the SBOM for any new critical vulnerabilities since the artifacts were built
    - Use `gh attestation verify` to verify the attestations of the build artifacts
- [Build information](https://octopus.com/docs/packaging-applications/build-servers/build-information/)
  - The GitHub action publishes build information to Octopus Deploy.
  - All build information is attached to the SBOM and uploaded to Octopus Deploy
  - This allows you to create release notes listing all the commits and issues included in the change
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

Please contribute to the Trident Application! If you would like to contribute, please follow these guidelines:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Push your changes to your forked repository.
5. Submit a pull request.

## License

The Trident SaaS Application is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Contact

If you have any questions or feedback, please create an issue.

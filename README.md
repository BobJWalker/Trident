# Trident
Sample SaaS Application written in .NET

## Introduction

Welcome to the Trident AI Sample SaaS Application! This application is written in .NET and serves as a demonstration of a Software-as-a-Service (SaaS) solution.  It's primary focus is to demonstrate how to build and deploy to AKS and AzureSQL using GitHub Actions and Octopus Deploy.  

## Demo application information

- Very simple application, has a web front end and a database backend
- Dockerfile is already created and supports building both x64 and ARM containers
- The deployment process supports a common feature branch workflow

## Running

To install and run the Trident AI Sample SaaS Application, follow these steps:

1. Clone the repository: `git clone https://github.com/OctopusSolutionsEngineering/Trident.git`
1. Open the project in your preferred .NET IDE.
1. You can run the application locally, or as a container.  
1. You will need to create the database.  
   1. You can do this by running `/src/Trident.Database.DbUp` and providing a connection string in the `/src/Trident.Database.DbUp/properties/launchSettings.json` file. 
   1. OR, you can go to `/src/Trident.Database.DbUp/DeploymentScripts` and running them all in order
1. Once the database is created, you'll need to set the connection string in the environment variable `TRIDENT_CONNECTION_STRING`.

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

- **Default**: Deploys to the `Development` environment only for feature branch work
- **Release**: Deploys to `Test` -> `Staging` -> `Production`.  This is what the main branch uses.

## Contributing

We welcome contributions to the Trident AI Sample SaaS Application! If you would like to contribute, please follow these guidelines:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Push your changes to your forked repository.
5. Submit a pull request.

## License

The Trident Sample SaaS Application is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Contact

If you have any questions or feedback, please create an issue.

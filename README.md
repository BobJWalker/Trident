# Trident
Sample SaaS Application written in .NET

## Introduction

Welcome to the Trident AI Sample SaaS Application! This application is written in .NET and serves as a demonstration of a Software-as-a-Service (SaaS) solution.  It's primary focus is to demonstrate how to build and deploy to AKS and AzureSQL using GitHub Actions and Octopus Deploy.  

## Demo application information

- Very simple application, has a web front end and a database backend
- Dockerfile is already created and supports building both x64 and ARM containers
- The deployment process supports a common feature branch workflow

## Installation

To install and run the Trident AI Sample SaaS Application, follow these steps:

1. Clone the repository: `git clone https://github.com/your-username/Trident_AI.git`
2. Open the project in your preferred .NET IDE.
3. Build the solution.
4. Run the application.

## Forking

If you want to fork this repository, please feel free to do so!  

1. Once you fork the repository you'll need to provide the following secrets for the github actions to properly work
   1. DOCKERHUB_PAT
   2. DOCKERHUB_USERNAME
   3. OCTOPUSSERVERAPIKEY
   4. OCTOPUS_SERVER_URL
1. You can run the application locally, or as a container.  You will need to create a SQL Server database and provide the connection string via the environment variable `TRIDENT_CONNECTION_STRING` 


## Contributing

We welcome contributions to the Trident AI Sample SaaS Application! If you would like to contribute, please follow these guidelines:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Push your changes to your forked repository.
5. Submit a pull request.

## License

The Trident AI Sample SaaS Application is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

## Contact

If you have any questions or feedback, please create an issue.

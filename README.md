# Crisp

Crisp is the web tool designed to speed up your threat modeling process. Crisp offers a user-friendly way to build, manage, and export comprehensive threat model reports named security plans. Tailored for engineering teams and project managers, this tool simplifies the complex process of threat modeling and security analysis. Moreover, Crisp provides important cloud security recommendations, enhancing the overall security posture of your projects.

## Features

- Easy-to-Use Interface: Crisp's intuitive design makes security planning accessible to everyone, from novices to experts.
- Customizable Security Plans: Generate detailed security plan reports in your preferred format, be it Markdown or Microsoft Word.
- Integrated Recommendations: Choose from a wide range of security recommendations and best practices to suit your project's specific needs.
- Seamless Integration: Export your security plans and recommendations directly into Azure DevOps work items or GitHub issues, streamlining your workflow.
- Resource Library: Access a curated selection of resources to inform and enhance your security strategies.

## How to Run

### Prerequisites

Ensure Docker is installed on your system as it is required to run Crisp.

### Running Crisp

1. Pull the Crisp Docker image from Docker Hub:

    To pull the latest version of the Crisp Docker image:

    ```bash
    docker pull anmalkov/crisp
    ```

    To pull a specific version of the Crisp Docker image, replace `1.4` with the desired version number:

    ```bash
    docker pull anmalkov/crisp:1.4
    ```

    Available versions of the Crisp Docker image can be found on the [Crisp Docker Hub page](https://hub.docker.com/r/anmalkov/crisp/tags).

2. Run the Crisp Docker image:

    You can run the Crisp tool with or without data persistence. If you run the Crisp tool without data persistence, all data will be lost when the container is stopped. If you run the Crisp tool with data persistence, all necessary data will be saved in the specified directory on your local machine. This allows you to start the new container without losing any previously saved data.

    To run the Crisp tool without data persistence:

    ```bash
    docker run -p 8080:80 anmalkov/crisp
    ```

    To run the Crisp tool with data persistence and save all data in the `C:\temp\crisp-data` directory on Windows:

    ```bash
    docker run -d -p 8080:80 -v C:\temp\crisp-data:/app/data anmalkov/crisp
    ```

    To run the Crisp tool with data persistence and save all data in the `~/crisp-data` directory on Linux:

    ```bash
    docker run -d -p 8080:80 -v ~/crisp-data:/app/data anmalkov/crisp
    ```

## Contribution

Crisp is an open-source project, and we welcome contributions from the community. Please read our contribution guidelines for more information on how you can contribute.

## Docker Guide

### Introduction

This guide provides a step-by-step introduction to using Docker with the ASP.NET SPA template.

### Prerequisites

* Docker installed on your machine
* ASP.NET SPA template project

### Step 1: Build the Docker Image

1. Open a terminal and navigate to the project directory.
2. Run the command `docker build -t aspnet-spa-template .` to build the Docker image.

### Step 2: Run the Docker Container

1. Run the command `docker run -p 5000:5000 aspnet-spa-template` to start a new container from the image.
2. The application will be available at `http://localhost:5000`.

### Step 3: Configure Environment Variables

1. Create a new file named `.env` in the project directory.
2. Add the following environment variables:

```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=<your_database_connection_string>
```

3. Update the `docker-compose.yml` file to include the environment variables:

```
services:
  aspnet-spa-template:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=${ConnectionStrings__DefaultConnection}
    ports:
      - "5000:5000"
```

### Step 4: Run the Application with Docker Compose

1. Run the command `docker-compose up` to start the application.
2. The application will be available at `http://localhost:5000`.

### Conclusion

This guide has provided a step-by-step introduction to using Docker with the ASP.NET SPA template. By following these steps, you can easily containerize and run your application using Docker.

### Troubleshooting

* Check the Docker logs for any errors.
* Verify that the environment variables are correctly configured.
* Make sure that the Docker image is correctly built and tagged.

### Further Reading

* [Docker Documentation](https://docs.docker.com/)
* [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)

### Contributing

This guide is open-source and welcomes contributions. If you have any suggestions or improvements, please submit a pull request.

### License

This guide is licensed under the MIT License.

### Versioning

This guide follows the Semantic Versioning specification.

### Change Log

All notable changes to this guide will be documented in this file.

### Credits

This guide was created by [Vladyslav Zaiets](https://sarmkadan.com).
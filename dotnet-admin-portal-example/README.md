# dotnet-admin-portal-example

An example application demonstrating how the Admin Portal works with WorkOS and .NET.

## Clone and Install

Clone this repo and install dependencies:

```sh
git clone https://github.com/workos-inc/dotnet-admin-portal-example.git && cd dotnet-admin-portal-example && dotnet build
```

## Configure your environment

1. Create an Organization in the WorkOS Dashboard (you will user the Organization ID later).
2. Grab your API Key from the WorkOS Dashboard.
Set this as an environment variable in the Properties/launchSettings.json file,
labeled `WORKOS_API_KEY`.

## Run the server

```sh
dotnet run
```

Head to `https://localhost:5001` and use your Organization ID to start an Admin Portal session and create an SSO Connection!

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).

# dotnet-mfa-example

An example application demonstrating how MFA works with WorkOS and .NET.

## Clone and Install

Clone this repository and install dependencies:

```sh
git clone https://github.com/workos/dotnet-example-applications.git && cd dotnet-mfa-example && dotnet build
```

## Configure your environment

Note: If you are using Apple M1 chip on Monterrary or higher you will need to change TargetFramework from net 5.0 to 6.0 inside the project file.

1. Grab your API Key and Client ID from the WorkOS Dashboard.
Set these as environment variables in the Properties/launchSettings.json file,
labeled `WORKOS_API_KEY` and `WORKOS_CLIENT_ID`

## Run the server and log in using SSO

```sh
dotnet run
```

Head to `https://localhost:5001` to authenticate!

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).

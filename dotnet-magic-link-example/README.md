# dotnet-magic-link-example

An example application demonstrating how Magic Link works with WorkOS and .NET.

## Clone and Install

Clone this repo and install dependencies:

```sh
git clone https://github.com/workos-inc/dotnet-magic-link-example.git && cd dotnet-magic-link-example && dotnet build
```

## Configure your environment

1. Grab your [API Key](https://dashboard.workos.com/api-keys). 
2. Grab your [Client ID](https://dashboard.workos.com/configuration).
3. Set these as environment variables in the Properties/launchSettings.json file,
labeled `WORKOS_API_KEY` and `WORKOS_CLIENT_ID`.
4. Set your [Default Redirect Link](https://dashboard.workos.com/configuration) to `https://localhost:5001/success`.

## Run the server

```sh
dotnet run
```

Head to `https://localhost:5001` and input an email address to log in using Magic Link!

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).

# dotnet-directory-sync-example

An example application demonstrating how Directory Sync works with WorkOS and .NET.

## Clone and Install

Clone this repo and install dependencies:

```sh
git clone https://github.com/workos-inc/dotnet-directory-sync-example.git && cd dotnet-directory-sync-example && dotnet build
```

## Configure your environment

1. Create a Directory Sync Connection in the WorkOS Dashboard.
2. Grab your API Key and Directory ID (from step 1) from the WorkOS Dashboard.
Set these as environment variables in the Properties/launchSettings.json file,
labeled `WORKOS_API_KEY` and `WORKOS_DIRECTORY_ID`.

## Run the server and log in using SSO

```sh
dotnet run
```

Head to `https://localhost:5001` to view users and groups in your directory!

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).

# dotnet-directory-sync-example

An example application demonstrating how Directory Sync works with WorkOS and .NET.

## Clone and Install

Clone the main repository and install dependencies for the Directory Sync app:

```sh
git clone https://github.com/workos/dotnet-example-applications.git && cd dotnet-directory-sync-example && dotnet build
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

Head to `https://localhost:5001` to to view the home page of the app where you can then select the view for users or groups.

## Test Webhooks

WorkOS sends Webhooks as a way of managing updates to Directory Sync connections. The Webhooks section of the WorkOS Dashboard allows you to send test webhooks to your application. The Test Webhooks section of this application allows you to visualize the validated webhooks directly in this application in real-time. Please review the tutorial here for details on how this can be done locally.

## Need help?

If you get stuck and aren't able to resolve the issue by reading our API reference or tutorials, please reach out to us at support@workos.com and we'll help you out.

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).

# dotnet-sso-example

An example application demonstrating how SSO works with WorkOS and .NET.

## Clone and Install

Clone this repository and install dependencies:

```sh
git clone https://github.com/workos/dotnet-example-applications.git && cd dotnet-sso-example && dotnet build
```

## Configure your environment

1. Grab your API Key and Client ID from the WorkOS Dashboard.
Set these as environment variables in the Properties/launchSettings.json file,
labeled `WORKOS_API_KEY` and `WORKOS_CLIENT_ID`,
2. Create an SSO Connection in the WorkOS Dashboard.
3. Add `https://localhost:5001/Home/Callback` as a Redirect URI in the Configuration section of the Dashboard.
4. Update line 62 of `HomeController.cs` with the Connection domain (or Connection ID).

## Run the server and log in using SSO

```sh
dotnet run
```

Head to `https://localhost:5001` to authenticate!

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).

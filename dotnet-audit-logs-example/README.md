# dotnet-directory-sync-example

An example application demonstrating how Directory Sync works with WorkOS and .NET.

## Clone and Install

Clone the main repository and install dependencies for the Directory Sync app:

```sh
git clone https://github.com/workos/dotnet-example-applications.git && cd dotnet-audit-logs-example && dotnet build
```

## Configure your environment

1. Grab your API Key and Client ID from the WorkOS Dashboard.
Set these as environment variables in the Properties/launchSettings.json file,
labeled `WORKOS_API_KEY` and `WORKOS_CLIENT_ID`.

## Run the server

```sh
dotnet run
```

Head to `https://localhost:5001` to begin the audit log flow!

For more information, see the [WorkOS .NET SDK documentation](https://workos.com/docs/reference/client-libraries).


## Audit Logs setup

Follow the [Audit Logs configuration steps](https://workos.com/docs/audit-logs/emit-an-audit-log-event/sign-in-to-your-workos-dashboard-account-and-configure-audit-log-event-schemas) to set up the following event that is sent with this example:

Action title: "user.organization_deleted" | Target type: "team"

Navigate to the Configuration tab in your WorkOS Dshboard. From there click the Admin Portal tab. Click the Edit Admin Portal Redirect Links button and add "http://localhost:5001" to the "When clicking the back navigation, return users to:" input, then click Save Redirect Links.

To obtain a CSV of the Audit Log events that were sent for the last 30 days, click the "Export Events" tab. This will bring you to a new page where you can download the events. After you have ensured that you have fired off the Organization Deleted event, downloading the events is a 2 step process. First you need to create the report by clicking the "Generate CSV" button. Then click the "Access CSV" button to download a CSV of the Audit Log events for the selected Organization for the past 30 days. You may also adjust the time range using the form inputs.

## Need help?

First, make sure to reference the Audit Logs docs at https://workos.com/docs/audit-logs.

If you get stuck and aren't able to resolve the issue by reading our docs or API reference, you can reach out to us at support@workos.com and we'll lend a hand.

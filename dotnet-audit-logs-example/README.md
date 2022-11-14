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

Follow the [Audit Logs configuration steps](https://workos.com/docs/audit-logs/emit-an-audit-log-event/sign-in-to-your-workos-dashboard-account-and-configure-audit-log-event-schemas) to set up the following 5 events that are sent with this example:

Action title: "user.signed_in" | Target type: "team"
Action title: "user.logged_out" | Target type: "team"
Action title: "user.organization_set" | Target type: "team"
Action title: "user.organization_deleted" | Target type: "team"
Action title: "user.connection_deleted" | Target type: "team"

Next, take note of the Organization ID for the Org which you will be sending the Audit Log events for. This ID gets entered into the splash page of the example application.

Once you enter the Organization ID and submit it, you will be brought to the page where you'll be able to send the audit log events that were just configured. You'll also notice that the action of setting the Organization triggered an Audit Log already. Click the buttons to send the respective events.

To obtain a CSV of the Audit Log events that were sent for the last 30 days, click the "Export Events" button. This will bring you to a new page where you can download the events. Downloading the events is a 2 step process. First you need to create the report by clicking the "Generate CSV" button. Then click the "Access CSV" button to download a CSV of the Audit Log events for the selected Organization for the past 30 days.

## Need help?

First, make sure to reference the Audit Logs docs at https://workos.com/docs/audit-logs.

If you get stuck and aren't able to resolve the issue by reading our docs or API reference, you can reach out to us at support@workos.com and we'll lend a hand.
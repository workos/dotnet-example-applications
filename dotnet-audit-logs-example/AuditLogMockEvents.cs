using System;
using System.Collections.Generic;
using WorkOS;

namespace WorkOS.AuditLogExampleApp.Constants
{
    public class AuditLogMockEvents
    {

        public static AuditLogEvent UserSignedIn = new AuditLogEvent
        {
            Action = "user.signed_in",
            OccurredAt = DateTime.Now,
            Version = 1,
            Actor =
                    new AuditLogEventActor
                    {
                        Id = "user_01GBNJC3MX9ZZJW1FSTF4C5938",
                        Type = "user",
                    },
            Targets =
                    new List<AuditLogEventTarget>() {
                        new AuditLogEventTarget {
                            Id = "team_01GBNJD4MKHVKJGEWK42JNMBGS",
                            Type = "team",
                        },
                    },
            Context =
                    new AuditLogEventContext
                    {
                        Location = "123.123.123.123",
                        UserAgent = "Chrome/104.0.0.0",
                    },
        };

        public static AuditLogEvent UserLoggedOut = new AuditLogEvent
        {
            Action = "user.logged_out",
            OccurredAt = DateTime.Now,
            Version = 1,
            Actor =
                    new AuditLogEventActor
                    {
                        Id = "user_01GBNJC3MX9ZZJW1FSTF4C5938",
                        Type = "user",
                    },
            Targets =
                    new List<AuditLogEventTarget>() {
                        new AuditLogEventTarget {
                            Id = "team_01GBNJD4MKHVKJGEWK42JNMBGS",
                            Type = "team",
                        },
                    },
            Context =
                    new AuditLogEventContext
                    {
                        Location = "123.123.123.123",
                        UserAgent = "Chrome/104.0.0.0",
                    },
        };

        public static AuditLogEvent UserOrganizationDeleted = new AuditLogEvent
        {
            Action = "user.organization_deleted",
            OccurredAt = DateTime.Now,
            Version = 1,
            Actor =
                    new AuditLogEventActor
                    {
                        Id = "user_01GBNJC3MX9ZZJW1FSTF4C5938",
                        Type = "user",
                    },
            Targets =
                    new List<AuditLogEventTarget>() {
                        new AuditLogEventTarget {
                            Id = "team_01GBNJD4MKHVKJGEWK42JNMBGS",
                            Type = "team",
                        },
                    },
            Context =
                    new AuditLogEventContext
                    {
                        Location = "123.123.123.123",
                        UserAgent = "Chrome/104.0.0.0",
                    },
        };

        public static AuditLogEvent UserConnectionDeleted = new AuditLogEvent
        {
            Action = "user.connection_deleted",
            OccurredAt = DateTime.Now,
            Version = 1,
            Actor =
                    new AuditLogEventActor
                    {
                        Id = "user_01GBNJC3MX9ZZJW1FSTF4C5938",
                        Type = "user",
                    },
            Targets =
                    new List<AuditLogEventTarget>() {
                        new AuditLogEventTarget {
                            Id = "team_01GBNJD4MKHVKJGEWK42JNMBGS",
                            Type = "team",
                        },
                    },
            Context =
                    new AuditLogEventContext
                    {
                        Location = "123.123.123.123",
                        UserAgent = "Chrome/104.0.0.0",
                    },
        };
    }
}
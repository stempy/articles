---
title: "Sending notification cards within release pipelines in Azure Devops to Microsoft Teams via a webhook"
categories:
  - Blog
  - DevOps

tags:
  - Azure Devops
  - Microsoft Teams
  - Release Pipeline
---

Here, I will demonstrate how to send nice notifications to Microsoft Teams, from a release pipeline in Azure Devops, notifying that a web application has been deployed, with build/commit, deployment environment details.

I highly recommend to create a task group to contain these set of tasks, and apply variables as needed.

Azure Classic Pipelines.... the same could be done for yaml pipelines.

## Create an incoming webhook in teams

For this, we have to go to a specific channel in Microsoft Teams, and add a connector, look for incoming webhook.

Teams will provide a full webhook URL, copy this URL for later steps.

## Create JSON File Card

Let's create a json file within the pipeline. There is a good marketplace task "File Creator" that will allow just this.... you paste in the content, specify a filename path, and done.

lets create a `notifymessage.json` file, and paste the following content

```json
{
    "@type": "MessageCard",
    "@context": "http://schema.org/extensions",
    "themeColor": "0076D7",
    "summary": "Release Pipeline Notification",
    "sections": [
        {
            "activityTitle": "$(Release.DefinitionName) ($(Release.version))",
            "activitySubtitle": "deployed to $(Release.Environmentname)",
            "activityImage": "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTy5ihdbdBvYPJ6cEoJFkF6ED9I7LnoaP22yg&usqp=CAU",
            "facts": [
                {
                    "name": "Build",
                    "value": "$(Build.DefinitionName) - $(Build.BuildNumber)"
                },
                {
                    "name": "Release",
                    "value": "$(Release.ReleaseName)"
                },
                {
                    "name": "Branch",
                    "value": "$(Build.SourceBranchName)"
                },
                {
                    "name": "Commit",
                    "value": "$(Build.SourceVersion)"
                },
                {
                    "name": "Requested For:",
                    "value": "$(Release.RequestedFor)"
                },
                {
                    "name": "Date & Time:",
                    "value": "$(Release.Deployment.Starttime)"
                }
            ],
            "markdown": true
        }
    ],
    "potentialAction": [
        {
            "@type": "OpenUri",
            "name": "View Release",
            "targets": [
                {
                    "os": "default",
                    "uri": "$(Release.ReleaseWebUrl)"
                }
            ]
        },
        {
            "@type": "OpenUri",
            "name": "View Published URL",
            "targets": [
                {
                    "os": "default",
                    "uri": "$(Release.PublishedUrl)"
                }
            ]
        }
    ]
}
```

## Next up, Send to the webhook

Now, all we need to do is POST the `notifymessage.json` file to the Microsoft Teams webhook, and done.

Add a command line task, and enter the following:

```sh
curl -vX POST $(Release.TeamsWebHook) -d @notifymessage.json --header "Content-Type: application/json"
```

## Variables

As a result of these tasks created, there will be 2 NEW variables defined:

- Release.TeamsWebHook - will contain the webhook provided by Microsoft Teams connector for a channel
- Release.PublishedUrl - will contain the deployed url for the website application


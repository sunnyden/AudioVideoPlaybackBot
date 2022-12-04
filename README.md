AudioVideoPlaybackBot Demo
==========================
This is a demo project refactored from the official [AudioVideoPlaybackBot](https://github.com/microsoftgraph/microsoft-graph-comms-samples) demo. There are some dependency issues in the old project and the project structure of the official demo is a little bit complicated.

I created this simplified project to make local testing and configuring easier in my case.

# Introduction

## About

The AudioVideoPlaybackBot sample guides you through building, deploying and testing an application hosted media bot. This sample demonstrates how a bot can do a video stream and change screen sharing role.

## Getting Started

This section walks you through the process of deploying and testing the sample bot.

### Bot Registration

1. Follow the steps in [Register Calling Bot](https://microsoftgraph.github.io/microsoft-graph-comms-samples/docs/articles/calls/register-calling-bot.html). Save the bot name, bot app id and bot secret for configuration.

1. Add the following Application Permissions to the bot:

    * Calls.AccessMedia.All
    * Calls.Initiate.All
    * Calls.JoinGroupCall.All
    * Calls.JoinGroupCallAsGuest.All
   
1. The permission needs to be consented by tenant admin. Go to "https://login.microsoftonline.com/common/adminconsent?client_id=<app_id>&state=<any_number>&redirect_uri=<any_callback_url>" using tenant admin to sign-in, then consent for the whole tenant.

### Prerequisites

* Install the prerequisites:
    * [Visual Studio 2017+](https://visualstudio.microsoft.com/downloads/)
    * [PowerShell] 7.0+
    * [Mirosoft Azure Subscription] (Can register for a <a href="https://azure.microsoft.com/en-us/free/" target="_blank">free account</a>)
    * [PostMan](https://chrome.google.com/webstore/detail/postman/fhbjgbiflinjbdggehcddcbncdddomop)
* Your computer needs to have a public IP address to test this project.
* A domain that have set DNS record pointing to your computer.
* You need to have a valid certificate for your domain installed on your computer.

### Debugging

* Edit the `config.json` file under the SkypeTeamsMeetingBot folder. Replacing fields with the value of your apps.
* Start debugging directly.

### Test

1. Schedule a Teams meeting with another person.

    ![Test Meeting1](Images/TestMeeting1.png)

1. Copy the Join Microsoft Teams Meeting link. Depending on where you copy this, this may be encoded as a safe link in Outlook.

    ![Test Meeting2](Images/TestMeeting2.png)

    Example, `https://teams.microsoft.com/l/meetup-join/19:cd9ce3da56624fe69c9d7cd026f9126d@thread.skype/1509579179399?context={"Tid":"72f988bf-xxxx-xxxx-xxxx-xxxxxxxxxxxx","Oid":"550fae72-xxxx-xxxx-xxxx-xxxxxxxxxxxx","MessageId":"1536978844957"}`

1. Join the meeting from the Teams client.

1. Interact with your service, _adjusting the service URL appropriately_.
    1. Use Postman to post the following `JSON` payload.

        ##### Request
        ```http
        POST https://bot.contoso.com/joinCall
        Content-Type: application/json
        {
            "JoinURL": "https://teams.microsoft.com/l/meetup-join/...",
        }
        ```

        ##### Response
        The guid "491f0500-401f-4f11-8af4-2eff4c0a0643" in the response will be your call id. Use your call id for the next request.
        ```json
        {
            "legId": "491f0500-401f-4f11-8af4-2eff4c0a0643",
            "scenarioId": "98ca8eab-8c03-4b7d-a468-15b37c0b648e",
            "call": "https://bot.contoso.com:10100/calls/491f0500-401f-4f11-8af4-2eff4c0a0643",
            "logs": "https://bot.contoso.com:10100/logs/491f0500-401f-4f11-8af4-2eff4c0a0643",
            "changeScreenSharingRole": "https://bot.contoso.com:10100/calls/491f0500-401f-4f11-8af4-2eff4c0a0643/changeRole"
        }
        ```

    1. After the bot joins the meeting. The bot will start playing a video. Change the bot's screen sharing role by `POST` changeRole request. Replace the call id 491f0500-401f-4f11-8af4-2eff4c0a0643 below with your call id from the first response.

        ##### Request
        ```http
        POST https://bot.contoso.com/calls/491f0500-401f-4f11-8af4-2eff4c0a0643/changeRole
        Content-Type: application/json
        {
            "role": "viewer"
        }
        ```
        You can play around with the bot by switching the screensharing role from "viewer" to "sharer" or from "sharer" to "viewer"

    1. Get diagnostics data from the bot. Open the links in a browser for auto-refresh. Replace the call id 491f0500-401f-4f11-8af4-2eff4c0a0643 below with your call id from the first response.
       * Active calls: https://bot.contoso.com/calls
       * Service logs: https://bot.contoso.com/logs

    1. Terminating the call through `DELETE`. Replace the call id 491f0500-401f-4f11-8af4-2eff4c0a0643 below with your call id from the first response.

        ##### Request
        ```http
        DELETE https://bot.contoso.com/calls/491f0500-401f-4f11-8af4-2eff4c0a0643
        ```
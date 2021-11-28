# Livekit .NET Server SDK

C# Server SDK for the [Livekit](https://livekit.io) project (https://github.com/livekit).

Only works with custom livekit build, using an extra grpc service (currently as forked project: https://github.com/sze-plusplusplus/livekit-server)

Docker image for the custom build: `docker pull ghcr.io/bencebaranyai/livekit-server:latest`

| Protocol | SDK   |
| -------- | ----- |
| 0.9.4    | 0.1.1 |
| 0.10.2   | 0.1.2 |

> Protocol: https://github.com/livekit/protocol

---

## Install

Package: https://www.nuget.org/packages/Livekit.Client/

```
Install-Package Livekit.Client -Version 0.1.2
```

or

```
dotnet add package Livekit.Client --version 0.1.2
```

## Usage

Similar to JS SDK (https://github.com/livekit/server-sdk-js)

### Example app

[Livekit.Client.Example/](Livekit.Client.Example/)

```bash
cd Livekit.Client.Example/
cp appsettings.example.json appsettings.json
docker-compose up
```

A livekit server and an example container will be started.
Example app has a webhook endpoint, write out the incoming requests, will create a new join token, list the rooms periodically

The displayed token can be used to join on the example page of livekit server: https://example.livekit.io (LiveKit URL: default [ws://localhost:7880])

### Creating Access Tokens

```c#
var ac = new AccessToken("<key>", "<secret>", new Grant()
            {
                video = new VideoGrant()
                {
                    roomJoin = true,
                    room = "Room Name"
                }
            }, "Username");
System.Console.WriteLine(ac.GetToken());
System.Console.WriteLine(ac.GetAsHeader());
System.Console.WriteLine(ac.VerifyToken("<webhook token to verify>")); // returns the "sha256" claim if otherwise the token is valid
```

### Room Service Client

```c#
// host: "http://<hostname>:<grpc_port_of_livekit>"
var room = new RoomClient("<host>", "<key>", "<secret>");

var room = room.CreateRoom("TestRoom");
System.Console.WriteLine(r.Name); // Properties can be accessed

var users = room.ListParticipants("TestRoom");
...
```

### Recording Service Client

> Requires Redis and recording instance of livekit

```c#
var rs = new RecordingClient("<host>", "<key>", "<secret>");
var id = rc.StartRecording(new Proto.RecordingTemplate(){
    RoomName = "TestRoom"
});
rc.EndRecording(id);
```

### Receive webhook requests

> The SDK does not contains a webserver/webservice. Simple example is added to the Example project.

```c#
... // "context" is a HttpContext variable
var receiver = new WebhookReceiver("<key>", "<secret>");
var jwt = context.Request.Headers[WebhookReceiver.AuthHeader];
var request = receiver.Receive("body string from context...", jwt); // skipAuth can be toggled
// if the checksum does not match, Receive will throw an Exception

if (r.Event == WebhookEvents.PARTICIPANT_JOINED)
{
    System.Console.WriteLine("{0} entered the room!", r.Participant.Identity);
}
...
```

## Tested

- Ubuntu 20.04/dotnet 5.0 - Livekit server 0.13.0

---

## License

Apache license 2.0

See [LICENSE](LICENSE)

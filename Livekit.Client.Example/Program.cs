using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Livekit.Client.Example
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
            var host = config.GetValue<string>("host");
            var key = config.GetValue<string>("key");
            var secret = config.GetValue<string>("secret");

            var ac = new AccessToken(key, secret, new Grant()
            {
                video = new VideoGrant()
                {
                    roomJoin = true,
                    room = "TestRoom"
                }
            }, "User");
            System.Console.WriteLine(ac.GetToken());

            var room = new RoomClient(host, key, secret);

            var r = room.CreateRoom("TestRoom");
            System.Console.WriteLine(r.Name);
            System.Console.WriteLine(r.Sid);

            writeOut(room);

            CreateHostBuilder(args).Build().Run();

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            _quitEvent.WaitOne();
        }

        static async void writeOut(RoomClient room)
        {
            while (true)
            {
                System.Console.WriteLine("Rooms:");
                foreach (var current in room.ListRooms())
                {
                    var p = room.ListParticipants(current.Name);
                    System.Console.WriteLine("\t" + current.Name + ": " + p.Count);
                }

                await System.Threading.Tasks.Task.Delay(3000);
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
            var webhook = config.GetSection("webhook");
            var key = webhook.GetValue<string>("key");
            var secret = webhook.GetValue<string>("secret");
            app.Run(async (context) =>
            {
                var jwt = context.Request.Headers[WebhookReceiver.AuthHeader];
                context.Request.EnableBuffering();
                var body = "";

                using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                    body = await reader.ReadToEndAsync();

                var rec = new WebhookReceiver(key, secret);

                var r = rec.Receive(body, jwt);
                System.Console.WriteLine(r);

                if (r.Event == WebhookEvents.PARTICIPANT_JOINED)
                {
                    System.Console.WriteLine("{0} entered the room!", r.Participant.Identity);
                }

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{}");
            });
        }
    }
}

// 必要な参照:
// - CeVIO.Talk.RemoteService2.dll
// - System.Net.Http
// - System.Web.Http.SelfHost (NuGet: Microsoft.AspNet.WebApi.SelfHost)

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using CeVIO.Talk.RemoteService2;

namespace CevioSelfHost
{
    public class SpeakRequest
    {
        public string Text { get; set; }
        public string Language { get; set; } = "ja"; // "ja" or "en"
        public string Cast { get; set; } // optional override
        public uint? Volume { get; set; }
        public uint? Speed { get; set; }
        public uint? Tone { get; set; }
        public uint? Alpha { get; set; }
        public uint? ToneScale { get; set; }
    }

    public class CevioController : ApiController
    {
        [HttpPost]
        [Route("speak")]
        public HttpResponseMessage Post([FromBody] SpeakRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Text is required");

            string tempFile = Path.GetTempFileName() + ".wav";

            var talker = new Talker2();

            // キャスト設定
            if (!string.IsNullOrEmpty(request.Cast))
            {
                talker.Cast = request.Cast;
            }
            else
            {
                talker.Cast = request.Language == "en" ? "弦巻マキ (英)" : "弦巻マキ (日)";
            }

            if (request.Volume.HasValue) talker.Volume = request.Volume.Value;
            if (request.Speed.HasValue) talker.Speed = request.Speed.Value;
            if (request.Tone.HasValue) talker.Tone = request.Tone.Value;
            if (request.Alpha.HasValue) talker.Alpha = request.Alpha.Value;
            if (request.ToneScale.HasValue) talker.ToneScale = request.ToneScale.Value;

            if (!talker.OutputWaveToFile(request.Text, tempFile))
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to synthesize");

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(File.ReadAllBytes(tempFile))
            };
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "voice.wav"
            };

            File.Delete(tempFile);
            return result;
        }
    }

    class Program
    {
        static string GetLocalIPAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a.Address))
                .Select(a => a.Address.ToString())
                .FirstOrDefault() ?? "127.0.0.1";
        }

        static void Main()
        {
            if (!ServiceControl2.IsHostStarted)
                ServiceControl2.StartHost(false);

            string ip = GetLocalIPAddress();
            string baseAddress = $"http://{ip}:5000/";

            var config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute(
                name: "API Default",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("CeVIO Web API running at " + baseAddress);
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }

            ServiceControl2.CloseHost();
        }
    }
}
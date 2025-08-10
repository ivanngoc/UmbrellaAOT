using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Umbrella.Server.Application.Pipeline
{
    public class TlsNode(IConfiguration configuration, ILogger<TlsNode> logger) : PipelineNode
    {
        private SslStream? stream;
        public override async Task PrepareAsync(PipelineRunner pipeline, Session session, CancellationToken ct = default)
        {
            try
            {
                var streamIn = pipeline.GetStream();
                var tlsCrtStr = configuration["TlsCrt"];
                var tlsKeyStr = configuration["TlsKey"];
                ArgumentNullException.ThrowIfNullOrWhiteSpace(tlsCrtStr);
                ArgumentNullException.ThrowIfNullOrWhiteSpace(tlsKeyStr);
                var certificate = LoadCertificateFromPem(tlsCrtStr, tlsKeyStr);
                await base.PrepareAsync(pipeline, session, ct);
                stream = new SslStream(streamIn);
                var opt = new SslServerAuthenticationOptions()
                {
                    ServerCertificate = certificate,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    ClientCertificateRequired = false
                };
                await stream.AuthenticateAsServerAsync(opt);
                pipeline.SetStream(stream);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw new AggregateException(ex);
            }
        }
        X509Certificate2 LoadCertificateFromPem(string certFile, string keyFile)
        {
#if WINDOWS
            var fiPfx = new FileInfo(certFile);
            var cert = X509CertificateLoader.LoadPkcs12FromFile(
       path: fiPfx.FullName,
       password: "",  // empty if your PFX has no password
       keyStorageFlags: X509KeyStorageFlags.PersistKeySet
                      | X509KeyStorageFlags.UserKeySet
                      | X509KeyStorageFlags.Exportable,
       loaderLimits: null   // null == Defaults :contentReference[oaicite:0]{index=0}
   );
            return cert;
#else
            var fiCert = new FileInfo(certFile);
            var fiKey = new FileInfo(keyFile);
            var cert = X509Certificate2.CreateFromPemFile(fiCert.FullName, fiKey.FullName);
            return cert;
#endif
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlockForge.Red
{
    /// <summary>Utilidades para jugar por internet: IP pública y tipo de dirección.</summary>
    public static class Internet
    {
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };

        /// <summary>Dirección IP pública de este equipo (la que ven los demás en internet), o null.</summary>
        public static async Task<string> IpPublica()
        {
            string[] servicios = { "https://api.ipify.org", "https://ipv4.icanhazip.com", "https://ifconfig.me/ip" };
            foreach (string servicio in servicios)
            {
                try
                {
                    string ip = (await http.GetStringAsync(servicio)).Trim();
                    IPAddress direccion;
                    if (IPAddress.TryParse(ip, out direccion) && direccion.AddressFamily == AddressFamily.InterNetwork) return ip;
                }
                catch (Exception)
                {
                    // Se prueba el siguiente servicio.
                }
            }
            return null;
        }

        /// <summary>Direcciones privadas o de CGNAT (100.64.0.0/10): no se puede llegar a ellas desde internet.</summary>
        public static bool EsPrivada(string ip)
        {
            IPAddress d;
            if (!IPAddress.TryParse(ip ?? "", out d) || d.AddressFamily != AddressFamily.InterNetwork) return false;
            byte[] b = d.GetAddressBytes();
            return b[0] == 10 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168) ||
                   (b[0] == 100 && b[1] >= 64 && b[1] <= 127) || b[0] == 127 || (b[0] == 169 && b[1] == 254);
        }
    }

    /// <summary>
    /// Apertura automática de puertos en el router con UPnP (Internet Gateway Device):
    /// así los amigos pueden entrar por internet sin configurar el router a mano.
    /// </summary>
    public sealed class Upnp
    {
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        private string urlControl, servicio;

        /// <summary>Dirección del equipo en la red local, la que recibe el reenvío del router.</summary>
        public string IpLocal { get; private set; }
        public string NombreRouter { get; private set; }

        /// <summary>Busca el router con SSDP. Devuelve null si no hay un router con UPnP activado.</summary>
        public static async Task<Upnp> Buscar(int milisegundos = 3000)
        {
            List<string> ubicaciones = await Ssdp(milisegundos);
            foreach (string ubicacion in ubicaciones)
            {
                try
                {
                    Upnp u = await LeerDescripcion(ubicacion);
                    if (u != null) return u;
                }
                catch (Exception)
                {
                    // Otro dispositivo UPnP que no es el router: se prueba el siguiente.
                }
            }
            return null;
        }

        private static async Task<List<string>> Ssdp(int milisegundos)
        {
            List<string> ubicaciones = new List<string>();
            string[] tipos =
            {
                "urn:schemas-upnp-org:device:InternetGatewayDevice:1",
                "urn:schemas-upnp-org:device:InternetGatewayDevice:2",
                "urn:schemas-upnp-org:service:WANIPConnection:1"
            };
            using (UdpClient udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
            {
                IPEndPoint grupo = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
                foreach (string tipo in tipos)
                {
                    byte[] consulta = Encoding.ASCII.GetBytes("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: " + tipo + "\r\n\r\n");
                    await udp.SendAsync(consulta, consulta.Length, grupo);
                }
                DateTime limite = DateTime.Now.AddMilliseconds(milisegundos);
                while (DateTime.Now < limite)
                {
                    Task<UdpReceiveResult> recibir = udp.ReceiveAsync();
                    Task gana = await Task.WhenAny(recibir, Task.Delay(limite - DateTime.Now));
                    if (gana != recibir) break;
                    string respuesta = Encoding.ASCII.GetString(recibir.Result.Buffer);
                    Match m = Regex.Match(respuesta, @"^LOCATION:\s*(\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (m.Success && !ubicaciones.Contains(m.Groups[1].Value)) ubicaciones.Add(m.Groups[1].Value);
                }
            }
            return ubicaciones;
        }

        private static async Task<Upnp> LeerDescripcion(string ubicacion)
        {
            XDocument xml = XDocument.Parse(await http.GetStringAsync(ubicacion));
            XNamespace ns = xml.Root.Name.Namespace;
            Uri baseUri = new Uri(ubicacion);
            XElement urlBase = xml.Root.Element(ns + "URLBase");
            if (urlBase != null && Uri.IsWellFormedUriString(urlBase.Value, UriKind.Absolute)) baseUri = new Uri(urlBase.Value);

            foreach (XElement s in xml.Descendants(ns + "service"))
            {
                string tipo = (string)s.Element(ns + "serviceType") ?? "";
                if (!tipo.Contains("WANIPConnection") && !tipo.Contains("WANPPPConnection")) continue;
                string control = (string)s.Element(ns + "controlURL");
                if (string.IsNullOrEmpty(control)) continue;

                // La IP local es la de la interfaz que llega al router.
                string ipLocal;
                using (UdpClient u = new UdpClient())
                {
                    u.Connect(baseUri.Host, baseUri.Port);
                    ipLocal = ((IPEndPoint)u.Client.LocalEndPoint).Address.ToString();
                }
                XElement nombre = xml.Descendants(ns + "friendlyName").FirstOrDefault();
                return new Upnp
                {
                    urlControl = new Uri(baseUri, control).ToString(),
                    servicio = tipo,
                    IpLocal = ipLocal,
                    NombreRouter = nombre != null ? nombre.Value : baseUri.Host
                };
            }
            return null;
        }

        private async Task<string> Soap(string accion, string argumentos)
        {
            string cuerpo = "<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                            "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body><u:" + accion + " xmlns:u=\"" + servicio + "\">" +
                            argumentos + "</u:" + accion + "></s:Body></s:Envelope>";
            using (HttpRequestMessage pedido = new HttpRequestMessage(HttpMethod.Post, urlControl))
            {
                pedido.Content = new StringContent(cuerpo, Encoding.UTF8, "text/xml");
                pedido.Headers.TryAddWithoutValidation("SOAPAction", "\"" + servicio + "#" + accion + "\"");
                HttpResponseMessage r = await http.SendAsync(pedido);
                string texto = await r.Content.ReadAsStringAsync();
                if (!r.IsSuccessStatusCode)
                {
                    Match error = Regex.Match(texto, "<errorDescription>([^<]*)</errorDescription>");
                    throw new InvalidOperationException(error.Success ? error.Groups[1].Value : "el router respondió " + (int)r.StatusCode);
                }
                return texto;
            }
        }

        /// <summary>Dirección pública según el router (si es privada, hay CGNAT o doble router).</summary>
        public async Task<string> IpExterna()
        {
            try
            {
                Match m = Regex.Match(await Soap("GetExternalIPAddress", ""), "<NewExternalIPAddress>([^<]*)</NewExternalIPAddress>");
                return m.Success ? m.Groups[1].Value : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Pide al router que reenvíe el puerto TCP a este equipo. Devuelve el error o null si funcionó.</summary>
        public async Task<string> AbrirPuerto(int puerto, string descripcion)
        {
            string error = null;
            foreach (int duracion in new[] { 0, 7200 })
            {
                try
                {
                    await Soap("AddPortMapping",
                        "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + puerto + "</NewExternalPort><NewProtocol>TCP</NewProtocol>" +
                        "<NewInternalPort>" + puerto + "</NewInternalPort><NewInternalClient>" + IpLocal + "</NewInternalClient>" +
                        "<NewEnabled>1</NewEnabled><NewPortMappingDescription>" + descripcion + "</NewPortMappingDescription>" +
                        "<NewLeaseDuration>" + duracion + "</NewLeaseDuration>");
                    return null;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }
            return error;
        }

        public async Task CerrarPuerto(int puerto)
        {
            try
            {
                await Soap("DeletePortMapping",
                    "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + puerto + "</NewExternalPort><NewProtocol>TCP</NewProtocol>");
            }
            catch (Exception)
            {
                // Si el router ya no tiene el reenvío, no hay nada que cerrar.
            }
        }
    }
}

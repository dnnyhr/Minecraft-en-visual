using System.IO;
using System.IO.Compression;

namespace BlockForge.Helpers
{
    /// <summary>Compresión GZip de las celdas del mundo (para la base de datos y para la red).</summary>
    public static class Compresion
    {
        public static byte[] Comprimir(byte[] datos)
        {
            using (MemoryStream salida = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(salida, CompressionLevel.Fastest))
                {
                    gzip.Write(datos, 0, datos.Length);
                }
                return salida.ToArray();
            }
        }

        public static byte[] Descomprimir(byte[] datos)
        {
            using (MemoryStream entrada = new MemoryStream(datos))
            using (GZipStream gzip = new GZipStream(entrada, CompressionMode.Decompress))
            using (MemoryStream salida = new MemoryStream())
            {
                gzip.CopyTo(salida);
                return salida.ToArray();
            }
        }
    }
}

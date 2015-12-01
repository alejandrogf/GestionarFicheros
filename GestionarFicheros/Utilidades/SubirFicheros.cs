using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GestionarFicheros.Utilidades
{
    public class SubirFicheros
    {
        public static String GuardarFicheroDisco(HttpPostedFileBase fichero, HttpServerUtilityBase server)
        {
            //Se crea con un GUID
            var id = Guid.NewGuid();
            //Otra opción es con DateTime, que añade la firma horaria
            //var id2 = DateTime.Now.Ticks;
            String nombrefinal = null;
            //Comprobar que existe y contiene algo
            if (fichero != null && fichero.ContentLength > 0)
            {
                var extension = fichero.FileName.Substring(fichero.FileName.LastIndexOf(".") + 1);
                nombrefinal = $"{id}.{extension}";

                fichero.SaveAs(server.MapPath("/ficherossubidos") + "/" + nombrefinal);
            }

            return nombrefinal;
        }

        public static byte[] ToBinario(HttpPostedFileBase fichero)
        {
            byte[] data = null;
            if (fichero != null && fichero.ContentLength > 0)
            {
                using (var stream = fichero.InputStream)
                {
                    var memoryStream = stream as MemoryStream;

                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                    }

                    data = memoryStream.ToArray();
                }
            }
            return data;
        }
    }
}

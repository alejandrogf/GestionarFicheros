using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GestionarFicheros.Utilidades
{
    public class AzureStorage
    {
        private CloudStorageAccount _cuenta;
        private String _contenedor;

        //Adicionalmente hay que añadire tres parametros al webconfig
        public AzureStorage(String cuenta, String clave, String contenedor = null)
        {
            StorageCredentials credenciales=new StorageCredentials(cuenta, clave);
            _cuenta=new CloudStorageAccount(credenciales, true);
            _contenedor = contenedor;
        }

        private void ComprobarContenedor(String contenedor = null)
        {
            if (contenedor!=null)
            {
                _contenedor = contenedor;
            }

            var cliente = _cuenta.CreateCloudBlobClient();
            var cont = cliente.GetContainerReference(_contenedor);
            cont.CreateIfNotExists();
        }

        public void SubirFichero(Stream fichero, String nombre, String contenedor = null)
        {
            ComprobarContenedor(contenedor);

            var cliente = _cuenta.CreateCloudBlobClient();
            var cont = cliente.GetContainerReference(_contenedor);
            var blob = cont.GetBlockBlobReference(nombre);
            blob.UploadFromStream(fichero);
            fichero.Close();
        }

        //Recuperar fichero desde el contenedor

        public byte[] RecuperarFicheroContenedor(String nombre, String contenedor)
        {
            ComprobarContenedor(contenedor);
            var cliente = _cuenta.CreateCloudBlobClient();
            var cont = cliente.GetContainerReference(_contenedor);
            var blob = cont.GetBlockBlobReference(nombre);

            //Se obtienen los atributos.
            blob.FetchAttributes();
            //Se recupera la longitud del fichero
            var longitud = blob.Properties.Length;
            //Crear un array de bytes del tamaño del fichero origen
            var destino=new byte[longitud];
            //0 es la posición desde donde empezar a copiar
            blob.DownloadToByteArray(destino, 0);

            return destino;


        }
    }
}

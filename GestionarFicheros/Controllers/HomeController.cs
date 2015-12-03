using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using GestionarFicheros.Models;
using GestionarFicheros.Utilidades;

namespace GestionarFicheros.Controllers
{
    public class HomeController : Controller
    {
        //Para crear un desplegable:
        //1.- Se crea una clase en Models, sobre los campos que se quiere hacer la lista
        //2.-Este código, creando la lista
        //3.-Añadir codigo:ViewBag.TipoFichero=new SelectList(tipos, "id", "nombre");
        //MUY MUY importante que el nombre del viewbag sea exáctamente igual que el nombre de
        //la base de datos del que queremos hacer el desplegable
        //4.-En la vista donde se quiere mostrar, hay que quitar algunas lineas para mostrarlo.
        //En este caso se quita:
        //@Html.EditorFor(model => model.TipoFichero, new { htmlAttributes = new { @class = "form-control" } })
        //Y se añade:
        //@Html.DropDownList("TipoFichero",String.Empty)
        private List<TipoFichero> tipos = new List<TipoFichero>()
        {
            new TipoFichero() {id = 1, nombre = "Fotografía"},
            new TipoFichero() {id = 2, nombre = "Otros"}
        };

        FicherosEntities db = new FicherosEntities();
        // GET: Home
        public ActionResult Index()
        {
            var data = db.Fichero;
            return View(data);
        }

        public ActionResult Subida(int almacen = 1)
        {
            //Almacen= donde se guardan los objetos
            //1.- fichero local 
            //2.- base 64
            //3.- binary
            //4.- nube
            var tipoAlmacen = "";
            if (almacen == 1)
                tipoAlmacen = "interno";
            if (almacen == 2)
                tipoAlmacen = "base64";
            if (almacen == 3)
                tipoAlmacen = "binario";
            if (almacen == 4)
                tipoAlmacen = "azure";

            ViewBag.almacen = tipoAlmacen;
            ViewBag.TipoFichero = new SelectList(tipos, "id", "nombre");
            return View(new Fichero());
        }

        public ActionResult GetBase64Azure(String nombre)
        {
            var cuentaAz = ConfigurationManager.AppSettings["cuentaAzureStorage"];
            var claveAz = ConfigurationManager.AppSettings["claveAzureStorage"];
            var contAz = ConfigurationManager.AppSettings["contenedorAzureStorage"];

            var storage = new AzureStorage(cuentaAz, claveAz, contAz);
            var data = storage.RecuperarFicheroContenedor(nombre, contAz);
            var f = new FicheroBase64() { Contenido = Convert.ToBase64String(data) };
            return View(f);
        }


        public FileResult DownloadFile(int id, int tipo = 0)
        {
            byte[] fichero = new byte[] { };
            var f = db.Fichero.Find(id);
            if (tipo == 0)
            {
                fichero = Convert.FromBase64String(f.Datos);
            }
            else if (tipo==1)
            {
                fichero = f.DatosBinarios;
            }
            else if (tipo==2)
            {
                var cu = ConfigurationManager.AppSettings["cuentaAzureStorage"];
                var cl = ConfigurationManager.AppSettings["claveAzureStorage"];
                var co = ConfigurationManager.AppSettings["contenedorAzureStorage"];

                var sto = new AzureStorage(cu, cl, co);

                fichero = sto.RecuperarFicheroContenedor(f.Datos, co);
            }
            return File(fichero, MediaTypeNames.Application.Octet, f.Nombre);
        }
        

        public FileResult DownloadFileB64(int id, int tipo = 0)
        {
            byte[] fichero = new byte[] {};
            var busqueda = db.Fichero.Find(id);
            if (tipo == 0)
            {
                fichero = Convert.FromBase64String(busqueda.Datos);
            }
            else if(tipo==1)
            {
                fichero = busqueda.DatosBinarios;
            }
            else if (tipo==2)
            {
                var cuentaAz = ConfigurationManager.AppSettings["cuentaAzureStorage"];
                var claveAz = ConfigurationManager.AppSettings["claveAzureStorage"];
                var contAz = ConfigurationManager.AppSettings["contenedorAzureStorage"];

                var azure = new AzureStorage(cuentaAz, claveAz, contAz);

                fichero = azure.RecuperarFicheroContenedor(busqueda.Datos, contAz);
            }
            return File(fichero, MediaTypeNames.Application.Octet, busqueda.Nombre);
        }

        [HttpPost]
        //HttpPostedFileBase: Envia información del fichero 
        //(nombre, encoding, tamaño, string transformado, etc)
        public ActionResult Subida(Fichero model, HttpPostedFileBase fichero)
        {
            if (model.Tipo == "interno")
            {
                var nombreFichero = SubirFicheros.GuardarFicheroDisco(fichero, Server);
                if (nombreFichero != null)
                {
                    model.Datos = nombreFichero;
                    db.Fichero.Add(model);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            else if (model.Tipo == "base64")
            {
                var data = SubirFicheros.ToBinario(fichero);
                if (data != null)
                {
                    model.Datos = Convert.ToBase64String(data);
                    model.Nombre = fichero.FileName;
                    db.Fichero.Add(model);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            else if (model.Tipo == "binario")
            {
                var datab = SubirFicheros.ToBinario(fichero);
                if (datab != null)
                {
                    model.Datos = "";
                    model.DatosBinarios = datab;
                    model.Nombre = fichero.FileName;
                    db.Fichero.Add(model);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            else if (model.Tipo == "azure")
            {
                var cuentaAz = ConfigurationManager.AppSettings["cuentaAzureStorage"];
                var claveAz = ConfigurationManager.AppSettings["claveAzureStorage"];
                var contAz = ConfigurationManager.AppSettings["contenedorAzureStorage"];

                var azure=new AzureStorage(cuentaAz,claveAz,contAz);

                var guid = Guid.NewGuid();
                var extension = fichero.FileName.Substring(fichero.FileName.LastIndexOf("."));

                azure.SubirFichero(fichero.InputStream,guid+extension);

                model.Datos = guid + extension;
                model.Nombre = fichero.FileName;

                db.Fichero.Add(model);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return RedirectToAction("Index");
        }
    }
}
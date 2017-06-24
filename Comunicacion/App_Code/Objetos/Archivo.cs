using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Clase Contenedora de la información de los archivos
/// </summary>
[DataContract]
public class Archivo
{
    [DataMember]
    public string Nombre { get; set; }

    [DataMember]
    public long LongitudArchivo { get; set; }

    [DataMember]
    public string ipFuente { get; set; }

}
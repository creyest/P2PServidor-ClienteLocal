using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
/// <summary>
/// Summary description for Lista
/// </summary>
public static class Lista
{
    public static ConcurrentBag<Archivo> archivos = new ConcurrentBag<Archivo>();
}
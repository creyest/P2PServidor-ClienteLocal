using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class Service : IService
{
	public string GetData(int value)
	{
		return string.Format("You entered: {0}", value);
	}

	public CompositeType GetDataUsingDataContract(CompositeType composite)
	{
		if (composite == null)
		{
			throw new ArgumentNullException("composite");
		}
		if (composite.BoolValue)
		{
			composite.StringValue += "Suffix";
		}
		return composite;
	}

    public IList<Archivo> GetLista()
    {
        IList<Archivo> aux = Lista.archivos.ToList();
        return aux;
    }

    public bool AddArchivo(string Nombre, string ip, long longitud)
    {
        var a = new Archivo() {
            Nombre = Nombre,
            ipFuente = ip,
            LongitudArchivo = longitud
        };
        try
        {
            Lista.archivos.Add(a);
            return true;
        }catch(Exception e)
        {
            return false;
        }
    }

    public void CerrarPeer(string ip)
    {
        List<Archivo> aux = Lista.archivos.Where(x => x.ipFuente != ip).ToList();
        Lista.archivos = new ConcurrentBag<Archivo>();
        foreach (var item in aux)
        {
            Lista.archivos.Add(item);
        }
        //Lista.archivos = (ConcurrentBag<Archivo>)aux;
        //Lista.archivos = aux;
    }
}

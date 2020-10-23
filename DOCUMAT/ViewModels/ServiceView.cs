using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.ViewModels
{
    public class ServiceView : IView<ServiceView,Models.Service>
    {
        public Models.Service Service { get; set; }
        public int NombreVersement { get; private set; }
        public int NombreRegistre { get; private set; }
        public int NumeroOrdre { get; set; }
        public DocumatContext context { get ; set ; }

        public static List<ServiceView> GetServiceViewsList(List<Models.Service> services)
        {
            List<ServiceView> serviceViews = new List<ServiceView>();
            foreach (Models.Service serv in services)
            {
                ServiceView servV = new ServiceView();
                servV.Service = serv;
                serviceViews.Add(servV);
            }
            return serviceViews;
        }


        public ServiceView()
        {
            Service = new Service();
            context = new DocumatContext();
        }

        public void Add()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ServiceView GetView(int Id)
        {
            List<ServiceView> ServiceViews = this.GetViewsList();
             return ServiceViews.FirstOrDefault(sv => sv.Service.ServiceID == Id);
        }

        public List<ServiceView> GetViewsList()
        {
            //List<Models.Service> services = context.Service.ToList();
            //List<ServiceView> ServiceViews = new List<ServiceView>();
            //foreach (Models.Service serv in services)
            //{
            //    ServiceView servV = new ServiceView();
            //    servV.Service = serv;

            //    servV.NombreVersement = 0;
            //    servV.NombreRegistre = 0;

            //    ServiceViews.Add(servV);
            //}
            //return ServiceViews;
            throw new NotImplementedException();
        }
        public List<ServiceView> GetViewsList(int IdRegion)
        {
            List<Service> services = context.Service.Where(s => s.RegionID == IdRegion).ToList();
            List<ServiceView> ServiceViews = new List<ServiceView>();
            foreach (Service service in services)
            {
                ServiceView serviceView = new ServiceView();
                serviceView.Service = service;

                // Devra être implementé dans les statistiques
                serviceView.NombreVersement = 0;
                serviceView.NombreRegistre = 0;

                ServiceViews.Add(serviceView);
            }
            return ServiceViews;
        }

        public bool Update(Service UpElement)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Obtien le nom du chef de service du service courant rataché à la serviceView
        /// Le service doit être instancier 
        /// </summary>
        public Agent getChefService()
        {
            throw new NotImplementedException();
            //return ChefService = Service.Agents.FirstOrDefault(a => a.ServiceID == Service.ServiceID && a.Type.Contains("CHEF DE SERVICE"));
        }

        bool IView<ServiceView, Service>.GetView(int Id)
        {
            throw new NotImplementedException();
        }
    }
}

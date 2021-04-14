using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.ViewModels
{
    class RegionView : IView<RegionView, Region>
    {
        public int NumeroOrdre { get; set; }

        public Region Region { get; set; }

        public RegionView()
        {
            Region = new Region();
        }

        public void Add()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public bool GetView(int Id)
        {
            throw new NotImplementedException();
        }

        public List<RegionView> GetViewsList()
        {
            using (var ct = new DocumatContext())
            {
                List<Region> regions = ct.Region.ToList();
                List<RegionView> regionViews = new List<RegionView>();

                foreach (var region in regions)
                {
                    RegionView regionView = new RegionView();
                    regionView.Region = region;
                    regionViews.Add(regionView);
                }

                return regionViews; 
            }
        }

        public bool Update(Region UpElement)
        {
            throw new NotImplementedException();
        }
    }
}

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
        public DocumatContext context { get; set; }

        public Region Region { get; set; }

        public RegionView()
        {
            context = new DocumatContext();
            Region = new Region();
        }

        public void Add()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public bool GetView(int Id)
        {
            throw new NotImplementedException();
        }

        public List<RegionView> GetViewsList()
        {
            List<Region> regions = context.Region.ToList();
            List<RegionView> regionViews = new List<RegionView>();

            foreach(var region in regions)
            {
                RegionView regionView = new RegionView();
                regionView.Region = region;
                regionViews.Add(regionView);
            }

            return regionViews;
        }

        public bool Update(Region UpElement)
        {
            throw new NotImplementedException();
        }
    }
}

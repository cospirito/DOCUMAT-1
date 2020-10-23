using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.ViewModels
{
    interface IView <MV,M> : IDisposable
    {
        int NumeroOrdre { get; set; }
        Models.DocumatContext context { get; set; }

        void Add();

        bool GetView(int Id);

        bool Update(M UpElement);

        List<MV> GetViewsList();
    }
}

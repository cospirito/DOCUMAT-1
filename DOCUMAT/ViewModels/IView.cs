using System;
using System.Collections.Generic;

namespace DOCUMAT.ViewModels
{
    interface IView<MV, M> : IDisposable
    {
        int NumeroOrdre { get; set; }
        Models.DocumatContext context { get; set; }

        void Add();

        bool GetView(int Id);

        bool Update(M UpElement);

        List<MV> GetViewsList();
    }
}

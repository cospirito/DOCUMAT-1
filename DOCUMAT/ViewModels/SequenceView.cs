using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace DOCUMAT.ViewModels
{
    class SequenceView : IView<SequenceView, Models.Sequence>
    {
        public int NumeroOrdre { get; set; }
        public DocumatContext context { get; set; }
        public Sequence Sequence { get; set; }

        // Propriété pour l'affichage des données
        public string strOrdre { get; set; }
        public string strDate { get; set; }
        public string strReferences { get; set; }
        public int NbRefIndex { get; set; }
        public bool isNbRefs_equal {get; set;}

        // Propriété pour la récupération des données lors du check, concernant le contrôle
        public bool Ordre_Is_Check { get; set; }
        public bool Date_Is_Check { get; set; }
        public bool References_Is_Check { get; set; }
        public bool ASupprimer_Is_Check { get; set; }
        public String NbRefsManquant { get; set; }
        public Dictionary<String, String> ListeRefecrences_check = new Dictionary<String, String>();
        public List<String> RefsList = new List<String>();

        public ObservableCollection<BoolStringClass> TheList { get; set; }

        // Propriété de récupération lors de la correction
        public string ListReferenceFausse { get; set; }
        public bool OrdreFaux { get; set; }
        public bool DateFausse { get; set; }
        public bool ASupprimer { get; set; }
        public Correction Demande_Correction { get; set; }
        public bool En_Correction { get; set; }
        public Models.Image Image { get; set; }
        public ManquantSequence Manquant { get; set; }

        // Propriété indiquant si le parent est manquant : En phase de contrôle 2
        public bool Is_ImageManquant { get; set; }

        public class BoolStringClass
        {
            public string TheText { get; set; }
            public bool TheValue { get; set; }
        }

        public void Add()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool GetView(int Id)
        {
            throw new NotImplementedException();
        }

        public List<SequenceView> GetViewsList()
        {
            throw new NotImplementedException();
        }

        //Est utilisé Uniquement Lors de la Correction PHASE 1 
        static public List<SequenceView> GetManquants(Models.Image image)
        {            
            List<SequenceView> sequenceViews = new List<SequenceView>();  
            using(var ct = new DocumatContext())
            {
                List<ManquantSequence> manquantSequences = ct.ManquantSequences.Where(ms => ms.IdImage == image.ImageID 
                                                                                            && ms.statutManquant == 1).ToList();
                foreach(var manquant in manquantSequences)
                {
                    SequenceView sequenceView = new SequenceView();
                    sequenceView.strOrdre = manquant.NumeroOrdre + "";
                    sequenceView.strDate = "manquant";
                    sequenceView.strReferences = "manquant";
                    sequenceView.Image = image;
                    sequenceView.Manquant = manquant;
                    if(ct.ManquantSequences.Any(ms=>ms.NumeroOrdre == manquant.NumeroOrdre && ms.statutManquant == 0 
                                                && ms.IdImage == image.ImageID))
                    {
                        sequenceView.En_Correction = false;
                    }
                    else
                    {
                        sequenceView.En_Correction = true;
                    }

                    sequenceViews.Add(sequenceView);
                }
            }
            return sequenceViews;
        }

        //Utilisé lors du deuxième controle
        static public List<SequenceView> GetSequenceCorrecte1(List<Sequence> sequences)
        {
            using(var ct = new DocumatContext())
            {
                List<SequenceView> sequenceViews = GetViewsList(sequences);
                List<Correction> corrections = ct.Correction.ToList();
                List<ManquantSequence> manquantSequences = ct.ManquantSequences.ToList();                

                var joins = from s in sequenceViews
                            join c in corrections
                            on s.Sequence.SequenceID equals c.SequenceID
                            where c.PhaseCorrection == 1 && c.StatutCorrection == 0
                            select s;

                var manq = from s in sequenceViews
                           join m in manquantSequences
                           on s.Sequence.SequenceID equals m.IdSequence                           
                           select s;

                // Indique que les séquentes sont manquante
                foreach (var m in manq.ToList())
                {
                    m.Is_ImageManquant = true;
                }

                foreach (var m in joins.ToList())
                {
                    m.Is_ImageManquant = false;
                }

                joins.ToList().AddRange(manq.ToList());

                return joins.ToList();
            }
        }

        //Utilisé lors du controle final
        static public List<SequenceView> GetSequenceCorrecte2(List<Sequence> sequences)
        {
            using (var ct = new DocumatContext())
            {
                List<SequenceView> sequenceViews = GetViewsList(sequences);
                List<Correction> corrections = ct.Correction.ToList();

                var joins = from s in sequenceViews
                            join c in corrections
                            on s.Sequence.SequenceID equals c.SequenceID
                            where c.PhaseCorrection == 3 && c.StatutCorrection == 0
                            select s;

                foreach (var m in joins.ToList())
                {
                    m.Is_ImageManquant = false;
                }

                return joins.ToList();
            }
        }

        static public List<SequenceView> GetViewsList(List<Sequence> sequences)
        {
            List<SequenceView> sequenceViews = new List<SequenceView>();            
            foreach(var sequence in sequences)
            {
                SequenceView sequenceView = new SequenceView();
                sequenceView.Sequence = sequence;
                sequenceView.strOrdre = sequence.NUmeroOdre.ToString();
                sequenceView.strDate = sequence.DateSequence.ToShortDateString();
                sequenceView.strReferences = sequence.References;
                if(sequence.References.ToLower().Contains("defaut"))
                {
                    sequenceView.NbRefIndex = 0;
                }
                else
                {
                    sequenceView.NbRefIndex = sequence.References.Split(',').Length;
                }

                // Définition de la couleur du Nombre de Référence indexer Pour SuperIndexeur
                if (sequence.NombreDeReferences != sequenceView.NbRefIndex)
                {
                    sequenceView.isNbRefs_equal = false;
                }
                else
                {
                    sequenceView.isNbRefs_equal = true;
                }

                if (sequence.isSpeciale.Contains("doublon"))
                {
                    sequenceView.strOrdre = sequence.NUmeroOdre + "d";
                }
                else if(sequence.isSpeciale.Contains("bis"))
                {
                    sequenceView.strOrdre = sequence.NUmeroOdre + "b";
                }
                else if(sequence.isSpeciale.Contains("saut"))
                {
                    sequenceView.strOrdre = sequence.NUmeroOdre + "";
                    sequenceView.strDate = "saut";
                    sequenceView.strReferences = "saut";
                }

                sequenceView.OrdreFaux = false;
                sequenceView.DateFausse = false;
                sequenceView.ASupprimer = false;               

                //Cas d'un contôle : Références Multiples
                string[] ListReferences = sequence.References.Split(',');
                sequenceView.TheList = new ObservableCollection<BoolStringClass>();
                var i = 0;
                foreach (var reference in ListReferences)
                {
                    if (!string.IsNullOrWhiteSpace(reference))
                    {
                        sequenceView.RefsList.Add(reference);
                        sequenceView.TheList.Add(new BoolStringClass() { TheText = reference, TheValue = false });
                    }
                }

                Models.Correction correction = new Models.Correction();
                
                #region CAS D'UNE DEMANDE DE CORRECTION EN PHASE 1
                if(sequence.PhaseActuelle == 1)
                { 
                    sequenceView.En_Correction = false;
                    using (var ct = new DocumatContext())
                    {
                        correction = ct.Correction.FirstOrDefault(c => c.StatutCorrection == 1 && c.PhaseCorrection == 1
                                                            && c.SequenceID == sequence.SequenceID);
                        if (correction != null)
                        {
                            if(ct.Correction.FirstOrDefault(c => c.StatutCorrection == 0 && c.PhaseCorrection == 1
                                                            && c.SequenceID == sequence.SequenceID) != null)
                            {                                
                                sequenceView.En_Correction = false;
                                sequenceView.Demande_Correction = null;
                            }
                            else
                            {
                                #region SEQUENCE NON CORRIGEE 
                                sequenceView.Demande_Correction = correction;
                                sequenceView.En_Correction = true;
                                if(correction.ASupprimer == 1)
                                {
                                    sequenceView.ASupprimer = true;
                                }
                                else
                                {
                                    sequenceView.ASupprimer = false;
                                }

                                if (correction.OrdreSequence_idx == 1)
                                {
                                    sequenceView.OrdreFaux = true;
                                }
                                else
                                {
                                    sequenceView.OrdreFaux = false;
                                }
                                if (correction.DateSequence_idx == 1)
                                {
                                    sequenceView.DateFausse = true;                                
                                }
                                else
                                {
                                    sequenceView.DateFausse = false;
                                }

                                if(correction.RefSequence_idx == 1)
                                {
                                    sequenceView.TheList.Clear();
                                    foreach (var reference in ListReferences)
                                    {
                                        if (!string.IsNullOrWhiteSpace(reference) && correction.RefRejetees_idx.Contains(reference))
                                        {
                                            sequenceView.ListReferenceFausse += reference + ",";
                                            sequenceView.References_Is_Check = true;
                                            sequenceView.RefsList.Add(reference);
                                            sequenceView.TheList.Add(new BoolStringClass() { TheText = reference, TheValue = true });
                                        }
                                    }

                                    // Sequences Manquants
                                    // Récupération du controle
                                    Controle controle = ct.Controle.FirstOrDefault(c=>c.ImageID == sequenceView.Sequence.ImageID && c.SequenceID == sequenceView.Sequence.SequenceID
                                                        && c.StatutControle == 1 && c.PhaseControle == 1 && c.RefSequence_idx == 1);
                                    if(controle != null)
                                    {
                                        // Calcule des Manquants
                                        int RefsManq = controle.RefRejetees_idx.Split(',').Where(st=>st.Contains("/manq")).Count();
                                        sequenceView.NbRefsManquant = RefsManq.ToString();
                                    }
                                }
                                #endregion
                            }
                        }
                    }                
                }
                #endregion

                #region CAS D'UN CONTROLE PHASE 3
                if(sequence.PhaseActuelle == 2)
                {
                    //sequenceView.En_Correction = false;
                    using (var ct = new DocumatContext())
                    {              
                        correction = ct.Correction.FirstOrDefault(c => c.StatutCorrection == 0 && c.PhaseCorrection == 3
                                                            && c.SequenceID == sequence.SequenceID);

                        if(correction != null)
                        {
                            #region SEQUENCE CORRIGEE A CONTROLER PHASE 3
                            //sequenceView.Demande_Correction = correction;
                            //sequenceView.En_Correction = true;
                            sequenceView.ASupprimer = true;

                            if (correction.OrdreSequence_idx == 1)
                            {
                                sequenceView.OrdreFaux = true;
                            }
                            else
                            {
                                sequenceView.OrdreFaux = false;
                            }
                            if (correction.DateSequence_idx == 1)
                            {
                                sequenceView.DateFausse = true;
                            }
                            else
                            {
                                sequenceView.DateFausse = false;
                            }
                            if (correction.RefSequence_idx == 1)
                            {
                                sequenceView.TheList.Clear();
                                foreach (var reference in ListReferences)
                                {
                                    if (!string.IsNullOrWhiteSpace(reference) && correction.RefRejetees_idx.Contains(reference))
                                    {
                                        sequenceView.ListReferenceFausse += reference + ",";
                                        sequenceView.References_Is_Check = false;
                                        sequenceView.RefsList.Add(reference);
                                        sequenceView.TheList.Add(new BoolStringClass() { TheText = reference, TheValue = false });
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            correction = ct.Correction.FirstOrDefault(c => c.StatutCorrection == 0 && c.PhaseCorrection == 1
                                                                && c.SequenceID == sequence.SequenceID);
                            if (correction != null)
                            {
                                #region SEQUENCE CORRIGEE A CONTROLER PHASE 1
                                //sequenceView.Demande_Correction = correction;
                                //sequenceView.En_Correction = true;
                                sequenceView.ASupprimer = true;

                                if (correction.OrdreSequence_idx == 1)
                                {
                                    sequenceView.OrdreFaux = true;
                                }
                                else
                                {
                                    sequenceView.OrdreFaux = false;
                                }
                                if (correction.DateSequence_idx == 1)
                                {
                                    sequenceView.DateFausse = true;
                                }
                                else
                                {
                                    sequenceView.DateFausse = false;
                                }
                                if (correction.RefSequence_idx == 1)
                                {
                                    sequenceView.TheList.Clear();
                                    // Calcule des Références Manquants
                                    int RefsManq = correction.RefRejetees_idx.Split(',').Where(st => st.Contains("/manq")).Count();
                                    sequenceView.NbRefsManquant = RefsManq.ToString();
                                    int NbRefsTotal = 0;
                                    foreach (var reference in ListReferences)
                                    {
                                        NbRefsTotal = NbRefsTotal + 1;
                                        if (!string.IsNullOrWhiteSpace(reference) && correction.RefRejetees_idx.Contains(reference))
                                        {
                                            sequenceView.ListReferenceFausse += reference + ",";
                                            sequenceView.References_Is_Check = false;
                                            sequenceView.RefsList.Add(reference);
                                            sequenceView.TheList.Add(new BoolStringClass() { TheText = reference, TheValue = false });
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }       
                }
                #endregion

                #region CAS DE STATUT EN CORRECTION PHASE 3
                if (sequence.PhaseActuelle == 3)
                {
                    sequenceView.En_Correction = false;
                    using (var ct = new DocumatContext())
                    {
                        correction = ct.Correction.FirstOrDefault(c => c.StatutCorrection == 1 && c.PhaseCorrection == 3
                                                            && c.SequenceID == sequence.SequenceID);
                        if (correction != null)
                        {
                            if (ct.Correction.FirstOrDefault(c => c.StatutCorrection == 0  && c.PhaseCorrection == 3
                                                             && c.SequenceID == sequence.SequenceID) != null)
                            {
                                sequenceView.En_Correction = false;
                                sequenceView.Demande_Correction = null;
                            }
                            else
                            {
                                #region SEQUENCE NON CORRIGEE 
                                sequenceView.Demande_Correction = correction;
                                sequenceView.En_Correction = true;
                                if (correction.ASupprimer == 1)
                                {
                                    sequenceView.ASupprimer = true;
                                }
                                else
                                {
                                    sequenceView.ASupprimer = false;
                                }

                                if (correction.OrdreSequence_idx == 1)
                                {
                                    sequenceView.OrdreFaux = true;
                                }
                                else
                                {
                                    sequenceView.OrdreFaux = false;
                                }
                                if (correction.DateSequence_idx == 1)
                                {
                                    sequenceView.DateFausse = true;
                                }
                                else
                                {
                                    sequenceView.DateFausse = false;
                                }

                                if (correction.RefSequence_idx == 1)
                                {
                                    sequenceView.TheList.Clear();
                                    foreach (var reference in ListReferences)
                                    {
                                        if (!string.IsNullOrWhiteSpace(reference) && correction.RefRejetees_idx.Contains(reference))
                                        {
                                            sequenceView.ListReferenceFausse += reference + ",";
                                            sequenceView.References_Is_Check = true;
                                            sequenceView.RefsList.Add(reference);
                                            sequenceView.TheList.Add(new BoolStringClass() { TheText = reference, TheValue = true });
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }

                
                #endregion

                sequenceViews.Add(sequenceView);
            }
            return sequenceViews;
        }

        public bool Update(Sequence UpElement)
        {
            throw new NotImplementedException();
        }
    }
}

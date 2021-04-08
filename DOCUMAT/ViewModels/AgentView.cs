using DOCUMAT.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DOCUMAT.ViewModels
{
    public class AgentView
    {
        private static string imageDefault = "/Images/user.png";

        public int NumeroOrdre { get; set; }
        public Models.Agent Agent { get; set; }
        public object Affectation { get; private set; }
        public DocumatContext context { get; set; }

        //Nom de l'image local
        public string cheminLocalPhoto { get; private set; }

        public AgentView()
        {
            Agent = new Agent();
            context = new DocumatContext();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public static int Add(Models.Agent agent)
        {
            using (var ct = new DocumatContext())
            {
                if (!ct.Agent.Any(a => a.Matricule.Trim().ToLower() == agent.Matricule.Trim().ToLower()
                 || a.Login.Trim().ToLower() == agent.Login.Trim().ToLower()))
                {
                    ct.Agent.Add(agent);
                    ct.SaveChanges();
                    return agent.AgentID;
                }
                else
                {
                    throw new Exception("Le Matricule et le Login doivent être unique !!!");
                }
            }
        }

        public bool GetView(int Id)
        {
            Agent = context.Agent.FirstOrDefault(a => a.AgentID == Id);

            if (Agent != null)
                return true;
            else
                throw new Exception("L'agent est introuvable !!!");
        }

        public static int Update(Models.Agent agent)
        {
            using (var ct = new DocumatContext())
            {
                if (!ct.Agent.Any(a => a.AgentID != agent.AgentID && (a.Matricule.Trim().ToLower() == agent.Matricule.Trim().ToLower()
                 || a.Login.Trim().ToLower() == agent.Login.Trim().ToLower())))
                {
                    Models.Agent modifAgent = ct.Agent.FirstOrDefault(a => a.AgentID == agent.AgentID);
                    modifAgent.Affectation = agent.Affectation;
                    modifAgent.CheminPhoto = agent.CheminPhoto;
                    modifAgent.DateModif = DateTime.Now;
                    modifAgent.DateNaiss = agent.DateNaiss;
                    modifAgent.Genre = agent.Genre;
                    modifAgent.Login = agent.Login;
                    modifAgent.Matricule = agent.Matricule;
                    modifAgent.Mdp = agent.Mdp;
                    modifAgent.Nom = agent.Nom;
                    modifAgent.Prenom = agent.Prenom;
                    modifAgent.StatutMat = agent.StatutMat;
                    return ct.SaveChanges();
                }
                else
                {
                    throw new Exception("Le Matricule et le Login doivent être unique !!!");
                }
            }
        }

        public static List<AgentView> GetViewsList()
        {
            using (var ct = new DocumatContext())
            {
                List<Models.Agent> Agents = ct.Agent.ToList();
                List<AgentView> AgentViews = new List<AgentView>();
                foreach (Models.Agent ag in Agents)
                {
                    AgentView agV = new AgentView();
                    agV.Agent = ag;
                    agV.Affectation = Enum.GetName(typeof(Enumeration.AffectationAgent), ag.Affectation);

                    agV.cheminLocalPhoto = string.IsNullOrWhiteSpace(ag.CheminPhoto) ? imageDefault : Path.Combine(ConfigurationManager.AppSettings["CheminDossier_Avatar"], ag.CheminPhoto);
                    AgentViews.Add(agV);
                }
                return AgentViews;
            }
        }
    }
}

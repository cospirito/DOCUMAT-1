using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCUMAT.Models
{
    public class Enumeration
    {
        //l'ordre doit être strictement respecter
        public enum AffectationAgent
        {
            INVENTAIRE,
            SCANNE,
            INDEXATION,
            CONTROLE,            
            CORRECTION,
            SUPERVISEUR,
            ADMINISTRATEUR
        }

        public enum Image
        {
            SCANNEE,
            CREEE,
            INSTANCE,
            INDEXEE,
            PHASE1,
            PHASE2,
            PHASE3,
            TERMINEE
        }

        public enum Registre
        {
            CREE,
            PREINDEXE,
            SCANNE,
            INDEXE,
            PHASE1,
            PHASE2,
            PHASE3,
            TERMINE,
        }

        // L'odre ne doit pas être changé
        public enum Genre
        {
            FEMME,
            HOMME
        }

        // L'odre ne doit pas être changé
        public enum StatutMatrimonial
        {
            CELIBATAIRE,
            MARIE,
            DIVORCE,
            VEUF
        }

        // L'odre ne doit pas être changé
        // définit d'abord les traitement CUD de table générale
        public enum TypeTraitement
        {
            CREATION, // TOUS
            MODIFICATION, // TOUS
            SUPPRESSION, // TOUS
            PREINDEXATION_REGISTRE, // REGISTRE          
            PREINDEXATION_REGISTRE_TERMINEE, // REGISTRE
            REGISTRE_SCANNE, // REGISTRE
            REGISTRE_ATTRIBUE_INDEXATION, // REGISTRE
            INDEXATION_REGISTRE_DEBUT, // REGISTRE
            INDEXATION_REGISTRE_TERMINE, // REGISTRE
            CONTROLE_ATTIBUE, // UNITE
            CONTROLE_PH1_DEBUT, // REGISTRE
            CONTROLE_PH1_TERMINE, // REGISTRE
            CORRECTION_ATTRIBUE, // UNITE
            CORRECTION_PH1_DEBUT, // REGISTRE
            CORRECTION_PH1_TERMINE, // REGISTRE
            CONTROLE_PH3_DEBUT, // REGISTRE
            CONTROLE_PH3_TERMINE, // REGISTRE
            CORRECTION_PH3_DEBUT, // REGISTRE
            CORRECTION_PH3_TERMINE, // REGISTRE
        }

        // Pour les séquences les statut sont :
        // ASupprimer : 1 oui, 0 non
        // PhaseControle : 1 phase1, 2 phase2, 3 phase3, 4 phase 4 (Terminé)
        // PhaseCorrection : 1 phase1, 2 phase2, 3 phase3
    }
}

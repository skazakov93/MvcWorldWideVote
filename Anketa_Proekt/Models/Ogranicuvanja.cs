//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Anketa_Proekt.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ogranicuvanja
    {
        public Ogranicuvanja()
        {
            this.Anketas = new HashSet<Anketa>();
            this.Lice = new HashSet<Louse>();
        }
    
        public int id_o { get; set; }
        public string ime_o { get; set; }
        public string opis_o { get; set; }
    
        public virtual ICollection<Anketa> Anketas { get; set; }
        public virtual ICollection<Louse> Lice { get; set; }
    }
}

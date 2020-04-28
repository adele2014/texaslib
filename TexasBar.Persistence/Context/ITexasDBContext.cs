using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TexasBar.Domain.Models;


namespace TexasBar.Persistence.Context
{
   public interface ITexasDBContext
    {

    //   void OnModelCreating(ModelBuilder modelBuilder);
        int SaveChanges(string userid);
        void Dispose();
    }
}

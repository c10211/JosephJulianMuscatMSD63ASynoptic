using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JosephJulianMuscatMSD63ASynoptic.Models.Domain;

namespace BlankWebApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<JosephJulianMuscatMSD63ASynoptic.Models.Domain.Message> Message { get; set; }
    }
}
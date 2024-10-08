﻿using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shortner.Models;

namespace Shortner.Data;

public class AppDbContext:DbContext
{
    public DbSet<Urls> Urls { get; set; }
    
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
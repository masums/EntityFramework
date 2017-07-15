// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindQueryFixtureBase : IQueryFixtureBase
    {
        protected NorthwindQueryFixtureBase()
        {
            var entitySorters = new Dictionary<Type, Func<dynamic, object>>
                {
                    { typeof(Customer), e => e.CustomerID },
                    { typeof(Order), e => e.OrderID },
                    { typeof(Employee), e => e.EmployeeID },
                    { typeof(Product), e => e.ProductID },
                    { typeof(OrderDetail), e => e.OrderID.ToString() + " " + e.ProductID.ToString() },
                };

            var entityAsserters = new Dictionary<Type, Action<dynamic, dynamic>>
            {
            };

            QueryAsserter = new QueryAsserter<NorthwindContext>(
                () => CreateContext(),
                new NorthwindData2(),
                entitySorters,
                entityAsserters);
        }

        public IQueryAsserter QueryAsserter { get; set; }

        public abstract DbContextOptions BuildOptions(IServiceCollection additionalServices = null);

        public virtual NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll,
            bool enableFilters = false)
        {
            EnableFilters = enableFilters;

            return new NorthwindContext(
                Options ?? (Options = new DbContextOptionsBuilder(BuildOptions()).Options),
                queryTrackingBehavior);
        }

        protected bool EnableFilters { get; set; }
        protected DbContextOptions Options { get; set; }

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>();

            modelBuilder.Entity<Employee>(
                e =>
                    {
                        e.Ignore(em => em.Address);
                        e.Ignore(em => em.BirthDate);
                        e.Ignore(em => em.Extension);
                        e.Ignore(em => em.HireDate);
                        e.Ignore(em => em.HomePhone);
                        e.Ignore(em => em.LastName);
                        e.Ignore(em => em.Notes);
                        e.Ignore(em => em.Photo);
                        e.Ignore(em => em.PhotoPath);
                        e.Ignore(em => em.PostalCode);
                        e.Ignore(em => em.Region);
                        e.Ignore(em => em.TitleOfCourtesy);

                        e.HasOne(e1 => e1.Manager).WithMany().HasForeignKey(e1 => e1.ReportsTo);
                    });

            modelBuilder.Entity<Product>(
                e =>
                    {
                        e.Ignore(p => p.CategoryID);
                        e.Ignore(p => p.QuantityPerUnit);
                        e.Ignore(p => p.ReorderLevel);
                        e.Ignore(p => p.UnitsOnOrder);
                    });

            modelBuilder.Entity<Order>(
                e =>
                    {
                        e.Ignore(o => o.Freight);
                        e.Ignore(o => o.RequiredDate);
                        e.Ignore(o => o.ShipAddress);
                        e.Ignore(o => o.ShipCity);
                        e.Ignore(o => o.ShipCountry);
                        e.Ignore(o => o.ShipName);
                        e.Ignore(o => o.ShipPostalCode);
                        e.Ignore(o => o.ShipRegion);
                        e.Ignore(o => o.ShipVia);
                        e.Ignore(o => o.ShippedDate);
                    });

            modelBuilder.Entity<OrderDetail>(e => { e.HasKey(od => new { od.OrderID, od.ProductID }); });

            if (EnableFilters)
            {
                new NorthwindContext().ConfigureFilters(modelBuilder);
            }
        }
    }
}

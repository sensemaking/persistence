﻿using System;
using Sensemaking.Dapper;

namespace Sensemaking.Query.Dapper
{
    public static class Query
    {
        public static void Configure(IDb db)
        {
            Db = db;
        }

        private static IDb? Db;

        internal static IDb Database
        {
            get
            {
                if (Db == null)
                    throw new Exception("Please configure the database.");

                return Db;
            }
        }
    }
}

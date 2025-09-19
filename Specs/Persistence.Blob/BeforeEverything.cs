// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Fdb.Rx.Persistence.Blob;
using Fdb.Rx.Testing.Persistence.Blob;
using NUnit.Framework;

// This class is without namespace on purpose, to make sure it runs once per test assembly regardless of how tests are packaged.
// It must be compiled into test assembly in order to work. Therefore using shared sources or making a copy is necessary.
[SetUpFixture]
public class BeforeEverything
{
    public static Azurite Instance { get; private set; }

    [OneTimeSetUp]
    public void SetUp()
    {
        var runAzurite = bool.Parse(Environment.GetEnvironmentVariable("RunAzurite") ?? "true");
        if (!runAzurite) return;
        
        Instance = InitializeAzuriteWithRetry(2);
    }

    private Azurite InitializeAzuriteWithRetry(int numberOfTries)
    {
        List<Exception> exceptions = null;
        for (var i = 0; i < numberOfTries; i++)
        {
            try
            {
                return new Azurite();
            }
            catch (Exception e)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(e);
            }
        }

        try
        {
            // an extra attempt to capture azurite debug log.
            return new Azurite(includeDebugLog: true);
        }
        catch (Exception e)
        {
            exceptions.Add(e);
        }

        throw new AggregateException(exceptions);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Instance?.Dispose();
    }
}
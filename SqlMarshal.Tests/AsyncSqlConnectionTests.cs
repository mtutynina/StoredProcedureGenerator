﻿// -----------------------------------------------------------------------
// <copyright file="AsyncSqlConnectionTests.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SqlMarshal.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AsyncSqlConnectionTests
    {
        [TestMethod]
        public void ScalarResult()
        {
            string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<int> M(int clientId, string? personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    partial class C
    {
        public partial async Task<int> M(int clientId, string? personId)
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync();
            return (int)result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void ScalarResultWithIntOutput()
        {
            string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<int> M(int clientId, out int personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    partial class C
    {
        public partial async Task<int> M(int clientId, out int personId)
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync();
            personId = (int)personIdParameter.Value;
            return (int)result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MapResultSetToProcedure()
        {
            string source = @"
namespace Foo
{
    public class Item
    {
        public string StringValue { get; set; }
        public int Int32Value { get; set; }
        public int? NullableInt32Value { get; set; }
    }

    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<IList<Item>> M()
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    partial class C
    {
        public partial async Task<IList<Foo.Item>> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = await command.ExecuteReaderAsync();
            var result = new List<Item>();
            while (await reader.ReadAsync())
            {
                var item = new Item();
                var value_0 = reader.GetValue(0);
                item.StringValue = value_0 == DBNull.Value ? (string)null : (string)value_0;
                var value_1 = reader.GetValue(1);
                item.Int32Value = (int)value_1;
                var value_2 = reader.GetValue(2);
                item.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
                result.Add(item);
            }

            await reader.CloseAsync();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MapSingleObjectToProcedure()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [SqlMarshal(""sp_TestSP"")]
        public partial Task<Item> M()
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial async Task<Item> M()
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            var result = await this.dbContext.Items.FromSqlRaw(sqlQuery).AsEnumerable().FirstOrDefaultAsync();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MapSingleObjectToProcedureConnection()
        {
            string source = @"
namespace Foo
{
    public class Item
    {
        public string StringValue { get; set; }
        public int Int32Value { get; set; }
        public int? NullableInt32Value { get; set; }
    }

    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task<Item> M()
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    partial class C
    {
        public partial async Task<Foo.Item> M()
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var sqlQuery = @""sp_TestSP"";
            command.CommandText = sqlQuery;
            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SingleRow);
            if (!(await reader.ReadAsync()))
            {
                return null;
            }

            var result = new Item();
            var value_0 = reader.GetValue(0);
            result.StringValue = value_0 == DBNull.Value ? (string)null : (string)value_0;
            var value_1 = reader.GetValue(1);
            result.Int32Value = (int)value_1;
            var value_2 = reader.GetValue(2);
            result.NullableInt32Value = value_2 == DBNull.Value ? (int?)null : (int)value_2;
            await reader.CloseAsync();
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MapListFromDbContext()
        {
            string source = @"
namespace Foo
{
    class C
    {
        [SqlMarshal(""sp_TestSP"")]
        public partial Task<IList<Item>> M(int clientId, out int? personId)
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Data.Common;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    partial class C
    {
        public partial async Task<IList<Item>> M(int clientId, out int? personId)
        {
            var connection = this.dbContext.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.DbType = System.Data.DbType.Int32;
            personIdParameter.Direction = System.Data.ParameterDirection.Output;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id OUTPUT"";
            var result = await this.dbContext.Items.FromSqlRaw(sqlQuery, parameters).ToListAsync();
            personId = personIdParameter.Value == DBNull.Value ? (int?)null : (int)personIdParameter.Value;
            return result;
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void NoResults()
        {
            string source = @"
namespace Foo
{
    class C
    {
        private DbConnection connection;

        [SqlMarshal(""sp_TestSP"")]
        public partial Task M(int clientId, string? personId);
    }
}";
            string output = this.GetGeneratedOutput(source, NullableContextOptions.Disable);

            Assert.IsNotNull(output);

            var expectedOutput = @"// <auto-generated>
// Code generated by Stored Procedures Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
#nullable enable
#pragma warning disable 1591

namespace Foo
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    partial class C
    {
        public partial async Task M(int clientId, string? personId)
        {
            var connection = this.connection;
            using var command = connection.CreateCommand();

            var clientIdParameter = command.CreateParameter();
            clientIdParameter.ParameterName = ""@client_id"";
            clientIdParameter.Value = clientId;

            var personIdParameter = command.CreateParameter();
            personIdParameter.ParameterName = ""@person_id"";
            personIdParameter.Value = personId == null ? (object)DBNull.Value : personId;

            var parameters = new DbParameter[]
            {
                clientIdParameter,
                personIdParameter,
            };

            var sqlQuery = @""sp_TestSP @client_id, @person_id"";
            command.CommandText = sqlQuery;
            command.Parameters.AddRange(parameters);
            await command.ExecuteNonQueryAsync();
        }
    }
}";
            Assert.AreEqual(expectedOutput, output);
        }

        private string GetGeneratedOutput(string source, NullableContextOptions nullableContextOptions)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create(
                "foo",
                new SyntaxTree[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));

            // var compileDiagnostics = compilation.GetDiagnostics();
            // Assert.IsFalse(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());
            ISourceGenerator generator = new Generator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            Assert.IsFalse(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

            string output = outputCompilation.SyntaxTrees.Last().ToString();

            Console.WriteLine(output);

            return output;
        }
    }
}

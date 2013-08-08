// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Reflection;
    using Moq;
    using Xunit;

    public class DbConfigurationTests
    {
        public class SetConfiguration
        {
            [Fact]
            public void DbConfiguration_cannot_be_set_to_null()
            {
                Assert.Equal(
                    "configuration",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.SetConfiguration(null)).ParamName);
            }
        }

        public class LoadConfiguration
        {
            [Fact]
            public void LoadConfiguration_throws_for_invalid_arguments()
            {
                Assert.Equal(
                    "assemblyHint",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.LoadConfiguration((Assembly)null)).ParamName);

                Assert.Equal(
                    "contextType",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.LoadConfiguration((Type)null)).ParamName);

                Assert.Equal(
                    Strings.BadContextTypeForDiscovery("Random"),
                    Assert.Throws<ArgumentException>(() => DbConfiguration.LoadConfiguration(typeof(Random))).Message);
            }
        }

        public class AddDependencyResolver
        {
            [Fact]
            public void AddDependencyResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDependencyResolver(null)).ParamName);
            }

            [Fact]
            public void AddDependencyResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDependencyResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDependencyResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddDependencyResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDependencyResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(resolver, false));
            }
        }

        public class AddDefaultResolver
        {
            [Fact]
            public void AddDefaultResolver_throws_if_given_a_null_resolver()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddDefaultResolver(null)).ParamName);
            }

            [Fact]
            public void AddDefaultResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddDefaultResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.AddDefaultResolver(new Mock<IDbDependencyResolver>().Object)).Message);
            }

            [Fact]
            public void AddDefaultResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var resolver = new Mock<IDbDependencyResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddDefaultResolver(resolver);

                mockInternalConfiguration.Verify(m => m.AddDefaultResolver(resolver));
            }
        }

        public class Loaded
        {
            [Fact]
            public void Loaded_throws_when_attempting_to_add_or_remove_a_null_handler()
            {
                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.Loaded += null).ParamName);

                Assert.Equal(
                    "value",
                    Assert.Throws<ArgumentNullException>(() => DbConfiguration.Loaded -= null).ParamName);
            }
        }

        public class ProviderServices
        {
            [Fact]
            public void ProviderServices_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "provider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetProviderServices("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetProviderServices(null, new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetProviderServices("", new Mock<DbProviderServices>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetProviderServices(" ", new Mock<DbProviderServices>().Object)).Message);
            }

            [Fact]
            public void ProviderServices_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderServices("900.ForTheWin", providerServices);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerServices, "900.ForTheWin"));
            }

            [Fact]
            public void ProviderServices_also_adds_the_provider_as_a_default_resolver()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var providerServices = new Mock<DbProviderServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderServices("900.ForTheWin", providerServices);

                mockInternalConfiguration.Verify(m => m.AddDefaultResolver(providerServices));
            }

            [Fact]
            public void ProviderServices_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetProviderServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetProviderServices("Karl", new Mock<DbProviderServices>().Object)).Message);
            }
        }

        public class ProviderFactory
        {
            [Fact]
            public void ProviderFactory_throws_if_given_a_null_provider_or_bad_invariant_name()
            {
                Assert.Equal(
                    "providerFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetProviderFactory("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetProviderFactory(null, new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetProviderFactory("", new Mock<DbProviderFactory>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetProviderFactory(" ", new Mock<DbProviderFactory>().Object)).Message);
            }

            [Fact]
            public void ProviderFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var providerFactory = new Mock<DbProviderFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderFactory("920.ForTheWin", providerFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(providerFactory, "920.ForTheWin"));
                mockInternalConfiguration.Verify(m => m.AddDependencyResolver(new InvariantNameResolver(providerFactory, "920.ForTheWin"), false));
            }

            [Fact]
            public void ProviderFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetProviderFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetProviderFactory("Karl", new Mock<DbProviderFactory>().Object)).Message);
            }
        }

        public class ExecutionStrategy
        {
            [Fact]
            public void Throws_if_given_a_null_server_name_or_bad_invariant_name()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy(null, () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy("", () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy(" ", () => new Mock<IDbExecutionStrategy>().Object)).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetExecutionStrategy("a", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy(null, () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy("", () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy(" ", () => new Mock<IDbExecutionStrategy>().Object, "a")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, null)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, "")).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("serverName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, " ")).Message);
                Assert.Equal(
                    "getExecutionStrategy",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetExecutionStrategy("a", null, "b")).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetExecutionStrategy"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetExecutionStrategy("a", () => new Mock<IDbExecutionStrategy>().Object, "b")).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var executionStrategy = new Func<IDbExecutionStrategy>(() => new Mock<IDbExecutionStrategy>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).SetExecutionStrategy("a", executionStrategy, "b");

                mockInternalConfiguration.Verify(
                    m => m.AddDependencyResolver(It.IsAny<ExecutionStrategyResolver<IDbExecutionStrategy>>(), false));
            }
        }

        public class ConnectionFactory : TestBase
        {
            [Fact]
            public void Setting_ConnectionFactory_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "connectionFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetDefaultConnectionFactory(null)).ParamName);
            }

            [Fact]
            public void Setting_ConnectionFactory_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDefaultConnectionFactory"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultConnectionFactory(new Mock<IDbConnectionFactory>().Object)).Message);
            }

            [Fact]
            public void ConnectionFactory_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var connectionFactory = new Mock<IDbConnectionFactory>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultConnectionFactory(connectionFactory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(connectionFactory));
            }

            [Fact]
            public void ConnectionFactory_set_in_code_can_be_overriden_before_config_is_locked()
            {
                Assert.IsType<SqlConnectionFactory>(DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>());
                Assert.IsType<DefaultUnitTestsConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories[0]);
            }
        }

        public class PluralizationService
        {
            [Fact]
            public void Setting_PluralizationService_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "pluralizationService",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetPluralizationService(null)).ParamName);
            }

            [Fact]
            public void Setting_PluralizationService_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetPluralizationService"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetPluralizationService(new Mock<IPluralizationService>().Object)).Message);
            }

            [Fact]
            public void PluralizationService_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var pluralizationService = new Mock<IPluralizationService>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetPluralizationService(pluralizationService);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(pluralizationService));
            }
        }

        public class DependencyResolver
        {
            [Fact]
            public void DefaultModelCacheKey_is_returned_by_default_cache_key_factory()
            {
                var factory = DbConfiguration.DependencyResolver.GetService<Func<DbContext, IDbModelCacheKey>>();

                using (var context = new CacheKeyContext())
                {
                    Assert.IsType<DefaultModelCacheKey>(factory(context));
                }
            }

            public class CacheKeyContext : DbContext
            {
                static CacheKeyContext()
                {
                    Database.SetInitializer<CacheKeyContext>(null);
                }
            }
        }

        public class DatabaseInitializer
        {
            [Fact]
            public void DatabaseInitializer_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDatabaseInitializer"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDatabaseInitializer(new Mock<IDatabaseInitializer<DbContext>>().Object)).Message);
            }

            [Fact]
            public void DatabaseInitializer_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var initializer = new Mock<IDatabaseInitializer<DbContext>>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer(initializer);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(initializer));
            }

            [Fact]
            public void DatabaseInitializer_creates_null_initializer_when_given_null_argument()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseInitializer<DbContext>(null);

                mockInternalConfiguration.Verify(
                    m => m.RegisterSingleton<IDatabaseInitializer<DbContext>>(It.IsAny<NullDatabaseInitializer<DbContext>>()));
            }
        }

        public class MigrationSqlGeneratorTests
        {
            [Fact]
            public void MigrationSqlGenerator_throws_if_given_a_null_generator_or_bad_invariant_name()
            {
                Assert.Equal(
                    "sqlGenerator",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetMigrationSqlGenerator("Karl", null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetMigrationSqlGenerator(null, () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetMigrationSqlGenerator("", () => new Mock<MigrationSqlGenerator>().Object)).Message);
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetMigrationSqlGenerator(" ", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void MigrationSqlGenerator_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetMigrationSqlGenerator"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetMigrationSqlGenerator("Karl", () => new Mock<MigrationSqlGenerator>().Object)).Message);
            }

            [Fact]
            public void MigrationSqlGenerator_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var generator = new Func<MigrationSqlGenerator>(() => new Mock<MigrationSqlGenerator>().Object);

                new DbConfiguration(mockInternalConfiguration.Object).SetMigrationSqlGenerator("Karl", generator);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(generator, "Karl"));
            }
        }

        public class ManifestTokenResolver
        {
            [Fact]
            public void ManifestTokenResolver_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "resolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetManifestTokenResolver(null)).ParamName);
            }

            [Fact]
            public void ManifestTokenResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetManifestTokenResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetManifestTokenResolver(new Mock<IManifestTokenResolver>().Object)).Message);
            }

            [Fact]
            public void ManifestTokenResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var service = new Mock<IManifestTokenResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetManifestTokenResolver(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service));
            }
        }

        public class ProviderFactoryResolver
        {
            [Fact]
            public void ProviderFactoryResolver_throws_if_given_a_null_service()
            {
                Assert.Equal(
                    "providerFactoryResolver",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetProviderFactoryResolver(null)).ParamName);
            }

            [Fact]
            public void ProviderFactoryResolver_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetProviderFactoryResolver"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetProviderFactoryResolver(new Mock<IDbProviderFactoryResolver>().Object)).Message);
            }

            [Fact]
            public void ProviderFactoryResolver_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var service = new Mock<IDbProviderFactoryResolver>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetProviderFactoryResolver(service);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(service));
            }
        }

        public class ModelCacheKey
        {
            [Fact]
            public void ModelCacheKey_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "keyFactory",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetModelCacheKey(null)).ParamName);
            }

            [Fact]
            public void ModelCacheKey_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetModelCacheKey"),
                    Assert.Throws<InvalidOperationException>(() => configuration.SetModelCacheKey(c => null)).Message);
            }

            [Fact]
            public void ModelCacheKey_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var factory = (Func<DbContext, IDbModelCacheKey>)(c => null);

                new DbConfiguration(mockInternalConfiguration.Object).SetModelCacheKey(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class HistoryContextFactory
        {
            [Fact]
            public void Throws_if_given_a_null_provider()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetHistoryContext(null, null)).Message);
            }

            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "factory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetHistoryContext("Foo", null)).ParamName);

                Assert.Equal(
                    "factory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetDefaultHistoryContext(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetHistoryContext"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetHistoryContext("Foo", (e, d) => null)).Message);

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDefaultHistoryContext"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultHistoryContext((e, d) => null)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                Func<DbConnection, string, HistoryContext> factory = (e, d) => null;

                new DbConfiguration(mockInternalConfiguration.Object).SetHistoryContext("Foo", factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory, "Foo"));
            }

            [Fact]
            public void Default_factory_method_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                Func<DbConnection, string, HistoryContext> factory = (e, d) => null;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultHistoryContext(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class SetDefaultSpatialProvider
        {
            [Fact]
            public void SetDefaultSpatialProvider_throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetDefaultSpatialServices(null)).ParamName);
            }

            [Fact]
            public void SetDefaultSpatialProvider_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDefaultSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDefaultSpatialServices(new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetDefaultSpatialProvider_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var provider = new Mock<DbSpatialServices>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).SetDefaultSpatialServices(provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(provider));
            }
        }

        public class SetSpatialProvider
        {
            [Fact]
            public void SetSpatialProvider_throws_if_given_a_null_factory_or_key_or_an_empty_invariant_name()
            {
                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().SetSpatialServices("Good.As.Gold", null)).ParamName);

                Assert.Equal(
                    "spatialProvider",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetSpatialServices(new DbProviderInfo("Especially.For.You", ""), null)).ParamName);

                Assert.Equal(
                    "key",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetSpatialServices((DbProviderInfo)null, new Mock<DbSpatialServices>().Object))
                        .ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetSpatialServices((string)null, new Mock<DbSpatialServices>().Object)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetSpatialServices("", new Mock<DbSpatialServices>().Object)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                    Assert.Throws<ArgumentException>(
                        () => new DbConfiguration().SetSpatialServices(" ", new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_invariant_name_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetSpatialServices("Song.For.Whoever", new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_key_throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetSpatialServices"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetSpatialServices(
                            new DbProviderInfo("Song.For.Whoever", ""),
                            new Mock<DbSpatialServices>().Object)).Message);
            }

            [Fact]
            public void SetSpatialProvider_with_invariant_name_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var provider = SqlSpatialServices.Instance;
                Func<object, bool> keyPredicate = null;
                mockInternalConfiguration.Setup(
                    m => m.RegisterSingleton<DbSpatialServices>(
                        provider,
                        It.IsAny<Func<object, bool>>())).Callback<DbSpatialServices, Func<object, bool>>((s, k) => { keyPredicate = k; });

                new DbConfiguration(mockInternalConfiguration.Object).SetSpatialServices("Mini.Tattoo", provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, It.IsAny<Func<object, bool>>()));

                Assert.True(keyPredicate(new DbProviderInfo("Mini.Tattoo", "Foo")));
                Assert.False(keyPredicate(new DbProviderInfo("Maxi.Tattoo", "Foo")));
                Assert.False(keyPredicate("Mini.Tattoo"));
                Assert.False(keyPredicate(null));
            }

            [Fact]
            public void SetSpatialProvider_with_key_delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var provider = SqlSpatialServices.Instance;

                var key = new DbProviderInfo("A.Little.Time", "Paul");
                new DbConfiguration(mockInternalConfiguration.Object).SetSpatialServices(key, provider);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton<DbSpatialServices>(provider, key));
            }
        }

        public class DatabaseLogFormatterTests
        {
            [Fact]
            public void Throws_if_given_a_null_factory()
            {
                Assert.Equal(
                    "logFormatterFactory",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbConfiguration().SetDatabaseLogFormatter(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("SetDatabaseLogFormatter"),
                    Assert.Throws<InvalidOperationException>(
                        () => configuration.SetDatabaseLogFormatter((_, __) => null)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                Func<DbContext, Action<string>, DatabaseLogFormatter> factory = (_, __) => null;

                new DbConfiguration(mockInternalConfiguration.Object).SetDatabaseLogFormatter(factory);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(factory));
            }
        }

        public class Interceptor
        {
            [Fact]
            public void Throws_if_given_a_null_interceptor()
            {
                Assert.Equal(
                    "interceptor",
                    Assert.Throws<ArgumentNullException>(() => new DbConfiguration().AddInterceptor(null)).ParamName);
            }

            [Fact]
            public void Throws_if_the_configuation_is_locked()
            {
                var configuration = CreatedLockedConfiguration();

                Assert.Equal(
                    Strings.ConfigurationLocked("AddInterceptor"),
                    Assert.Throws<InvalidOperationException>(() => configuration.AddInterceptor(new Mock<IDbInterceptor>().Object)).Message);
            }

            [Fact]
            public void Delegates_to_internal_configuration()
            {
                var mockInternalConfiguration = new Mock<InternalConfiguration>(null, null, null, null, null);
                var interceptor = new Mock<IDbInterceptor>().Object;

                new DbConfiguration(mockInternalConfiguration.Object).AddInterceptor(interceptor);

                mockInternalConfiguration.Verify(m => m.RegisterSingleton(interceptor));
            }
        }

        private static DbConfiguration CreatedLockedConfiguration()
        {
            var configuration = new DbConfiguration();
            configuration.InternalConfiguration.Lock();
            return configuration;
        }
    }
}
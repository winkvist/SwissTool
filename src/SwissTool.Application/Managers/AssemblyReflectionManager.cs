// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyReflectionManager.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//  Modified from http://www.codeproject.com/Articles/453778/Loading-Assemblies-from-Anywhere-into-a-New-AppDom
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Policy;
    
    /// <summary>
    /// The assembly reflection manager.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class AssemblyReflectionManager : IDisposable
    {
        /// <summary>
        /// The map domains
        /// </summary>
        private readonly Dictionary<string, AppDomain> mapDomains = new Dictionary<string, AppDomain>();

        /// <summary>
        /// The loaded assemblies
        /// </summary>
        private readonly Dictionary<string, AppDomain> loadedAssemblies = new Dictionary<string, AppDomain>();

        /// <summary>
        /// The proxies
        /// </summary>
        private readonly Dictionary<string, AssemblyReflectionProxy> proxies = new Dictionary<string, AssemblyReflectionProxy>();

        /// <summary>
        /// Finalizes an instance of the <see cref="AssemblyReflectionManager"/> class.
        /// </summary>
        ~AssemblyReflectionManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Loads the assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>A value indicating whether the assembly was loaded.</returns>
        public bool LoadAssembly(string assemblyPath, string domainName)
        {
            // if the assembly file does not exist then fail
            if (!File.Exists(assemblyPath))
            {
                return false;
            }

            // if the assembly was already loaded then fail
            if (this.loadedAssemblies.ContainsKey(assemblyPath))
            {
                return false;
            }

            // check if the appdomain exists, and if not create a new one
            AppDomain appDomain = null;
            if (this.mapDomains.ContainsKey(domainName))
            {
                appDomain = this.mapDomains[domainName];
            }
            else
            {
                appDomain = this.CreateChildDomain(AppDomain.CurrentDomain, domainName);
                this.mapDomains[domainName] = appDomain;
            }

            // load the assembly in the specified app domain
            try
            {
                var proxyType = typeof(AssemblyReflectionProxy);
                var proxy = (AssemblyReflectionProxy)appDomain.CreateInstanceFrom(proxyType.Assembly.Location, proxyType.FullName).Unwrap();

                proxy.LoadAssembly(assemblyPath);

                this.loadedAssemblies[assemblyPath] = appDomain;
                this.proxies[assemblyPath] = proxy;

                return true;
            }
            catch
            {
                
            }

            return false;
        }

        /// <summary>
        /// Unloads the assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>A value indicating whether the assembly was unloaded.</returns>
        public bool UnloadAssembly(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                return false;
            }

            // check if the assembly is found in the internal dictionaries
            if (this.loadedAssemblies.ContainsKey(assemblyPath) && this.proxies.ContainsKey(assemblyPath))
            {
                // check if there are more assemblies loaded in the same app domain; in this case fail
                var appDomain = this.loadedAssemblies[assemblyPath];
                var count = this.loadedAssemblies.Values.Count(a => a == appDomain);
                if (count != 1)
                {
                    return false;
                }

                try
                {
                    // remove the appdomain from the dictionary and unload it from the process
                    this.mapDomains.Remove(appDomain.FriendlyName);
                    AppDomain.Unload(appDomain);

                    // remove the assembly from the dictionaries
                    this.loadedAssemblies.Remove(assemblyPath);
                    this.proxies.Remove(assemblyPath);

                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Unloads the domain.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>A value indicating whether the domain was unloaded.</returns>
        public bool UnloadDomain(string domainName)
        {
            // check the appdomain name is valid
            if (string.IsNullOrEmpty(domainName))
            {
                return false;
            }

            // check we have an instance of the domain
            if (this.mapDomains.ContainsKey(domainName))
            {
                try
                {
                    var appDomain = this.mapDomains[domainName];

                    // check the assemblies that are loaded in this app domain
                    var assemblies = new List<string>();

                    foreach (var kvp in this.loadedAssemblies)
                    {
                        if (kvp.Value == appDomain)
                        {
                            assemblies.Add(kvp.Key);
                        }
                    }

                    // remove these assemblies from the internal dictionaries
                    foreach (var assemblyName in assemblies)
                    {
                        this.loadedAssemblies.Remove(assemblyName);
                        this.proxies.Remove(assemblyName);
                    }

                    // remove the appdomain from the dictionary
                    this.mapDomains.Remove(domainName);

                    // unload the appdomain
                    AppDomain.Unload(appDomain);

                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Reflects the specified assembly path.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="func">The function.</param>
        /// <returns>The result.</returns>
        public TResult Reflect<TResult>(string assemblyPath, Func<Assembly, TResult> func)
        {
            // check if the assembly is found in the internal dictionaries
            if (this.loadedAssemblies.ContainsKey(assemblyPath) && this.proxies.ContainsKey(assemblyPath))
            {
                return this.proxies[assemblyPath].Reflect(func);
            }

            return default(TResult);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }       

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var appDomain in this.mapDomains.Values)
                {
                    AppDomain.Unload(appDomain);
                }

                this.loadedAssemblies.Clear();
                this.proxies.Clear();
                this.mapDomains.Clear();
            }
        }

        /// <summary>
        /// Creates the child domain.
        /// </summary>
        /// <param name="parentDomain">The parent domain.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>The app domain.</returns>
        private AppDomain CreateChildDomain(AppDomain parentDomain, string domainName)
        {
            var evidence = new Evidence(parentDomain.Evidence);
            var setup = parentDomain.SetupInformation;

            return AppDomain.CreateDomain(domainName, evidence, setup);
        }

        /// <summary>
        /// The assembly reflection proxy.
        /// </summary>
        /// <seealso cref="System.MarshalByRefObject" />
        private class AssemblyReflectionProxy : MarshalByRefObject
        {
            /// <summary>
            /// The assembly path
            /// </summary>
            private string assemblyPath;

            /// <summary>
            /// Loads the assembly.
            /// </summary>
            /// <param name="assemblyPath">The assembly path.</param>
            public void LoadAssembly(string assemblyPath)
            {
                try
                {
                    this.assemblyPath = assemblyPath;
                    Assembly.LoadFrom(assemblyPath);
                }
                catch (FileNotFoundException)
                {
                    // Continue loading assemblies even if an assembly can not be loaded in the new AppDomain.
                }
            }

            /// <summary>
            /// Reflects the specified function.
            /// </summary>
            /// <typeparam name="TResult">The type of the result.</typeparam>
            /// <param name="func">The function.</param>
            /// <returns>The result.</returns>
            public TResult Reflect<TResult>(Func<Assembly, TResult> func)
            {
                var directory = new FileInfo(this.assemblyPath).Directory;
                ResolveEventHandler resolveEventHandler = (s, e) => this.OnAssemblyResolve(e, directory);

                AppDomain.CurrentDomain.AssemblyResolve += resolveEventHandler;

                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Compare(a.Location, this.assemblyPath, StringComparison.OrdinalIgnoreCase) == 0);
                var result = func(assembly);

                AppDomain.CurrentDomain.AssemblyResolve -= resolveEventHandler;

                return result;
            }

            /// <summary>
            /// Raises the <see cref="E:ReflectionOnlyResolve" /> event.
            /// </summary>
            /// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
            /// <param name="directory">The directory.</param>
            /// <returns>The assembly.</returns>
            private Assembly OnAssemblyResolve(ResolveEventArgs args, DirectoryInfo directory)
            {
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                var assemblyName = new AssemblyName(args.Name);
                var dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

                if (File.Exists(dependentAssemblyFilename))
                {
                    return Assembly.LoadFrom(dependentAssemblyFilename);
                }

                return Assembly.Load(args.Name);
            }
        }
    }
}

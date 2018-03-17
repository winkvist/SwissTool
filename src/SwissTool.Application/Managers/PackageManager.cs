// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageManager.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApplicationUpdater type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    using Newtonsoft.Json;

    using SwissTool.Application.Models;
    using SwissTool.Framework.Definitions;
    using SwissTool.Framework.Extensions;
    using SwissTool.Framework.Infrastructure;
    using SwissTool.Logging;

    /// <summary>
    /// The application updater class.
    /// </summary>
    internal static class PackageManager
    {
        /// <summary>
        /// The update service url
        /// </summary>
#if DEBUG
        private const string ServiceBaseUrl = "http://localhost:60565/api/repository/";
#else
        private const string ServiceBaseUrl = "http://www.swissapps.org/api/repository/";
#endif

        /// <summary>
        /// Gets the application.
        /// </summary>
        /// <value>The application.</value>
        internal static IApplication Application { get; private set; }

        /// <summary>
        /// Setups the package manager.
        /// </summary>
        /// <param name="application">The application.</param>
        internal static void Setup(IApplication application)
        {
            Application = application;
        }

        /// <summary>
        /// Checks for application updates.
        /// </summary>
        /// <returns>A value indicating whether there are application updates available.</returns>
        internal static bool CheckApplicationUpdates()
        {
            var versions = GetInstalledApplicationVersions(true);

            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(versions);
                var response = client.PostAsync(new Uri(new Uri(ServiceBaseUrl), "checkpackageupdates"), new StringContent(json, Encoding.Default, "application/json")).Result;
                var stringResult = response.Content.ReadAsStringAsync().Result;

                return stringResult == "true";
            }
        }

        /// <summary>
        /// Gets all available application updates.
        /// </summary>
        /// <param name="includePreReleases">if set to <c>true</c> [include pre releases].</param>
        /// <returns>
        /// A list of application update info.
        /// </returns>
        internal static List<DetailedPackageVersion> GetApplicationUpdates(bool includePreReleases = false)
        {
            var versions = GetInstalledApplicationVersions(true);

            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(versions);

                var action = includePreReleases ? "getpackageupdates/?includePreReleases=true" : "getpackageupdates";

                var uri = new Uri(new Uri(ServiceBaseUrl), action);
                var response = client.PostAsync(uri, new StringContent(json, Encoding.Default, "application/json")).Result;
                var jsonResult = response.Content.ReadAsStringAsync().Result;

                var list = JsonConvert.DeserializeObject<List<DetailedPackageVersion>>(jsonResult);

                return list;
            }
        }

        /// <summary>
        /// Downloads the update files.
        /// </summary>
        /// <param name="updatesToInstall">The updates to install.</param>
        internal static void DownloadUpdateFiles(IEnumerable<DetailedPackageVersion> updatesToInstall)
        {
            var appInfo = ReadApplicationInfo();

            if (!Directory.Exists(AppConstants.UpdatesDataPath))
            {
                Directory.CreateDirectory(AppConstants.UpdatesDataPath);
            }

            var metadataFile = Path.Combine(AppConstants.UpdatesDataPath, Guid.NewGuid() + ".xml");

            var applicationInstallationInfo = new List<PackageInstallationInfo>();

            using (var client = new WebClient())
            {
                foreach (var updateToInstall in updatesToInstall)
                {
                    var existingApplication = appInfo.FirstOrDefault(ia => ia.Identifier == updateToInstall.Identifier);

                    if (existingApplication == null)
                    {
                        throw new ApplicationException("The application for this update is not installed.");
                    }

                    var downloadFilename = updateToInstall.DownloadFilename;
                    var fileExtension = Path.GetExtension(downloadFilename);
                    var fileName = Guid.NewGuid() + fileExtension;
                    var tempFilePath = Path.Combine(AppConstants.UpdatesDataPath, fileName);
                    var downloadUrl = ServiceBaseUrl + $"downloadpackage/{updateToInstall.Identifier}/{downloadFilename}";

                    client.DownloadFile(downloadUrl, tempFilePath);

                    applicationInstallationInfo.Add(new PackageInstallationInfo
                        {
                            ApplicationIdentifier = updateToInstall.Identifier.ToString(),
                            ApplicationVersion = updateToInstall.Version.ToString(),
                            ApplicationPath = existingApplication.Path,
                            InstallationFile = tempFilePath
                        });
                }
            }

            var installationSourceData = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(
                    "installationPackages",
                    applicationInstallationInfo.Select(
                        info =>
                        new XElement(
                            "installationPackage",
                            new XElement("applicationIdentifier", info.ApplicationIdentifier),
                            new XElement("applicationVersion", info.ApplicationVersion),
                            new XElement("applicationPath", info.ApplicationPath),
                            new XElement("installationFile", info.InstallationFile)))));

            installationSourceData.Save(metadataFile);
        }

        /// <summary>
        /// Reads the application info.
        /// </summary>
        /// <returns>The application info.</returns>
        private static List<ApplicationInfo> ReadApplicationInfo()
        {
            var applications = new List<ApplicationInfo>();

            var files = Directory.GetFiles(Application.HomeDirectory, "*.dll", SearchOption.AllDirectories);

            using (var manager = new AssemblyReflectionManager())
            {
                foreach (var file in files)
                {
                    try
                    {
                        manager.LoadAssembly(file, "SwissTool.Packages");

                        var applicationInfo = manager.Reflect(
                            file,
                            a => 
                            {
                                if (a == null)
                                {
                                    return null;
                                }

                                var isApplication = a.GetTypes().Any(t => t.IsClass && t.IsPublic && !t.IsAbstract && t.IsSubclassOf(typeof(ApplicationBase)));
                                if (!isApplication)
                                {
                                    return null;
                                }

                                return new ApplicationInfo
                                {
                                    Name = a.GetAssemblyName(),
                                    Identifier = a.GetAssemblyIdentifier(),
                                    Path = a.GetAssemblyDirectoryPath(),
                                    Version = a.GetAssemblyVersion()
                                };
                            });

                        if (applicationInfo != null)
                        {
                            applications.Add(applicationInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.DebugException(ex, "Error while loading assembly {0}", file ?? string.Empty);
                    }
                }
            }

            return applications;
        }

        /// <summary>
        /// Gets the application versions for installed applications.
        /// </summary>
        /// <param name="includePendingPackages">if set to <c>true</c> includes pending packages.</param>
        /// <returns>
        /// The installed application versions.
        /// </returns>
        /// <exception cref="System.ApplicationException">The installation structure file is corrupt.</exception>
        private static IEnumerable<PackageVersion> GetInstalledApplicationVersions(bool includePendingPackages = false)
        {
            var installedApplications = ReadApplicationInfo();
            var installedPackageVersions = installedApplications.Select(a => new PackageVersion { Identifier = a.Identifier, Version = a.Version }).ToList();

            if (includePendingPackages && Directory.Exists(AppConstants.UpdatesDataPath))
            {
                var pendingPackages = new List<PackageVersion>();

                var xmlFiles = Directory.GetFiles(AppConstants.UpdatesDataPath, "*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    using (var xmlTextReader = new XmlTextReader(xmlFile))
                    {
                        var document = XDocument.Load(xmlTextReader);

                        foreach (var installationPackage in document.Descendants("installationPackage"))
                        {
                            var applicationIdentifier = (string)installationPackage.Element("applicationIdentifier");
                            var applicationVersion = (string)installationPackage.Element("applicationVersion");

                            if (string.IsNullOrEmpty(applicationIdentifier) || string.IsNullOrEmpty(applicationVersion))
                            {
                                throw new ApplicationException("The installation structure file is corrupt.");
                            }

                            pendingPackages.Add(new PackageVersion
                            {
                                Identifier = Guid.Parse(applicationIdentifier),
                                Version = new Version(applicationVersion),
                            });
                        }
                    }
                }

                var installedPackagesDict = installedPackageVersions.ToDictionary(a => a.Identifier);

                foreach (var pendingPackage in pendingPackages)
                {
                    if (installedPackagesDict.ContainsKey(pendingPackage.Identifier))
                    {
                        var package = installedPackagesDict[pendingPackage.Identifier];

                        if (pendingPackage.Version > package.Version)
                        {
                            installedPackagesDict[pendingPackage.Identifier] = pendingPackage;
                        }
                    }
                    else
                    {
                        installedPackageVersions.Add(pendingPackage);
                    }
                }

                installedPackageVersions = installedPackagesDict.Values.ToList();
            }

            return installedPackageVersions;
        }
    }
}

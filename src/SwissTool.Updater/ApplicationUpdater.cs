// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationUpdater.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApplicationUpdater type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace SwissTool.Updater
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// The application updater.
    /// </summary>
    public static class ApplicationUpdater
    {
        /// <summary>
        /// Installs the application updates.
        /// </summary>
        /// <param name="installationSourcePath">The installation source path.</param>
        /// <returns>The applications that were installed.</returns>
        public static bool InstallUpdates(string installationSourcePath)
        {
            var installedUpdates = false;

            if (!Directory.Exists(installationSourcePath))
            {
                return false;
            }

            var xmlFiles = Directory.GetFiles(installationSourcePath, "*.xml");

            var validPackages = new List<InstallationPackage>();

            foreach (var xmlFile in xmlFiles)
            {
                using (var xmlTextReader = new XmlTextReader(xmlFile))
                {
                    var document = XDocument.Load(xmlTextReader);
                
                    foreach (var installationPackage in document.Descendants("installationPackage"))
                    {
                        var applicationIdentifier = (string)installationPackage.Element("applicationIdentifier");
                        var applicationVersion = (string)installationPackage.Element("applicationVersion");
                        var applicationPath = (string)installationPackage.Element("applicationPath");
                        var installationFile = (string)installationPackage.Element("installationFile");

                        if (string.IsNullOrEmpty(applicationIdentifier) || string.IsNullOrEmpty(applicationVersion) || string.IsNullOrEmpty(applicationPath) || string.IsNullOrEmpty(installationFile))
                        {
                            throw new ApplicationException("The installation structure file is corrupt.");
                        }

                        validPackages.Add(new InstallationPackage
                                          {
                                              ApplicationIdentifier = applicationIdentifier,
                                              ApplicationVersion = applicationVersion,
                                              ApplicationPath = applicationPath,
                                              InstallationFile = installationFile
                                          });

                    }
                }

                var orderedPackages = validPackages.OrderBy(x => x.ApplicationIdentifier).ThenBy(x => x.ApplicationVersion).ToList();

                foreach (var orderedPackage in orderedPackages)
                {
                    var applicationPath = orderedPackage.ApplicationPath;
                    var installationFile = orderedPackage.InstallationFile;

                    if (!Directory.Exists(applicationPath))
                    {
                        Directory.CreateDirectory(applicationPath);
                    }

                    using (var streamReader = new StreamReader(installationFile))
                    {
                        // Open and extract the downloaded archive.
                        var rarArchive = RarArchive.Open(streamReader.BaseStream);

                        foreach (var entry in rarArchive.Entries)
                        {
                            if (entry.IsDirectory)
                            {
                                var entryPath = Path.Combine(applicationPath, entry.Key);

                                if (!Directory.Exists(entryPath))
                                {
                                    Directory.CreateDirectory(entryPath);
                                }
                            }
                            else
                            {
                                entry.WriteToDirectory(applicationPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                    }

                    // Remove the installation file.
                    File.Delete(installationFile);
                }

                // Clean up the processed file.
                File.Delete(xmlFile);

                // Indicate that there were updates installed.
                installedUpdates = true;
            }

            return installedUpdates;
        }
    }
}

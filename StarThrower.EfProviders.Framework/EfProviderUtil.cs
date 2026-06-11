// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace StarThrower.EfProviders.Framework
{
    internal static class EfProviderUtil
    {
        internal static DateTime DbNullDate = new DateTime(1940, 1, 1);

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            string sValue = config[valueName];

            if (sValue == null)
            {
                return defaultValue;
            }

            int iValue;
            if (!Int32.TryParse(sValue, out iValue))
            {
                if (zeroAllowed)
                {
                    throw new ProviderException(String.Format(Resources.Value_must_be_non_negative_integer, valueName));
                }

                throw new ProviderException(String.Format(Resources.Value_must_be_positive_integer, valueName));
            }

            if (zeroAllowed && iValue < 0)
            {
                throw new ProviderException(String.Format(Resources.Value_must_be_non_negative_integer, valueName));
            }

            if (!zeroAllowed && iValue <= 0)
            {
                throw new ProviderException(String.Format(Resources.Value_must_be_positive_integer, valueName));
            }

            if (maxValueAllowed > 0 && iValue > maxValueAllowed)
            {
                throw new ProviderException(String.Format(Resources.Value_too_big, valueName, maxValueAllowed.ToString(CultureInfo.InvariantCulture)));
            }

            return iValue;
        }

        internal static void CheckSchemaVersion(ProviderBase provider, MembershipDB db, string[] features, string version, ref int schemaVersionCheck)
        {
            if (db == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (features == null)
            {
                throw new ArgumentNullException("features");
            }

            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            if (schemaVersionCheck == -1)
            {
                throw new ProviderException(String.Format(Resources.Provider_Schema_Version_Not_Match, provider.ToString(), version));
            }
            else if (schemaVersionCheck == 0)
            {
                lock (provider)
                {
                    if (schemaVersionCheck == -1)
                    {
                        throw new ProviderException(String.Format(Resources.Provider_Schema_Version_Not_Match, provider.ToString(), version));
                    }
                    else if (schemaVersionCheck == 0)
                    {
                        foreach (string feature in features)
                        {
                            var result = (from x in db.aspnet_SchemaVersions
                                          where x.Feature.ToLower() == feature.ToLower() && x.CompatibleSchemaVersion == version
                                          select x);
                            if (result.Count() == 0)
                            {
                                schemaVersionCheck = -1;
                                throw new ProviderException(String.Format(Resources.Provider_Schema_Version_Not_Match, provider.ToString(), version));
                            }
                        }
                        schemaVersionCheck = 1;
                    }
                }
            }
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
                return;
            }

            param = param.Trim();
            if (checkIfEmpty && param.Length < 1)
            {
                throw new ArgumentException(String.Format(Resources.Parameter_can_not_be_empty, paramName), paramName);
            }

            if (maxSize > 0 && param.Length > maxSize)
            {
                throw new ArgumentException(String.Format(Resources.Parameter_too_long, paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
            }

            if (checkForCommas && param.Contains(","))
            {
                throw new ArgumentException(String.Format(Resources.Parameter_can_not_contain_comma, paramName), paramName);
            }
        }

        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (param.Length < 1)
            {
                throw new ArgumentException(String.Format(Resources.Parameter_array_empty, paramName), paramName);
            }

            Hashtable values = new Hashtable(param.Length);
            for (int i = param.Length - 1; i >= 0; i--)
            {
                EfProviderUtil.CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize, paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if (values.Contains(param[i]))
                {
                    throw new ArgumentException(String.Format(Resources.Parameter_duplicate_array_element, paramName), paramName);
                }
                else
                {
                    values.Add(param[i], param[i]);
                }
            }
        }

        internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            string sValue = config[valueName];
            if (sValue == null)
            {
                return defaultValue;
            }

            bool result;
            if (bool.TryParse(sValue, out result))
            {
                return result;
            }
            else
            {
                throw new ProviderException(String.Format(Resources.Value_must_be_boolean, valueName));
            }
        }

        internal static string GetDefaultAppName()
        {
            try
            {
                string appName = HostingEnvironment.ApplicationVirtualPath;
                if (String.IsNullOrWhiteSpace(appName))
                {

                    appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;

                    int indexOfDot = appName.IndexOf('.');
                    if (indexOfDot != -1)
                    {
                        appName = appName.Remove(indexOfDot);
                    }
                }

                if (String.IsNullOrWhiteSpace(appName))
                {
                    return "/";
                }
                else
                {
                    return appName;
                }
            }
            catch
            {
                return "/";
            }
        }

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }

            param = param.Trim();
            if ((checkIfEmpty && param.Length < 1) ||
                (maxSize > 0 && param.Length > maxSize) ||
                (checkForCommas && param.Contains(",")))
            {
                return false;
            }

            return true;
        }
    }
}

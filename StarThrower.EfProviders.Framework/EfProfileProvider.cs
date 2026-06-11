// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Profile;
using System.Diagnostics;

namespace StarThrower.EfProviders.Framework
{
    public class EfProfileProvider : ProfileProvider
    {
        #region [ Private Instance Variables ]

        private string _applicationName = "/";

        #endregion


        #region [ Public Properties ]

        /// <summary>
        /// The application name that is stored with each profile. The profile provider uses the 
        /// application name to store profile information separately for each application. This 
        /// enables multiple ASP.NET applications to use the same data source without a conflict 
        /// if the same user name is created in different applications. Alternatively, multiple 
        /// ASP.NET applications can share a profile data source by specifying the same 
        /// application name.
        /// </summary>
        public override string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }

                if (value.Length > 256)
                {
                    throw new ProviderException(String.Format(Resources.Provider_application_name_too_long));
                }
                _applicationName = value;
            }
        }

        #endregion


        #region [ Construction & Instantiation ]

        /// <summary>
        /// Takes as input the name of the provider instance and a NameValueCollection of 
        /// configuration settings. Used to set options and property values for the provider 
        /// instance, including implementation-specific values and options specified in the 
        /// machine configuration or Web.config file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (String.IsNullOrWhiteSpace(name))
            {
                name = "EfProfileProvider";
            }
            if (String.IsNullOrWhiteSpace(config["description"]))
            {
                config.Remove("description");
                config.Add("description", String.Format(Resources.MembershipSqlProvider_description));
            }
            base.Initialize(name, config);


            _applicationName = config["applicationName"];
            if (String.IsNullOrWhiteSpace(_applicationName))
            {
                _applicationName = EfProviderUtil.GetDefaultAppName();
            }

            if (_applicationName.Length > 256)
            {
                throw new ProviderException(String.Format(Resources.Provider_application_name_too_long));
            }



            config.Remove("applicationName");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrWhiteSpace(attribUnrecognized))
                {
                    throw new ProviderException(String.Format(Resources.Provider_unrecognized_attribute, attribUnrecognized));
                }
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Takes as input a ProfileAuthenticationOption value and a DateTime object and deletes 
        /// from the data source all profile information and property values where the last 
        /// activity date is less than or equal to the specified date and time and where the 
        /// application name matches the ApplicationName property value. The 
        /// ProfileAuthenticationOption parameter specifies whether only anonymous profiles, 
        /// only authenticated profiles, or all profiles are to be deleted.
        /// 
        /// If your data source supports transactions, it is recommended that you include all 
        /// delete operations in a transaction and roll back the transaction and throw an 
        /// exception if any delete operation fails.
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <returns></returns>
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int numDeleted = 0;
            using (MembershipDB db = new MembershipDB())
            {
                IQueryable<Guid> ids = null;
                switch (authenticationOption)
                {
                    case ProfileAuthenticationOption.Anonymous:
                        ids = from x in db.aspnet_Users
                              where x.LastActivityDate <= userInactiveSinceDate && x.IsAnonymous
                              select x.UserId;
                        break;
                    case ProfileAuthenticationOption.Authenticated:
                        ids = from x in db.aspnet_Users
                              where x.LastActivityDate <= userInactiveSinceDate && !x.IsAnonymous
                              select x.UserId;
                        break;
                    default:
                        ids = from x in db.aspnet_Users
                              where x.LastActivityDate <= userInactiveSinceDate
                              select x.UserId;
                        break;
                }
                foreach (Guid id in ids)
                {
                    numDeleted += DeleteProfile(id, db);
                }
            }
            return numDeleted;
        }

        /// <summary>
        /// Takes as input a string array of user names and deletes from the data source all profile 
        /// information and property values for the specified names where the application name 
        /// matches the ApplicationName property value.
        ///
        /// If your data source supports transactions, it is recommended that you include all 
        /// delete operations in a transaction and that you roll back the transaction and 
        /// throw an exception if any delete operation fails.
        /// </summary>
        /// <param name="userNames"></param>
        /// <returns></returns>
        public override int DeleteProfiles(string[] userNames)
        {
            int numDeleted = 0;
            using (MembershipDB db = new MembershipDB())
            {
                foreach (string user in userNames)
                {
                    Guid id = Guid.Empty;
                    numDeleted += DeleteProfile(id, db);
                }
            }
            return numDeleted;
        }

        /// <summary>
        /// Takes as input a collection of ProfileInfo objects and deletes from the data source 
        /// all profile information and property values for each profile where the application 
        /// name matches the ApplicationName property value.
        ///
        /// If your data source supports transactions, it is recommended that you include all 
        /// delete operations in a transaction and roll back the transaction and throw an 
        /// exception if any delete operation fails.
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            int numDeleted = 0;
            using (MembershipDB db = new MembershipDB())
            {
                foreach (ProfileInfo p in profiles)
                {
                    Guid id = Guid.Empty;
                    numDeleted += DeleteProfile(id, db);
                }
            }
            return numDeleted;
        }

        /// <summary>
        /// Takes as input a ProfileAuthenticationOption value, a string containing a user name, 
        /// a DateTime object, an integer that specifies the page index, an integer that specifies 
        /// the page size, and a reference to an integer that will be set to the total count of 
        /// profiles. Returns a ProfileInfoCollection that contains ProfileInfo objects for all 
        /// profiles in the data source where the user name matches the specified user name, where 
        /// the last activity date is less than or equal to the specified DateTime, and where the 
        /// application name matches the ApplicationName property value. The ProfileAuthenticationOption 
        /// parameter specifies whether only anonymous profiles, only authenticated profiles, or all 
        /// profiles are to be returned.
        /// 
        /// If your data source supports additional search capabilities, such as wildcard characters, 
        /// you can provide more extensive search capabilities for user names.
        /// 
        /// The results returned by the FindInactiveProfilesByUserName method are constrained by 
        /// the page index and page size values. The page size value specifies the maximum number 
        /// of ProfileInfo objects to return in the ProfileInfoCollection. The page index value 
        /// specifies which page of results to return, where 1 identifies the first page. The 
        /// parameter for total records is an out parameter (you can use ByRef in Visual Basic) 
        /// that is set to the total number of profiles. For example, if the data store contains 
        /// 13 profiles for the application and the page index value is 2 with a page size of 5, 
        /// the ProfileInfoCollection returned contains the sixth through the tenth profiles. The 
        /// total records value is set to 13 when the method returns.
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userNameToMatch"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Page index must 0 or greater.");
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0.");

            return GetProfileInfo(authenticationOption, userNameToMatch, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Takes as input a ProfileAuthenticationOption value, a string containing a user name, 
        /// an integer that specifies the page index, an integer that specifies the page size, 
        /// and a reference to an integer that will be set to the total count of profiles. 
        /// Returns a ProfileInfoCollection that contains ProfileInfo objects for all profiles 
        /// in the data source where the user name matches the specified user name and where the 
        /// application name matches the ApplicationName property value. The ProfileAuthenticationOption 
        /// parameter specifies whether only anonymous profiles, only authenticated profiles, or all 
        /// profiles are to be returned.
        ///
        /// If your data source supports additional search capabilities, such as wildcard 
        /// characters, you can provide more extensive search capabilities for user names.
        /// 
        /// The results returned by the FindProfilesByUserName method are constrained by the 
        /// page index and page size values. The page size value specifies the maximum number 
        /// of ProfileInfo objects to return in the ProfileInfoCollection. The page index value 
        /// specifies which page of results to return, where 1 identifies the first page. The
        /// parameter for total records is an out parameter (you can use ByRef in Visual Basic) 
        /// that is set to the total number of profiles. For example, if the data store contains 
        /// 13 profiles for the application and the page index value is 2 with a page size of 5, 
        /// the ProfileInfoCollection returned contains the sixth through the tenth profiles. The 
        /// total records value is set to 13 when the method returns.
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userNameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Page index must 0 or greater.");
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0.");

            return GetProfileInfo(authenticationOption, userNameToMatch, null, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Takes as input a ProfileAuthenticationOption value, a DateTime object, an integer 
        /// that specifies the page index, an integer that specifies the page size, and a 
        /// reference to an integer that will be set to the total count of profiles. Returns 
        /// a ProfileInfoCollection that contains ProfileInfo objects for all profiles in the 
        /// data source where the last activity date is less than or equal to the specified 
        /// DateTime and where the application name matches the ApplicationName property value. 
        /// The ProfileAuthenticationOption parameter specifies whether only anonymous profiles, 
        /// only authenticated profiles, or all profiles are to be returned.
        /// 
        /// The results returned by the GetAllInactiveProfiles method are constrained by the 
        /// page index and page size values. The page size value specifies the maximum number 
        /// of ProfileInfo objects to return in the ProfileInfoCollection. The page index value 
        /// specifies which page of results to return, where 1 identifies the first page. The 
        /// parameter for total records is an out parameter (you can use ByRef in Visual Basic) 
        /// that is set to the total number of profiles. For example, if the data store contains 
        /// 13 profiles for the application and the page index value is 2 with a page size of 5, 
        /// the ProfileInfoCollection returned contains the sixth through the tenth profiles. 
        /// The total records value is set to 13 when the method returns
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Page index must 0 or greater.");
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0.");

            return GetProfileInfo(authenticationOption, null, userInactiveSinceDate, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Takes as input a ProfileAuthenticationOption value, an integer that specifies the 
        /// page index, an integer that specifies the page size, and a reference to an integer 
        /// that will be set to the total count of profiles. Returns a ProfileInfoCollection that 
        /// contains ProfileInfo objects for all profiles in the data source where the application 
        /// name matches the ApplicationName property value. The ProfileAuthenticationOption parameter 
        /// specifies whether only anonymous profiles, only authenticated profiles, or all profiles 
        /// are to be returned.
        /// 
        /// The results returned by the GetAllProfiles method are constrained by the page index and 
        /// page size values. The page size value specifies the maximum number of ProfileInfo objects 
        /// to return in the ProfileInfoCollection. The page index value specifies which page of 
        /// results to return, where 1 identifies the first page. The parameter for total records is 
        /// an out parameter (you can use ByRef in Visual Basic) that is set to the total number of 
        /// profiles. For example, if the data store contains 13 profiles for the application and the 
        /// page index value is 6 with a page size of 5, the ProfileInfoCollection returned contains 
        /// the sixth through the tenth profiles. The total records value is set to 13 when the 
        /// method returns.
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Page index must 0 or greater.");
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0.");

            return GetProfileInfo(authenticationOption, null, null, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Takes as input a ProfileAuthenticationOption value and a DateTime object and returns 
        /// a count of all profiles in the data source where the last activity date is less than 
        /// or equal to the specified DateTime and where the application name matches the 
        /// ApplicationName property value. The ProfileAuthenticationOption parameter specifies 
        /// whether only anonymous profiles, only authenticated profiles, or all profiles are 
        /// to be counted.
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <returns></returns>
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int inactiveProfiles = 0;
            ProfileInfoCollection profiles = GetProfileInfo(authenticationOption, null, userInactiveSinceDate, 0, 0, out inactiveProfiles);
            return inactiveProfiles;
        }

        /// <summary>
        /// Takes as input a SettingsContext and a SettingsPropertyCollection object.
        ///
        /// The SettingsContext provides information about the user. You can use the information 
        /// as a primary key to retrieve profile property information for the user. Use the 
        /// SettingsContext object to get the user name and whether the user is authenticated or anonymous.
        ///
        /// The SettingsPropertyCollection contains a collection of SettingsProperty objects. Each 
        /// SettingsProperty object provides the name and type of the property as well as additional 
        /// information such as the default value for the property and whether the property is 
        /// read-only. The GetPropertyValues method populates a SettingsPropertyValueCollection with 
        /// SettingsPropertyValue objects based on the SettingsProperty objects provided as input. 
        /// The values from the data source for the specified user are assigned to the PropertyValue 
        /// properties for each SettingsPropertyValue object and the entire collection is returned.
        ///
        /// Calling the method also updates the LastActivityDate value for the specified user 
        /// profile to the current date and time.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            string userName = context["UserName"].ToString().Trim().ToLowerInvariant();
            bool isAuthenticated = (bool)context["IsAuthenticated"];

            SettingsPropertyValueCollection result = new SettingsPropertyValueCollection();

            using (MembershipDB db = new MembershipDB())
            {
                aspnet_Profile p = (from x in db.aspnet_Profile
                                    where x.UserId == (from y in db.aspnet_Users
                                                       where y.LoweredUserName == userName
                                                       select y.UserId).FirstOrDefault()
                                    select x).FirstOrDefault();
                if (p != null)
                {
                    result = ParseProfile(p);
                }

                db.SaveChanges();

                UpdateActivityDates(userName, isAuthenticated, true, db);

                db.SaveChanges();
            }

            return result;
        }

        /// <summary>
        /// Takes as input a SettingsContext and a SettingsPropertyValueCollection object.
        /// 
        /// The SettingsContext provides information about the user. You can use the information 
        /// as a primary key to retrieve profile property information for the user. Use the 
        /// SettingsContext object to get the user name and whether the user is authenticated or anonymous.
        /// 
        /// The SettingsPropertyValueCollection contains a collection of SettingsPropertyValue 
        /// objects. Each SettingsPropertyValue object provides the name, type, and value of 
        /// the property as well as additional information such as the default value for the 
        /// property and whether the property is read-only. The SetPropertyValues method updates 
        /// the profile property values in the data source for the specified user.
        /// 
        /// Calling the method also updates the LastActivityDate and LastUpdatedDate values 
        /// for the specified user profile to the current date and time
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            string userName = context["UserName"].ToString().Trim().ToLowerInvariant();
            bool isAuthenticated = (bool)context["IsAuthenticated"];

            using (MembershipDB db = new MembershipDB())
            {
                Guid id = (from x in db.aspnet_Users
                           where x.LoweredUserName == userName
                           select x.UserId).FirstOrDefault();
                var p = (from x in db.aspnet_Profile
                         where x.UserId == id
                         select x).FirstOrDefault();
                if (p == null)
                {
                    p = CreateProfile(id, collection);
                    db.aspnet_Profile.AddObject(p);
                }
                else
                {
                    UpdateProfile(p, collection);
                }

                db.SaveChanges();

                UpdateActivityDates(userName, isAuthenticated, false, db);

                db.SaveChanges();
            }
        }

        #endregion


        #region [ Private Methods ]

        #region utilities

        private ProfileInfoCollection GetProfileInfo(ProfileAuthenticationOption authenticationOption, string userNameToMatch, object userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        private int DeleteProfile(Guid userId, MembershipDB db)
        {
            var profile = (from x in db.aspnet_Profile
                           where x.UserId == userId
                           select x).FirstOrDefault();
            if (profile != null)
            {
                db.aspnet_Profile.DeleteObject(profile);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private void UpdateActivityDates(string userName, bool isAuthenticated, bool activityOnly, MembershipDB db)
        {
            DateTime activityDate = DateTime.UtcNow;

            if (activityOnly)
            {
                var users = from x in db.aspnet_Users
                            where x.UserName == userName && x.IsAnonymous == !isAuthenticated
                            select x;
                foreach (var user in users)
                {
                    user.LastActivityDate = activityDate;
                }
            }
            else
            {
                var users = from x in db.aspnet_Users
                            where x.UserName == userName && x.IsAnonymous == !isAuthenticated
                            select x;
                foreach (var user in users)
                {
                    user.LastActivityDate = activityDate;
                    var profile = (from x in db.aspnet_Profile
                                   where x.UserId == user.UserId
                                   select x).FirstOrDefault();
                    profile.LastUpdatedDate = activityDate;
                }
            }
        }

        #endregion

        #region profile parsing

        private SettingsPropertyValueCollection ParseProfile(aspnet_Profile p)
        {
            SettingsPropertyValueCollection result = new SettingsPropertyValueCollection();

            Dictionary<string, ProfileItem> infos = ParseProfileItemsInfo(p.PropertyNames);
            foreach (string key in infos.Keys)
            {
                ProfileItem info = infos[key];

                switch (info.Storage)
                {
                    case "B":
                        SettingsProperty bProp = new SettingsProperty(info.Name, Type.GetType(info.PropertyType), null, false, info.DefaultValue, info.SerializeAs, new SettingsAttributeDictionary(), true, true);
                        SettingsPropertyValue bVal = new SettingsPropertyValue(bProp);
                        bVal.PropertyValue = GetBinaryPropertyValue(info, p);
                        result.Add(bVal);
                        break;
                    case "S":
                        SettingsProperty tProp = new SettingsProperty(info.Name, Type.GetType(info.PropertyType), null, false, info.DefaultValue, info.SerializeAs, new SettingsAttributeDictionary(), true, true);
                        SettingsPropertyValue tVal = new SettingsPropertyValue(tProp);
                        tVal.PropertyValue = GetTextPropertyValue(info, p);
                        result.Add(tVal);
                        break;
                }

            }

            return result;
        }

        private Dictionary<string, ProfileItem> ParseProfileItemsInfo(string propertyNames)
        {
            Dictionary<string, ProfileItem> result = new Dictionary<string, ProfileItem>();

            string[] items = propertyNames.Split(new char[] { ':' }, StringSplitOptions.None);

            string name = String.Empty;
            string storage = String.Empty;
            int startIndex = 0;
            int length = 0;

            int n = 0;
            for (int i = 0; i < items.Length; i++)
            {
                switch (n)
                {
                    case 0:
                        name = items[i];
                        n++;
                        break;
                    case 1:
                        storage = items[i];
                        n++;
                        break;
                    case 2:
                        startIndex = Int32.Parse(items[i]);
                        n++;
                        break;
                    case 3:
                        length = Int32.Parse(items[i]);
                        ProfileItem itm = new ProfileItem();
                        itm.Name = name;
                        itm.Storage = storage;
                        switch (storage)
                        {
                            case "S":
                                itm.PropertyType = typeof(Object).AssemblyQualifiedName; //TODO: anyway to determine the Type information here? and, if so, is it even really necessary?  for now, just call it an Object
                                itm.DefaultValue = String.Empty;
                                itm.SerializeAs = SettingsSerializeAs.String;
                                break;
                            case "B":
                                itm.PropertyType = typeof(Object).AssemblyQualifiedName; //TODO: anyway to determine the Type information here? and, if so, is it even really necessary?  for now, just call it an Object
                                itm.DefaultValue = new byte[0];
                                itm.SerializeAs = SettingsSerializeAs.Binary;
                                break;
                        }
                        itm.StartIndex = startIndex;
                        itm.Length = length;
                        result.Add(itm.Name, itm);
                        n = 0;
                        name = String.Empty;
                        storage = String.Empty;
                        startIndex = 0;
                        length = 0;
                        break;
                }
            }

            return result;
        }

        private string GetTextPropertyValue(ProfileItem info, aspnet_Profile p)
        {
            return p.PropertyValuesString.Substring(info.StartIndex, info.Length);
        }

        private byte[] GetBinaryPropertyValue(ProfileItem info, aspnet_Profile p)
        {
            //return p.PropertyValuesBinary.AsSpan(info.StartIndex, info.Length).ToArray();
            return ByteSubstring(p.PropertyValuesBinary, info.StartIndex, info.Length, true);
        }

        /// <summary>
        /// Retrieves a subset of bytes from a byte array. The subset starts at a specified position and has a specified length.
        /// 
        /// NOTE: This was pulled out of StarThrower.ByteUtilities because the functionality is needed in this class but we don't want to take a dependency on the entire StarThrower.ByteUtilities assembly just for this one method.  This method is not intended to be a general purpose utility method and is only intended to be used in the context of parsing profile property values from the aspnet_Profile table, which is why it has the additional parameter for whether or not to pad the remaining space with nulls.
        /// </summary>
        /// <param name="source">The original array of bytes.</param>
        /// <param name="startIndex">The index of the start of the subset.</param>
        /// <param name="length">The number of bytes in the subset.</param>
        /// <param name="trimWithNulls">Whether or not to pad the space remaining after startIndex + length with nulls</param>
        /// <returns>A byte array equivalent to the subset of length length that begins at startIndex in the original byte array, or an empty byte array if startIndex is equal to the length of the original byte array and length is zero.</returns>
        /// <exception cref="ArgumentNullException">Thrown if source is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if startIndex is less than zero or greater than source.Length - 1.  Also thrown if startIndex + length exceeds the length of the array.</exception>
        public static byte[] ByteSubstring(byte[] source, long startIndex, long length, bool trimWithNulls)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (startIndex < 0 || startIndex >= source.Length) throw new ArgumentOutOfRangeException("startIndex");
            if ((startIndex + length) > source.Length) throw new ArgumentOutOfRangeException("length");

            bool isNullTerminated = false;

            try
            {
                byte[] result = new byte[length];

                for (long i = 0; i < length; i++)
                {
                    if (!isNullTerminated)
                    {
                        byte b = source[startIndex + i];
                        result[i] = b;
                        if (trimWithNulls && b == 0)
                        {
                            isNullTerminated = true;
                        }
                    }
                    else
                    {
                        result[i] = 0;
                    }
                }

                return result;
            }
            catch //(Exception ex)
            {
                //Logger.ReportError(ErrorPolicy.Internal, "Bytes.ByteSubstring(byte[], long, long)", ex);
                throw;
            }
        }

        #endregion

        #region profile creation

        private aspnet_Profile CreateProfile(Guid userId, SettingsPropertyValueCollection items)
        {
            aspnet_Profile result = new aspnet_Profile();
            result.UserId = userId;
            UpdateProfile(result, items);
            return result;
        }

        private void UpdateProfile(aspnet_Profile profile, SettingsPropertyValueCollection items)
        {
            StringBuilder propertyNames = new StringBuilder(String.Empty);
            StringBuilder propertyValuesString = new StringBuilder(String.Empty);

            int n = 0;
            int startIndex = 0;
            int textStartIndex = 0;
            int binaryStartIndex = 0;
            int binaryLength = 0;
            Collection<byte[]> binaries = new Collection<byte[]>();
            foreach (SettingsPropertyValue pval in items)
            {
                if (n == 0)
                {
                    propertyNames.Append(pval.Name);
                    n = 1;
                }
                else
                {
                    propertyNames.Append(":" + pval.Name);
                }
                object val = pval.SerializedValue;
                if (val is String)
                {
                    string sVal = (String)val;
                    startIndex = textStartIndex;
                    propertyNames.Append(":S");
                    propertyNames.Append(":" + startIndex.ToString());
                    propertyNames.Append(":" + sVal.Length.ToString());
                    textStartIndex += sVal.Length;
                    propertyValuesString.Append(sVal);
                }
                else
                {
                    byte[] bVal = (byte[])val;
                    binaries.Add(bVal);
                    startIndex = binaryStartIndex;
                    propertyNames.Append(":B");
                    propertyNames.Append(":" + startIndex.ToString());
                    propertyNames.Append(":" + bVal.Length.ToString());
                    binaryStartIndex += bVal.Length;
                    binaryLength += bVal.Length;
                }
            }

            byte[] propertyValuesBinary = new byte[binaryLength];
            int q = 0;
            for (int i = 0; i < binaries.Count; i++)
            {
                for (int j = 0; j < binaries[i].Length; j++)
                {
                    propertyValuesBinary[q] = binaries[i][j];
                    q++;
                }
            }

            profile.PropertyNames = propertyNames.ToString();
            profile.PropertyValuesString = propertyValuesString.ToString();
            profile.PropertyValuesBinary = propertyValuesBinary;
            profile.LastUpdatedDate = DateTime.UtcNow;
        }

        private string CreateProfileItemsInfoString(Dictionary<string, ProfileItem> items)
        {
            StringBuilder result = new StringBuilder(String.Empty);

            foreach (string key in items.Keys)
            {
                ProfileItem itm = items[key];
                string name = itm.Name;
                string Storage = itm.Storage;
                int startIndex = itm.StartIndex;
                int length = itm.Length;
                if (result.Length > 0)
                {
                    result.Append(":" + name + ":" + Storage + ":" + startIndex.ToString() + ":" + length.ToString());
                }
                else
                {
                    result.Append(name + ":" + Storage + ":" + startIndex.ToString() + ":" + length.ToString());
                }
            }

            return result.ToString();
        }

        #endregion

        #endregion
    }
}

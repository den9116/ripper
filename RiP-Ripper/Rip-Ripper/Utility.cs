//////////////////////////////////////////////////////////////////////////
// Code Named: RiP-Ripper
// Function  : Extracts Images posted on RiP forums and attempts to fetch
//			   them to disk.
//
// This software is licensed under the MIT license. See license.txt for
// details.
// 
// Copyright (c) The Watcher
// Partial Rights Reserved.
// 
//////////////////////////////////////////////////////////////////////////
// This file is part of the RiP Ripper project base.

// This page is probably the biggest mess I've ever managed to conceive.
// It's so nasty that I dare not even comment much.
// But as the file name says, it's just a bunch of non-dependant classes
// and funcs for doing nifty little things.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RiPRipper
{
    using System.Linq;

    using RiPRipper.Objects;

    /// <summary>
	/// Summary description for Utility.
	/// </summary>
	public class Utility
	{
        static readonly Configuration Conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        static readonly AppSettingsSection App = (AppSettingsSection)Conf.Sections["appSettings"];


	    /// <summary>
        /// Attempts to extract hotlinked and thumb->FullScale images.
        /// </summary>
        public static List< ImageInfo > ExtractImages( string strDump )
		{ 

			List< ImageInfo > rtnList = new List< ImageInfo >();
	        Hashtable rtnHashChk = new Hashtable();

	        ImageInfo newPicPool;

            try
            {
                DataSet ds = new DataSet();

                ds.ReadXml(new StringReader(strDump));

                foreach (DataRow row in ds.Tables["Image"].Rows)
                {
                    newPicPool = new ImageInfo {ImageUrl = row["main_url"].ToString()};

                    newPicPool.ImageUrl = Regex.Replace(newPicPool.ImageUrl, @"""", string.Empty);

                    //////////////////////////////////////////////////////////////////////////
                    if (IsImageNoneSense(newPicPool.ImageUrl))
                    {
                        continue;
                    }

                    newPicPool.ImageUrl = ReplaceHexWithAscii(newPicPool.ImageUrl);

                    // Remove anonym.to from Link if exists
                    if (newPicPool.ImageUrl.Contains("anonym.to"))
                    {
                        newPicPool.ImageUrl = newPicPool.ImageUrl.Replace("http://www.anonym.to/?", string.Empty);
                    }

                    if (rtnHashChk.Contains(newPicPool.ImageUrl))
                    {
                        continue;
                    }

                    rtnList.Add(newPicPool);
                    rtnHashChk.Add(newPicPool.ImageUrl, "OK");
                    //////////////////////////////////////////////////////////////////////////


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            return rtnList;
		}

        /// <summary>
        /// Extracts links leading to other threads and posts for indicies crawling.
        /// </summary>
        /// 
        public static List< ImageInfo > ExtractRiPUrls(string strDump)
		{   

			List< ImageInfo > rtnList = new List< ImageInfo >();
            Hashtable rtnHashChk = new Hashtable();

            ImageInfo newPicPool;

			try
            {
                DataSet ds = new DataSet();

                ds.ReadXml(new StringReader(strDump));

                foreach (DataRow row in ds.Tables["Image"].Rows)
                {
                    newPicPool = new ImageInfo {ImageUrl = row["main_url"].ToString()};

                    newPicPool.ImageUrl = ReplaceHexWithAscii(newPicPool.ImageUrl);

                    if (rtnHashChk.Contains(newPicPool.ImageUrl))
                    {
                        continue;
                    }

                    rtnList.Add(newPicPool);
                    rtnHashChk.Add(newPicPool.ImageUrl, "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

			return rtnList;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strDump"></param>
        /// <returns></returns>
        public static List<ImageInfo> ExtractThreadtoPosts(string strDump)
        {
            List<ImageInfo> rtnList = new List<ImageInfo>();
            Hashtable rtnHashChk = new Hashtable();

            ImageInfo newPicPool;

            try
            {
                DataSet ds = new DataSet();

                ds.ReadXml(new StringReader(strDump));

                foreach (DataRow row in ds.Tables["post"].Rows.Cast<DataRow>().Where(row => row["id"] != null))
                {
                    newPicPool = new ImageInfo {ImageUrl = row["id"].ToString()};

                    newPicPool.ImageUrl = ReplaceHexWithAscii(newPicPool.ImageUrl);

                    if (rtnHashChk.Contains(newPicPool.ImageUrl))
                    {
                        continue;
                    }

                    rtnList.Add(newPicPool);
                    rtnHashChk.Add(newPicPool.ImageUrl, "OK");
                }
            }
            catch (Exception ex)
            {
                SaveOnCrash(strDump, ex.StackTrace, null);
            }

            return rtnList;
        }
        /// <summary>
        /// This function allows or disallows the inclusion of an image for fetching.
        /// returning true DISALLOWS the image from inclusion...
        /// </summary>
        /// <param name="szImgPth"></param>
        /// <returns></returns>
		public static bool IsImageNoneSense(string szImgPth)
        {
            return szImgPth.ToLower().IndexOf("rip-pro") >= 0 || (szImgPth.ToLower().IndexOf("rip.bamva") >= 0 ||
                                                                  (szImgPth.Contains(@"Smilies") ||
                                                                   szImgPth.Contains(@"emoticons")));
        }


	    /// <summary>
        /// Encrypts a password using MD5.
        /// not my code in this func., but falls under public domain.
        /// Author unknown. But Thanks to the author none the less.
        /// </summary>
        public static string EncodePassword(string sOriginalPass)
		{ 

			//Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
			MD5 md5 = new MD5CryptoServiceProvider();

            byte[] originalBytes = Encoding.Default.GetBytes(sOriginalPass);
			byte[] encodedBytes = md5.ComputeHash(originalBytes);

			//Convert encoded bytes back to a 'readable' string
			return BitConverter.ToString(encodedBytes);
		}
        /// <summary>
        /// Checks if object is a Number
        /// </summary>
        /// <param name="valueToCheck"></param>
        /// <returns></returns>
		public static bool IsNumeric(object valueToCheck)
		{
			double dummy;
			string inputValue = Convert.ToString(valueToCheck);

			bool numeric = double.TryParse( inputValue , System.Globalization.NumberStyles.Any , null , out dummy);

			return numeric;
		}
        /// <summary>
        /// It's essential to give files legal names. Otherwise the Win32API 
        /// sends back a bucket full of cow dung.
        /// </summary>
        /// <param name="sString">String to check</param>
        /// <returns></returns>
		public static string RemoveIllegalCharecters(string sString)
		{
			string sNewComposed = sString;

            sNewComposed = sNewComposed.Replace("&amp;amp;", "&");
            sNewComposed = sNewComposed.Replace("\\", string.Empty);
            sNewComposed = sNewComposed.Replace("/", "-");
            sNewComposed = sNewComposed.Replace("*", "+");
            sNewComposed = sNewComposed.Replace("?", string.Empty);
            sNewComposed = sNewComposed.Replace("!", string.Empty);
            sNewComposed = sNewComposed.Replace("\"", "'");
            sNewComposed = sNewComposed.Replace("<", "(");
            sNewComposed = sNewComposed.Replace(">", ")");
            sNewComposed = sNewComposed.Replace("|", "!");
            sNewComposed = sNewComposed.Replace(":", ";");
            sNewComposed = sNewComposed.Replace("&amp;", "&");
            sNewComposed = sNewComposed.Replace("&quot;", "''");
            sNewComposed = sNewComposed.Replace("&apos;", "'");
            sNewComposed = sNewComposed.Replace("&lt;", string.Empty);
            sNewComposed = sNewComposed.Replace("&gt;", string.Empty);
            sNewComposed = sNewComposed.Replace("�", "e");
            sNewComposed = sNewComposed.Replace("\t", string.Empty);
            sNewComposed = sNewComposed.Replace("@", "at");
            sNewComposed = sNewComposed.Replace("\r", string.Empty);
            sNewComposed = sNewComposed.Replace("\n", string.Empty);

            return sNewComposed;
		}
        /// <summary>
        /// Although these are not hex, but rather html codes for special characters
        /// </summary>
        /// <param name="sURL">String to check</param>
        /// <returns></returns>
		public static string ReplaceHexWithAscii(string sURL)
		{
			string sString = sURL;
			if (sString == null)
				return string.Empty;

            sString = sString.Replace("&amp;amp;", "&");
			sString = sString.Replace("&amp;", "&");
            sString = sString.Replace("&quot;", "''");
            sString = sString.Replace("&lt;", string.Empty);
            sString = sString.Replace("&gt;", string.Empty);
            sString = sString.Replace("�", "e");
            sString = sString.Replace("\t", string.Empty);
            sString = sString.Replace("@", "at");

			return sString;
		}
        /// <summary>
        /// This func checks to see if a file already exists at destination
        /// thats of the same name. If so, it incrementally adds numerical
        /// values prior to the image extension until the new file path doesn't
        /// already have a file there.
        /// </summary>
        /// <param name="sPath">Image path</param>
        /// <returns></returns>
		public static string GetSuitableName(string sPath)
		{
			string newAlteredPath = sPath;
            int iRenameCnt = 1;
			string sbegining = newAlteredPath.Substring(0, newAlteredPath.LastIndexOf( "." )  );
			string sEnd = newAlteredPath.Substring(newAlteredPath.LastIndexOf( "." ));
			
			while (File.Exists( newAlteredPath ))
			{
				newAlteredPath = sbegining + "_" + iRenameCnt + sEnd;
				iRenameCnt++;
			}
			
			return newAlteredPath;
		}
        /// <summary>
        /// Loads a Setting from the App.config
        /// </summary>
        /// <param name="sKey">Setting name</param>
        /// <returns>Setting value</returns>
        public static string LoadSetting(string sKey)
        {
            string setting = App.Settings[sKey].Value;

            return setting;
        }
        /// <summary>
        /// Saves a setting to the App.config
        /// </summary>
        /// <param name="sKey">Setting Name</param>
        /// <param name="sValue">Setting Value</param>
        public static void SaveSetting(string sKey, string sValue)
        {
            if (App.Settings[sKey] != null)
            {
                App.Settings.Remove(sKey);
            }

            App.Settings.Add(sKey, sValue);

            App.SectionInformation.ForceSave = true;
            Conf.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");  
        }
        /// <summary>
        /// Delete a Setting
        /// </summary>
        /// <param name="sKey">Setting Name</param>
        public static void DeleteSetting(string sKey)
        {
            if (App.Settings[sKey] != null)
            {
                App.Settings.Remove(sKey);
            }

            App.SectionInformation.ForceSave = true;
            Conf.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
        /// <summary>
        /// Gets the Security Token for the Thank You Button
        /// </summary>
        /// <param name="sURL">URL of the Post</param>
        /// <returns>The Security Token</returns>
        public static string GetSToken(string sURL)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Referer: " + sURL);
            wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US; rv:1.7.10) Gecko/20050716 Firefox/1.0.6");
            wc.Headers.Add("Cookie: " + CookieManager.GetInstance().GetCookieString());
            string sSToken = wc.DownloadString(sURL);
            
            wc.Dispose();

            const string sStart = "var SECURITYTOKEN = \"";

            int iStartSrc = sSToken.IndexOf(sStart);

            if (iStartSrc < 0)
            {
                return null;
            }

            iStartSrc += sStart.Length;

            sSToken = sSToken.Substring(iStartSrc);

            sSToken = sSToken.Remove(sSToken.IndexOf("\";"));

            return sSToken;
        }
        /// <summary>
        /// Save all Jobs, and the current one which causes the crash to a CrashLog_...txt
        /// </summary>
        /// <param name="sExMessage">Exception Message</param>
        /// <param name="sStackTrace">Exception Stack Trace</param>
        /// <param name="mCurrentJob">Current Download Job</param>
        public static void SaveOnCrash(string sExMessage, string sStackTrace, JobInfo mCurrentJob)
        {
            const string sErrMessage = "An application error occurred. Please contact Admin (http://ripper.watchersnet.de/Feedback.aspx) " +
                                       "with the following information:";

            string sFile = string.Format("Crash_{0}.txt",
                                         DateTime.Now.ToString().Replace("/", string.Empty).Replace(":", string.Empty).Replace(".", string.Empty).
                                             Replace(" ", "_"));

            // Save Current Job and the Error to txt file
            FileStream file = new FileStream(Path.Combine(Application.StartupPath, sFile), FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine(sErrMessage);
            sw.Write(sw.NewLine);
            sw.Write(sExMessage);
            sw.Write(sw.NewLine);
            sw.Write(sw.NewLine);
            sw.WriteLine("Stack Trace:");
            sw.Write(sw.NewLine);
            sw.Write(sStackTrace);
            sw.Write(sw.NewLine);
            sw.Write(sw.NewLine);

            if (mCurrentJob != null)
            {
                sw.WriteLine("Current Job DUMP:");
                sw.Write(sw.NewLine);

                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw.WriteLine("<ArrayOfJobInfo xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                sw.WriteLine("  <JobInfo>");
                sw.WriteLine("    <sStorePath>{0}</sStorePath>", mCurrentJob.StorePath);
                sw.WriteLine("    <sTitle>{0}</sTitle>", mCurrentJob.Title);
                sw.WriteLine("    <sPostTitle>{0}</sPostTitle>", mCurrentJob.PostTitle);
                sw.WriteLine("    <sForumTitle>{0}</sForumTitle>", mCurrentJob.ForumTitle);
                sw.WriteLine("    <sURL>{0}</sURL>", mCurrentJob.URL);
                sw.WriteLine("    <sXMLPayLoad>{0}</sXMLPayLoad>", mCurrentJob.XMLPayLoad);
                sw.WriteLine("    <sImageCount>{0}</sImageCount>", mCurrentJob.ImageCount);
                sw.WriteLine("  </JobInfo>");
                sw.WriteLine("</ArrayOfJobInfo>");
            }

            sw.Close();
            file.Close();
        }
        /// <summary>
        /// Check the FilePath for Length because if its more then 260 characters long it will crash
        /// </summary>
        /// <param name="sFilePath">Folder Path to check</param>
        public static string CheckPathLength(string sFilePath)
        {
            if (sFilePath.Length > 260)
            {
                string sShortFilePath = sFilePath.Substring(sFilePath.LastIndexOf("\\") + 1);

                sFilePath = Path.Combine(MainForm.userSettings.sDownloadFolder, sShortFilePath);
            }

            return sFilePath;
        }
	}
}

using System;
using System.Net;
using System.Windows.Forms;
using VideoOS.Platform;
using VideoOS.Platform.Data;
using System.IO;
using System.Collections.Generic;
using VideoOS.Platform.SDK.Platform;

namespace VideoOs
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            string scaleNumb = args[0].Substring(0, args[0].IndexOf("!CV!"));
            string cameraName = args[0].Substring((args[0].IndexOf("!CV!") + 4)).Replace("!SPACE!", " ");

            getVideoOs(scaleNumb, cameraName);
        }

        public static string getVideoOs(string scaleNumb, string cameraName)
        {
            try
            {
                VideoOS.Platform.SDK.Environment.Initialize();          // General initialize.  Always required
                VideoOS.Platform.SDK.UI.Environment.Initialize();       // Initialize ActiveX references, e.g. usage of ImageViewerActiveX etc
                VideoOS.Platform.SDK.Export.Environment.Initialize();   // Initialize export references

                Uri uri = new Uri("http://localhost");
                //NetworkCredential nc = System.Net.CredentialCache.DefaultNetworkCredentials;
                CredentialCache cc = VideoOS.Platform.Login.Util.BuildCredentialCache(uri, "admin", "admin", "Basic");
                //CredentialCache cc = VideoOS.Platform.Login.Util.BuildCredentialCache(uri, "http://desktop-g7ejpeu:7563/\admin", "admin", "Negotiate");

                VideoOS.Platform.SDK.Environment.AddServer(uri, cc);

                try
                {
                    Guid IntegrationId = new Guid("1478D9D6-6168-4520-ACE3-4B795E6F3501");
                    const string IntegrationName = "Export Sample";
                    const string Version = "1.0";
                    const string ManufacturerName = "Sample Manufacturer";

                    VideoOS.Platform.SDK.Environment.Login(uri, IntegrationId, IntegrationName, Version, ManufacturerName, true);
                    VideoOS.Platform.SDK.Environment.LoadConfiguration(uri);
                }
                catch (ServerNotFoundMIPException snfe)
                {

                }
                catch (InvalidCredentialsMIPException ice)
                {

                }
                catch (Exception e)
                {

                }

                IExporter _exporter = new MKVExporter { Filename = scaleNumb };
                List<Item> _cameraItems = new List<Item>();

                Configuration oInstance = Configuration.Instance;
                List<VideoOS.Platform.Item> oItems = oInstance.GetItems();
                List<Item> Cams = GetCameras(oItems[0]);
                Item setItem = Cams.Find(x => x.Name == cameraName);

                _cameraItems.Add(setItem);

                string destPath = Path.Combine(@"D:\IMS", "Replay\\" + scaleNumb + @"\" + cameraName);

                _exporter.Init();
                _exporter.Path = destPath;
                foreach (var camera in _cameraItems)
                {
                    _exporter.CameraList.Add(camera);
                }

                DateTime frDt = Convert.ToDateTime("2021-08-17 15:56:45");
                DateTime toDt = Convert.ToDateTime("2021-08-17 16:06:45");

                if (_exporter.StartExport(frDt.ToUniversalTime(), toDt.ToUniversalTime()))
                {
                    _exporter.EndExport();

                    return "Y";
                }
                else
                {
                    return "N";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static List<Item> getchikldren(List<Item> oItem)
        {
            List<Item> oCams = new List<Item>();

            foreach (Item childSite in oItem)
            {
                if (childSite.FQID.Kind == Kind.Camera && childSite.FQID.FolderType == FolderType.No) oCams.Add(childSite);
                oCams.AddRange(getchikldren(childSite.GetChildren()));
            }
            return oCams;
        }


        public static List<Item> GetCameras(Item oItem)
        {
            List<Item> oCams = new List<Item>();

            if (oItem.HasChildren != VideoOS.Platform.HasChildren.No)
            {
                if (oItem.FQID.Kind == Kind.Camera && oItem.FQID.FolderType == FolderType.No) oCams.Add(oItem);
                oCams = getchikldren(oItem.GetChildren());

            }

            return oCams;
        }
    }
}

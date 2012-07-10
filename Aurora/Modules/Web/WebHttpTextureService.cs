﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Aurora.Framework;
using Aurora.Framework.Servers.HttpServer;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenSim.Services.Interfaces;

namespace Aurora.Modules.Web
{
    public class WebHttpTextureService : IService, IWebHttpTextureService
    {
        protected IRegistryCore _registry;
        protected string _gridNick;
        protected IHttpServer _server;

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            _registry = registry;
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            var conf = config.Configs["GridInfoService"];
            if (conf != null)
                _gridNick = conf.GetString("gridname", "No Grid Nick Available, please set this");
            else
                _gridNick = "No Grid Nick Available, please set this";
        }

        public void FinishedStartup()
        {
            _server = _registry.RequestModuleInterface<ISimulationBase>().GetHttpServer(0);
            if (_server != null)
            {
                _server.AddHTTPHandler("GridTexture", OnHTTPGetTextureImage);
                _registry.RegisterModuleInterface<IWebHttpTextureService>(this);
            }
        }

        public string GetTextureURL(UUID textureID)
        {
            return _server.ServerURI + "/index.php?method=GridTexture&uuid=" + textureID.ToString();
        }

        public Hashtable OnHTTPGetTextureImage(Hashtable keysvals)
        {
            Hashtable reply = new Hashtable();

            int statuscode = 200;
            byte[] jpeg = new byte[0];
            IAssetService m_AssetService = _registry.RequestModuleInterface<IAssetService>();

            MemoryStream imgstream = new MemoryStream();
            Bitmap texture = new Bitmap(1, 1);
            ManagedImage managedImage;
            Image image = (Image)texture;

            try
            {
                // Taking our jpeg2000 data, decoding it, then saving it to a byte array with regular jpeg data

                imgstream = new MemoryStream();

                // non-async because we know we have the asset immediately.
                AssetBase mapasset = m_AssetService.Get(keysvals["uuid"].ToString());

                if (mapasset != null)
                {
                    // Decode image to System.Drawing.Image
                    if (OpenJPEG.DecodeToImage(mapasset.Data, out managedImage, out image))
                    {
                        // Save to bitmap

                        texture = ResizeBitmap(image, 128, 128);
                        EncoderParameters myEncoderParameters = new EncoderParameters();
                        myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);

                        // Save bitmap to stream
                        texture.Save(imgstream, GetEncoderInfo("image/jpeg"), myEncoderParameters);

                        // Write the stream to a byte array for output
                        jpeg = imgstream.ToArray();
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                // Reclaim memory, these are unmanaged resources
                // If we encountered an exception, one or more of these will be null
                if (texture != null)
                    texture.Dispose();

                if (image != null)
                    image.Dispose();

                if (imgstream != null)
                {
                    imgstream.Close();
                    imgstream.Dispose();
                }
            }


            reply["str_response_string"] = Convert.ToBase64String(jpeg);
            reply["int_response_code"] = statuscode;
            reply["content_type"] = "image/jpeg";

            return reply;
        }

        public Bitmap ResizeBitmap(Image b, int nWidth, int nHeight)
        {
            Bitmap newsize = new Bitmap(nWidth, nHeight);
            Graphics temp = Graphics.FromImage(newsize);
            temp.DrawImage(b, 0, 0, nWidth, nHeight);
            temp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            temp.DrawString(_gridNick, new Font("Arial", 8, FontStyle.Regular), 
                new SolidBrush(Color.FromArgb(90, 255, 255, 50)), new Point(2, 115));

            return newsize;
        }

        // From msdn
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (int j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}

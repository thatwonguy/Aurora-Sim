# AURORA

The Aurora Development Team is proud to present the release of Aurora virtual world server.

The Aurora server is an OpenSim derived project with heavy emphasis on supporting all users, 
increased technology focus, heavy emphasis on working with other developers,
whether it be viewer based developers or server based developers, 
and a set of features around stability and simplified usability for users.

## Compiling Aurora

*To compile Aurora, look at the Compiling.txt in the AuroraDocs folder for more information*

## Configuration

*To see how to configure Aurora, look at "Setting up Aurora.txt" in the AuroraDocs folder for more information*

## Router issues
If you are having issues logging into your simulator, take a look at http://forums.osgrid.org/viewtopic.php?f=14&t=2082 in the Router Configuration section for more information on ways to resolve this issue.

------------------------------------------
## lsl second life language script 
channelID, channelUri);
            }
            voice_credentials["channel_uri"] = channelUri;
            voice_credentials["channel_credentials"] = "";
            map["voice_credentials"] = voice_credentials;

            // <llsd><map>
            //       <key>session-id</key><string>c0da7611-9405-e3a4-0172-c36a1120c77a</string>
            //       <key>voice_credentials</key><map>
            //           <key>channel_credentials</key><string>rh1iIIiT2v+ebJjRI+klpFHjFmo</string>
            //           <key>channel_uri</key><string>sip:confctl-12574742@bhr.vivox.com</string>
            //       </map>
            // </map></llsd>
            return map;
        }

        #endregion

        #region Vivox Calls

        private static readonly string m_vivoxLoginPath = "http://{0}/api2/viv_signin.php?userid={1}&pwd={2}";

        /// <summary>
        ///     Perform administrative login for Vivox.
        ///     Returns a hash table containing values returned from the request.
        /// </summary>
        private XmlElement VivoxLogin(string name, string password)
        {
            string requrl = String.Format(m_vivoxLoginPath, m_vivoxServer, name, password);
            return VivoxCall(requrl, false);

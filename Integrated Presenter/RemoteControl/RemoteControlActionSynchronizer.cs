using Integrated_Presenter.BMDSwitcher.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VonEX;

namespace Integrated_Presenter.RemoteControl
{
    class RemoteControlActionSynchronizer
    {

        MainWindow _window;
        MasterConnection MasterConnection;
        SlaveConnection SlaveConnection;

        bool isConnected = false;
        bool isMasterCon = false;


        public RemoteControlActionSynchronizer(MainWindow window)
        {
            _window = window;
        }


        public void OpenConnectionAsMaster()
        {
            isMasterCon = true;
            IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName()).Last();

            MasterConnection = new MasterConnection();
            MasterConnection.StartServer(ip, 80);
            MasterConnection.OnConnectionFromClient += (string ip, bool connected) =>
            {
                if (connected)
                {
                    MessageBox.Show($"Recieved connection from {ip} : {Dns.GetHostEntry(IPAddress.Parse(ip)).HostName}", "Remote Connection Established");
                    isConnected = connected;
                }
            };
            MasterConnection.OnDataRecieved += Connection_OnDataRecieved;
            Task.Run(() =>
            {
                MasterConnection.AcceptConnection();
            });
        }

        private void Connection_OnDataRecieved(string data)
        {
            var cmdval = Regex.Match(data, "(?<cmd>.*):(?<params>.*)");

            string cmd = cmdval.Groups["cmd"].Value;
            string args = cmdval.Groups["params"].Value;

            switch (cmd)
            {
                case "Preset":
                    ActionPreset(Convert.ToInt32(args), false);
                    break;
                default:
                    break;
            }
        }

        public void OpenConnectionAsSlave()
        {
            Connection ipmodal = new Connection("Connect to Master", "Master IP Address");
            if (ipmodal.ShowDialog() == true)
            {
                string ip = ipmodal.IP;

                SlaveConnection = new SlaveConnection();
                SlaveConnection.OnDataRecieved += Connection_OnDataRecieved;
                SlaveConnection.OnConnectionFromMaster += (string ip, bool connected) =>
                {
                    isConnected = connected;
                };
                SlaveConnection.Connect(ip, 80);
            }

        }


        private void SendRemoteCmd(string cmd, params string[] args)
        {
            string cmdstring = $"{cmd}:{string.Join(",", args)}";


            if (!isConnected)
            {
                return;
            }

            if (isMasterCon)
            {
                MasterConnection.SendMessage(cmdstring);     
            }
            else
            {
                SlaveConnection.SendString(cmdstring);
            }

        }





        public void ActionPreset(int button, bool sendremote = true)
        {
            _window.SetPreset(button);
            if (sendremote)
            {
                SendRemoteCmd("Preset", button.ToString());
            }
        }

        public void ActionProgram(int button)
        {
            _window.SetProgram(button);
        }

        public void ActionToggleUSK1()
        {
            _window.ToggleUSK1();
        }

        public void ActionTieUSK1()
        {
            _window.ToggleTieDSK1();
        }

        public void ActionUSK1RuntoKeyA()
        {
            _window.USK1RuntoA();
        }

        public void ActionUSK1RuntoKeyB()
        {
            _window.USK1RuntoB();
        }

        public void ActionUSK1RunToFull()
        {
            _window.USK1RuntoFull();
        }

        public void ActionSetUSK1FillSource(int source)
        {
            _window.ChangePIPFillSource(source);
        }



        public void ActionToggleDSK1()
        {
            _window.ToggleDSK1();
        }

        public void ActionTieDSK1()
        {
            _window.ToggleTieDSK1();
        }

        public void ActionAutoDSK1()
        {
            _window.AutoDSK1();
        }

        public void ActionToggleDSK2()
        {
            _window.ToggleDSK2();
        }

        public void ActionTieDSK2()
        {
            _window.ToggleTieDSK2();
        }

        public void ActionAutoDSK2()
        {
            _window.AutoDSK2();
        }

        public void ActionFTB()
        {

        }

        public void ActionToggleLayerBKGD()
        {
            _window.ToggleTransBkgd();
        }

        public void ActionToggleLayerKey1()
        {
            _window.ToggleTransKey1();
        }


        public void ActionCutTransition()
        {
            _window.TakeCutTransition();
        }

        public void ActionAutoTransition()
        {
            _window.TakeAutoTransition();
        }


        public void ActionConfigureSwitcher(BMDSwitcherConfigSettings config)
        {
            _window.SetSwitcherSettings(config);
        }




        public void ActionNextSlide()
        {
            _window.nextSlide();
        }

        public void ActionPrevSlide()
        {
            _window.prevSlide();
        }

        public void ActionTakeSlide()
        {
            _window.SlideDriveVideo_Current();
        }

        public void ActionToggleDrive(bool toval)
        {
            _window.ToggleSlideDriveVideo(toval);
        }


        public void ActionStartMedia()
        {
            _window.playMedia();
        }

        public void ActionStopMedia()
        {
            _window.stopMedia();
        }

        public void ActionPauseMedia()
        {
            _window.pauseMedia();
        }

        public void ActionRestartMedia()
        {
            _window.restartMedia();
        }



        public void ActionResetGPTimer1()
        {
            _window.ResetGPTimer1();
        }

        public void ActionClearMasterCaution()
        {
            _window.ClearMasterCaution();
        }





    }
}

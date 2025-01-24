﻿using CCU.Config;

using CommonGraphics;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using Xenon.Helpers;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for CCUConfigEditor.xaml
    /// </summary>
    public partial class CCUConfigEditor : Window
    {

        public bool WasClosed { get; set; }

        List<CombinedPresetInfo> m_presets = new List<CombinedPresetInfo>();

        Action<CCPUConfig_Extended> _saveChanges;

        Action<CCPUConfig_Extended> _exportChanges;

        CCPUConfig_Extended _cfgOrig;

        bool _newchanges = false;
        bool NewChanges
        {
            get => _newchanges;
            set
            {
                _newchanges = value;
                Dispatcher.Invoke(() =>
                {
                    btnSave.IsEnabled = _newchanges;
                    btnSave.Foreground = _newchanges ? Brushes.Orange : Brushes.Gray;
                });
            }
        }

        public CCUConfigEditor(CCPUConfig_Extended cfg, Action<CCPUConfig_Extended> SaveChanges, Action<CCPUConfig_Extended> exportChanges)
        {
            InitializeComponent();

            _cfgOrig = cfg;

            _saveChanges = SaveChanges;

            m_presets = cfg.CompileIntoPresetInfo();
            foreach (var pst in m_presets.OrderBy(x => x.CamName))
            {
                var ctrl = new CCUPresetItem(pst);
                ctrl.OnEditsChanged += Ctrl_OnEditsChanged;
                ctrl.OnDeleteRequest += Ctrl_OnDeleteRequest;
                lvItems.Children.Add(ctrl);
            }

            foreach (var client in cfg.Clients)
            {
                string cass = "";
                if (!cfg.CameraAssociations.TryGetValue(client.Name, out cass))
                {
                    cass = "NO ASSOCIATED CAMERA";
                }

                var ctrl = new CCUClientItem(client.Name, client.IPAddress, client.Port.ToString(), cass);
                ctrl.OnDeleteRequest += Ctrl_OnDeleteRequest_client;
                ctrl.OnEditsMade += Ctrl_OnEditsMade;
                wpClients.Children.Add(ctrl);
            }



            NewChanges = false;
            _exportChanges = exportChanges;
        }

        private void Ctrl_OnEditsMade(object sender, EventArgs e)
        {
            NewChanges = true;
        }

        private void Ctrl_OnDeleteRequest_client(object sender, EventArgs e)
        {
            NewChanges = true;
            var ctrl = sender as CCUClientItem;
            ctrl.OnDeleteRequest -= Ctrl_OnDeleteRequest_client;
            ctrl.OnEditsMade -= Ctrl_OnEditsMade;
            wpClients.Children.Remove(ctrl);
        }

        private void Ctrl_OnDeleteRequest(object sender, CCUPresetItem e)
        {
            e.OnEditsChanged -= Ctrl_OnEditsChanged;
            e.OnDeleteRequest -= Ctrl_OnDeleteRequest;

            lvItems.Children.Remove(e);
            NewChanges = true;
        }

        private void Ctrl_OnEditsChanged(object sender, EventArgs e)
        {
            NewChanges = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.WasClosed = true;
        }

        private void ClickCreateNewPreset(object sender, RoutedEventArgs e)
        {
            var ctrl = new CCUPresetItem(CombinedPresetInfo.DefaultTemplate());
            ctrl.OnDeleteRequest += Ctrl_OnDeleteRequest;
            ctrl.OnEditsChanged += Ctrl_OnEditsChanged;
            lvItems.Children.Insert(0, ctrl);

            NewChanges = true;
        }

        private void ClickSaveChanges(object sender, RoutedEventArgs e)
        {
            CCPUConfig_Extended newCFG = ConsolidateConfig();

            _saveChanges?.Invoke(newCFG);

            NewChanges = false;
        }

        private CCPUConfig_Extended ConsolidateConfig()
        {
            List<CombinedPresetInfo> info = new List<CombinedPresetInfo>();

            foreach (var child in lvItems.Children)
            {
                var citem = child as CCUPresetItem;
                if (citem != null)
                {
                    info.Add(citem?.GetInfo());
                }
            }


            // build an extended cfg from it
            CCPUConfig_Extended newCFG = new CCPUConfig_Extended();

            foreach (var cpst in info)
            {

                if (newCFG.KeyedPresets.TryGetValue(cpst.CamName, out var presets))
                {
                    if (presets == null)
                    {
                        presets = new Dictionary<string, DVIPProtocol.Protocol.Lib.Inquiry.PTDrive.RESP_PanTilt_Position>();
                    }
                    presets[cpst.PresetPosName] = new DVIPProtocol.Protocol.Lib.Inquiry.PTDrive.RESP_PanTilt_Position() { Pan = cpst.Pan, Tilt = cpst.Tilt, Valid = cpst.Valid };
                }
                else
                {
                    newCFG.KeyedPresets[cpst.CamName] = new Dictionary<string, DVIPProtocol.Protocol.Lib.Inquiry.PTDrive.RESP_PanTilt_Position>();
                    newCFG.KeyedPresets[cpst.CamName][cpst.PresetPosName] = new DVIPProtocol.Protocol.Lib.Inquiry.PTDrive.RESP_PanTilt_Position() { Pan = cpst.Pan, Tilt = cpst.Tilt, Valid = cpst.Valid };
                }

                if (newCFG.KeyedZooms.TryGetValue(cpst.CamName, out var zooms))
                {
                    if (zooms == null)
                    {
                        zooms = new Dictionary<string, CameraDriver.ZoomProgram>();
                    }
                    zooms[cpst.ZoomPresetName] = new CameraDriver.ZoomProgram(cpst.ZoomMS, cpst.ZoomMode);
                }
                else
                {
                    newCFG.KeyedZooms[cpst.CamName] = new Dictionary<string, CameraDriver.ZoomProgram>();
                    newCFG.KeyedZooms[cpst.CamName][cpst.ZoomPresetName] = new CameraDriver.ZoomProgram(cpst.ZoomMS, cpst.ZoomMode);
                }

                if (newCFG.MockPresetInfo.TryGetValue(cpst.CamName, out var mocks))
                {
                    if (mocks == null)
                    {
                        mocks = new Dictionary<string, CCPUConfig_Extended.PresetMockInfo>();
                    }
                    mocks[cpst.PresetPosName] = new CCPUConfig_Extended.PresetMockInfo { RuntimeMS = cpst.MoveMS, Thumbnail = cpst.Thumbnail };
                }
                else
                {
                    newCFG.MockPresetInfo[cpst.CamName] = new Dictionary<string, CCPUConfig_Extended.PresetMockInfo>();
                    newCFG.MockPresetInfo[cpst.CamName][cpst.PresetPosName] = new CCPUConfig_Extended.PresetMockInfo { RuntimeMS = cpst.MoveMS, Thumbnail = cpst.Thumbnail };
                }

            }


            foreach (var ictrl in wpClients.Children)
            {
                var ctrl = ictrl as CCUClientItem;
                if (ctrl != null)
                {
                    var client = new CCPUConfig.ClientConfig();
                    client.Name = ctrl.tbName.Text;
                    client.IPAddress = ctrl.tbIP.Text;
                    if (int.TryParse(ctrl.tbPort.Text, out var p))
                    {
                        client.Port = p;
                    }
                    newCFG.Clients.Add(client);
                    newCFG.CameraAssociations[client.Name] = ctrl.tbAssociation.Text;
                }
            }

            return newCFG;
        }

        private void AddClient(object sender, RoutedEventArgs e)
        {
            var ctrl = new CCUClientItem("NEW CAMERA", "172.0.0.1", "5002", "NEW CAMERA ASSOCIATION");
            ctrl.OnDeleteRequest += Ctrl_OnDeleteRequest_client;
            ctrl.OnEditsMade += Ctrl_OnEditsMade;
            wpClients.Children.Add(ctrl);
        }

        private void ClickExportAll(object sender, RoutedEventArgs e)
        {
            CCPUConfig_Extended newCFG = ConsolidateConfig();

            _exportChanges?.Invoke(newCFG);
        }

        private void DumpImg(object sender, RoutedEventArgs e)
        {
            CCPUConfig_Extended newCFG = ConsolidateConfig();

            using (var sfd = new SaveFileDialog())
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // use the filename as a folder name
                    try
                    {
                        var dir = Directory.CreateDirectory(Path.GetDirectoryName(sfd.FileName));
                        foreach (var cam in newCFG.MockPresetInfo)
                        {
                            string camName = cam.Key;
                            foreach (var pst in cam.Value)
                            {
                                var bytes = Convert.FromBase64String(pst.Value.Thumbnail);
                                using (var img = Image.Load(bytes))
                                {
                                    var path = Path.Combine(dir.FullName, $"{camName}_{pst.Key}.png");
                                    img.SaveAsPng(path);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // sad. do nothing
                    }
                }
            }
        }
    }
}

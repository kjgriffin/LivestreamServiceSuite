using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FFPlayer = Unosquare.FFME;

namespace MediaEngine.Playout
{
    /// <summary>
    /// Interaction logic for CueableMediaPlayer.xaml
    /// </summary>
    public partial class CueableMediaPlayer : UserControl, ICueableMediaPlayer
    {

        private VisualBrush[] m_brushSources;
        private FFPlayer.MediaElement[] m_players;

        private int m_OnAirIndex = 0;
        private int backing_incCueRequestIndex = 0;
        private int m_GetNextCueRequestIndex { get => backing_incCueRequestIndex++; }
        private int m_activeCues { get => MaxConcurrentCue - m_availablePlayers.Count; }

        private HashSet<string> m_cuedmediapaths = new HashSet<string>();

        private Dictionary<string, CueRequest> m_cueRequests = new Dictionary<string, CueRequest>();
        private Dictionary<int, CueRequest> m_allocatedCueRequestTracker = new Dictionary<int, CueRequest>();
        private Dictionary<Guid, Uri> m_houseIdTracker = new Dictionary<Guid, Uri>();
        private Queue<int> m_availablePlayers = new Queue<int>();

        private struct CueRequest
        {
            public int RequestNum { get; private set; }
            public int AllocatedClipSource { get; private set; }
            public Uri Source { get; private set; }
            public Guid HouseID { get; private set; }
            public ICueableMediaPlayer.MediaCueState CueState { get; private set; }
            public string SourcePath { get => Source.AbsolutePath; }

            public CueRequest(int rnum, int clipsource, Uri source, Guid houseID, ICueableMediaPlayer.MediaCueState cueState = ICueableMediaPlayer.MediaCueState.Uncued)
            {
                this.CueState = cueState;
                this.RequestNum = rnum;
                this.AllocatedClipSource = clipsource;
                this.Source = source;
                this.HouseID = houseID;
            }
            public CueRequest UpdateRequestNum(int newVal)
            {
                return new CueRequest(newVal, this.AllocatedClipSource, this.Source, this.HouseID, this.CueState);
            }
            public CueRequest UpdateCueState(ICueableMediaPlayer.MediaCueState newVal)
            {
                return new CueRequest(this.RequestNum, this.AllocatedClipSource, this.Source, this.HouseID, newVal);
            }

        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CueableMediaPlayer()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();

            // register players and brushes
            m_players = new FFPlayer.MediaElement[4]
            {
                clip_a,
                clip_b,
                clip_c,
                clip_d,
            };
            m_brushSources = new VisualBrush[m_players.Length];
            for (int i = 0; i < m_players.Length; i++)
            {
                m_brushSources[i] = new VisualBrush(m_players[i]);
                m_availablePlayers.Enqueue(i);
            }

        }

        public IEnumerable<Uri> CuedMedia { get; }
        public int MaxConcurrentCue { get => 4; }

        public Task CueMedia(Uri source, Guid houseID)
        {
            // check we haven't already got it cued.
            if (m_cuedmediapaths.Contains(source.AbsolutePath))
            {
                // if so, update its cue-priority
                var oldreq = m_cueRequests[source.AbsolutePath];
                var newreq = oldreq.UpdateRequestNum(m_GetNextCueRequestIndex);
                m_cueRequests[source.AbsolutePath] = newreq;
                m_allocatedCueRequestTracker[newreq.AllocatedClipSource] = newreq;
                m_houseIdTracker[houseID] = source;
                return Task.CompletedTask;
            }

            // figure out if we can cue without contention
            if (m_availablePlayers.Any())
            {
                var req = new CueRequest(m_GetNextCueRequestIndex, m_availablePlayers.Dequeue(), source, houseID);
                m_houseIdTracker[houseID] = source;
                return ProcessCueRequestInternal(req);
            }

            // else we need to kick the oldest cue request
            // search all cue-requests for oldest and borrow re-cue that player to the new media
            var oldest = m_cueRequests.AsQueryable().OrderBy(x => x.Value.RequestNum).First();
            var overwritereq = new CueRequest(m_GetNextCueRequestIndex, oldest.Value.AllocatedClipSource, source, houseID);
            m_houseIdTracker[houseID] = source;
            // remove old cue-request
            m_cueRequests.Remove(oldest.Key);
            m_houseIdTracker.Remove(oldest.Value.HouseID);
            return ProcessCueRequestInternal(overwritereq);
        }

        private async Task ProcessCueRequestInternal(CueRequest req)
        {
            var cuingreq = req.UpdateCueState(ICueableMediaPlayer.MediaCueState.Cueing);
            m_allocatedCueRequestTracker[cuingreq.AllocatedClipSource] = cuingreq;
            m_cueRequests[req.SourcePath] = cuingreq;
            await Dispatcher.BeginInvoke(() => m_players[req.AllocatedClipSource].Open(req.Source));
            var cuedreq = cuingreq.UpdateCueState(ICueableMediaPlayer.MediaCueState.Cued);
            m_cueRequests[cuedreq.SourcePath] = cuedreq;
            m_allocatedCueRequestTracker[cuedreq.AllocatedClipSource] = cuedreq;
        }




        public async Task HotLoadMediOnAir(Uri source, Guid houseID)
        {
            // just replace the on-air media with what's provided.
            // update cue-tracking as necessary
            var replacereq = m_allocatedCueRequestTracker[m_OnAirIndex];
            var overwritereq = new CueRequest(m_GetNextCueRequestIndex, replacereq.AllocatedClipSource, source, houseID);
            m_houseIdTracker[houseID] = source;
            // remove old cue-request
            m_cueRequests.Remove(replacereq.SourcePath);
            m_houseIdTracker.Remove(replacereq.HouseID);
            await ProcessCueRequestInternal(overwritereq);
            PutMediaOnAir(houseID);
        }

        public bool PutMediaOnAir(Guid houseID, bool UnCueLastOnAir = true)
        {
            if (m_houseIdTracker.TryGetValue(houseID, out var source))
            {
                var req = m_cueRequests[source.AbsolutePath];
                // check if source is currently cued
                if (req.CueState == ICueableMediaPlayer.MediaCueState.Cued)
                {
                    CueRequest lastonairreq = new CueRequest();
                    bool hadsomethingprevonair = false;
                    if (m_allocatedCueRequestTracker.ContainsKey(m_OnAirIndex))
                    {
                        hadsomethingprevonair = true;
                        lastonairreq = m_allocatedCueRequestTracker[m_OnAirIndex];
                    }

                    // put it on air
                    m_OnAirIndex = req.AllocatedClipSource;
                    // NOTE: super important this is called with BeginInvoke. This seems to make all the difference for it not to crash unexpectedly
                    Dispatcher.BeginInvoke(() =>
                    {
                        //rect_display.Fill = m_brushSources[m_OnAirIndex];
                        rect_display.Fill = new VisualBrush(m_players[m_OnAirIndex]);
                    });

                    // TODO: this was envisioned to handle the case where we'd want the last 'slide' to remain cued so we could go backwards in slides
                    /*
                        for now we won't worry about this.
                        Reason: handling it here only helps go back one, then we'd still need to track the 'presentation' to re-cue the one beforehand if we're going backward
                        So I think we can just get away with doing something simpler like over-writing the oldest cue-request if we want to keep the last source cued.
                        But I'm not really sure we'd ever strictly need to do that...
                     */

                    // re/un-cue last media as requested
                    if (hadsomethingprevonair)
                    {
                        if (UnCueLastOnAir)
                        {
                            m_houseIdTracker.Remove(lastonairreq.HouseID);
                            RemoveCueRequestBySource(lastonairreq.Source);
                            m_allocatedCueRequestTracker.Remove(lastonairreq.AllocatedClipSource);
                            m_availablePlayers.Enqueue(lastonairreq.AllocatedClipSource);
                        }
                        else
                        {
                            var recue = lastonairreq.UpdateRequestNum(m_GetNextCueRequestIndex);
                            m_allocatedCueRequestTracker[recue.AllocatedClipSource] = recue;
                            m_cueRequests[recue.SourcePath] = recue;
                        }
                    }
                }

            }
            return false;
        }

        private void RemoveCueRequestBySource(Uri source)
        {
            if (!m_houseIdTracker.ContainsValue(source))
            {
                m_cueRequests.Remove(source.AbsolutePath);
            }
        }



    }
}

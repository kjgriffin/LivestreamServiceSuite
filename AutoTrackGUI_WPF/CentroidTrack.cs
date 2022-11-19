using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTrackGUI_WPF
{

    internal class BoundingBox
    {
        internal double CenterX { get; private set; }
        internal double CenterY { get; private set; }
        internal double Width { get; private set; }
        internal double Height { get; private set; }

        public BoundingBox(Rect box)
        {
            Width = box.Width;
            Height = box.Height;
            CenterX = box.Left + box.Width / 2;
            CenterY = box.Top + box.Height / 2;
        }
    }

    internal class CentroidTrack
    {
        internal int TrackID { get; set; }
        internal long TotalHistory { get; set; }
        internal long ConcurrentHistory { get; set; }
        internal int TTL { get; set; }
        internal BoundingBox Box { get; set; }
        internal Queue<BoundingBox> HistoricBoxes { get; set; } = new Queue<BoundingBox>();
        internal int MaxHistoricBoxes = 100;
        internal bool IsStale { get => TTL <= 0; }


        internal void Update(BoundingBox closest, int ttl)
        {
            HistoricBoxes.Enqueue(Box);
            if (HistoricBoxes.Count > MaxHistoricBoxes)
            {
                HistoricBoxes.Dequeue();
            }
            Box = closest;
            TotalHistory++;
            TTL = ttl;
            ConcurrentHistory++;
        }

        internal void GoneStale()
        {
            TotalHistory++;
            TTL--;
            ConcurrentHistory = 0;
        }
    }

    internal class PIPTracker
    {
        /// <summary>
        /// PIP CENTER
        /// </summary>
        internal int CenterX { get; set; }
        /// <summary>
        /// PIP CENTER
        /// </summary>
        internal int CenterY { get; set; }
        /// <summary>
        /// PIP Half Width
        /// </summary>
        internal int HWidth { get; set; }
        /// <summary>
        /// PIP Half Height
        /// </summary>
        internal int HHeight { get; set; }


        private int _newID { get; set; }
        private int _TTL { get; set; } = 10;
        private int _DeadXTol { get; set; } = 100;
        private int _DeadYTol { get; set; } = 100;
        private int _CCHPROMOTE { get; set; } = 10;

        private List<CentroidTrack> _tracks = new List<CentroidTrack>();
        private List<CentroidTrack> _unconfirmedTracks = new List<CentroidTrack>();
        private List<CentroidTrack> _longDead = new List<CentroidTrack>();

        internal List<CentroidTrack> GetActiveTracks()
        {
            return new List<CentroidTrack>(_tracks);
        }

        internal void Update(List<BoundingBox> blips)
        {
            // reject any blip not in bounds
            var validBlips = blips.Where(b => Math.Abs(b.CenterX - CenterX) < HWidth && Math.Abs(b.CenterY - CenterY) < HHeight)
                                  .ToList();

            // update tracks assuming closest match
            foreach (var existingTrack in _tracks)
            {
                // find a closest match from anything we have captured
                var closest = validBlips.OrderBy(x => Math.Pow(CenterX - x.CenterX, 2) + Math.Pow(CenterY - x.CenterY, 2))
                                       .FirstOrDefault();
                if (closest != null)
                {
                    // update track
                    existingTrack.Update(closest, _TTL);
                    validBlips.Remove(closest);
                }
                else
                {
                    existingTrack.GoneStale();
                }
            }

            // if we have leftover blips, then we'll see if they match something that's long dead
            if (validBlips.Any())
            {
                foreach (var dead in _longDead)
                {
                    // find a closest match from anything we have captured
                    var closest = validBlips.OrderBy(x => Math.Pow(CenterX - x.CenterX, 2) + Math.Pow(CenterY - x.CenterY, 2))
                                           .FirstOrDefault();

                    // we'll also require that it looks similar (size/position to last known pos)
                    if (closest != null && Math.Abs(closest.CenterX - CenterX) < _DeadXTol && Math.Abs(closest.CenterY - CenterY) < _DeadYTol)
                    {
                        // update track
                        dead.Update(closest, _TTL);
                        // move to speculative tracks
                        _unconfirmedTracks.Add(dead);
                        validBlips.Remove(closest);
                    }
                }
            }

            // if we still have leftover blips then we'll start a new un-confirmed track
            foreach (var newblip in validBlips)
            {
                var uctrack = new CentroidTrack
                {
                    Box = newblip,
                    TotalHistory = 0,
                    TrackID = _newID++,
                    TTL = _TTL,
                };
            }

            var staleTracks = _tracks.Where(x => x.IsStale).ToList();
            foreach (var staleTrack in staleTracks)
            {
                _longDead.Add(staleTrack);
                _tracks.Remove(staleTrack);
            }

            // promote new un-confirmed tracks that have sufficient history
            var promotedTracks = _unconfirmedTracks.Where(x => x.ConcurrentHistory > _CCHPROMOTE).ToList();
            foreach (var ptrack in promotedTracks)
            {
                _tracks.Add(ptrack);
                _unconfirmedTracks.Remove(ptrack);
            }

        }


    }

}

﻿using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestScreenshot
{

    internal class PIPBox
    {
        public int PIPID { get; set; }
        public int CX { get; set; }
        public int CY { get; set; }
        public int XR { get; set; }
        public int YR { get; set; }

        public bool ContainsBox(Centroid obj)
        {
            bool c1 =  Math.Abs(obj.Center.X - CX) < XR && Math.Abs(obj.Center.Y - CY) < YR;
            bool b = obj.Bounds.Left > CX - XR && obj.Bounds.Right < CX + XR && obj.Bounds.Top > CY - YR && obj.Bounds.Bottom < CY + YR;
            return c1 && b;
        }
    }

    internal class Centroid
    {

        internal Rect Bounds { get; set; }
        internal Point2d Center { get; set; }

        public Centroid(Rect bounds)
        {
            Bounds = bounds;
            Center = new Point2d(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        }

    }

    internal class CentroidTrack
    {

        internal Centroid Centroid { get; private set; }
        internal int XVel { get; private set; }
        internal int YVel { get; private set; }

        internal bool Stale { get => TTL < 45; }
        internal int TTL { get; private set; } = 50;
        internal int ID { get; private set; }
        internal string Name { get => $"<{PIPGEN}> ID: {ID:00} -- TTL:{TTL}"; }
        internal int PIPGEN { get; set; } = -1;

        internal long Hits { get; private set; } = 0;

        internal void Update(Centroid newTroid)
        {
            // compute velocity
            if (Math.Abs(newTroid.Center.X - Centroid.Center.X) > 5)
            {
                XVel = newTroid.Center.X < Centroid.Center.X ? -1 : 1;
            }
            else
            {
                XVel = 0;
            }
            if (Math.Abs(newTroid.Center.Y - Centroid.Center.Y) > 5)
            {
                YVel = newTroid.Center.Y < Centroid.Center.Y ? -1 : 1;
            }
            else
            {
                YVel = 0;
            }

            Centroid = newTroid;
            TTL = 50;
            Hits++;
        }
        internal void UpdateLifetime()
        {
            TTL -= 1;
        }
        public CentroidTrack(Centroid centroid, int ttl, int id, int PIPGEN = -1)
        {
            Centroid = centroid;
            TTL = ttl;
            ID = id;
            this.PIPGEN = PIPGEN;
        }

        internal void UpdatePredictionByVelocity()
        {
            double vscalar = 10;
            Centroid.Center = new Point2d(Centroid.Center.X + XVel * vscalar, Centroid.Center.Y + YVel * vscalar);

            // only allow this trick once
            //XVel = 0;
            //YVel = 0;
        }
    }


    internal class CentroidTracker
    {
        internal List<PIPBox> PIPS { get; set; } = new List<PIPBox>();
        List<CentroidTrack> Tracks { get; set; } = new List<CentroidTrack>();

        List<CentroidTrack> CandidateTracks = new List<CentroidTrack>();

        private int m_nextID = 1;

        internal List<CentroidTrack> GetTracks()
        {
            return new List<CentroidTrack>(Tracks);
        }

        internal void UpdateTracks(IEnumerable<Rect> rects)
        {
            // compute new centroids for all rects
            List<Centroid> newTroids = new List<Centroid>();
            foreach (var rect in rects)
            {
                newTroids.Add(new Centroid(rect));
            }

            // try update tracks with closest object
            foreach (var track in Tracks)
            {
                var closest = newTroids.OrderBy(c => Math.Pow(c.Center.X - track.Centroid.Center.X, 2) + Math.Pow(c.Center.Y - track.Centroid.Center.Y, 2)).FirstOrDefault();
                if (closest != null)
                {
                    // use it!
                    newTroids.Remove(closest);
                    track.Update(closest);
                }
                else
                {
                    // try updating the track by its velocity
                    // (we'll only do this once, then kill velocity)
                    track.UpdatePredictionByVelocity();
                }

                // always update lifes
                track.UpdateLifetime();
            }
            // remove expired tracks
            Tracks.RemoveAll(t => t.TTL < 0);

            foreach (var candidate in CandidateTracks)
            {
                var closest = newTroids.OrderBy(c => Math.Pow(c.Center.X - candidate.Centroid.Center.X, 2) + Math.Pow(c.Center.Y - candidate.Centroid.Center.Y, 2)).FirstOrDefault();
                if (closest != null)
                {
                    // use it!
                    newTroids.Remove(closest);
                    candidate.Update(closest);
                }

                // always update lifes
                candidate.UpdateLifetime();
            }
            // remove stale candidate tracks
            CandidateTracks.RemoveAll(t => t.TTL < 0);

            // promote tracks with high confidence
            var promoted = CandidateTracks.Where(t => t.Hits > 10).ToList();
            foreach (var track in promoted)
            {
                CandidateTracks.Remove(track);
                Tracks.Add(track);
            }

            // create new tracks for remaining objects
            foreach (var newObj in newTroids)
            {
                int pipid = -1;
                // assign it to a pip
                //CandidateTracks.Add(new CentroidTrack(newObj, 50, m_nextID++, pipid));
                foreach (var pip in PIPS)//.OrderBy(x => Math.Pow(x.CX - newObj.Center.X, 2) + Math.Pow(x.CY - newObj.Center.Y, 2))) // let closetst PIP capture the new track
                {
                    if (pip.ContainsBox(newObj))
                    {
                        pipid = pip.PIPID;
                        CandidateTracks.Add(new CentroidTrack(newObj, 50, m_nextID++, pipid));
                        break;
                    }
                }
            }
        }

    }


}

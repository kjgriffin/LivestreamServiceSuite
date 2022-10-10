using OpenCvSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestScreenshot
{
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
        internal string Name { get => $"ID: {ID:00} -- TTL:{TTL}"; }
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
        }
        internal void UpdateLifetime()
        {
            TTL -= 1;
        }
        public CentroidTrack(Centroid centroid, int ttl, int id)
        {
            Centroid = centroid;
            TTL = ttl;
            ID = id;
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
        List<CentroidTrack> Tracks { get; set; } = new List<CentroidTrack>();

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

            // create new tracks for remaining objects
            foreach (var newObj in newTroids)
            {
                Tracks.Add(new CentroidTrack(newObj, 50, m_nextID++));
            }
        }

    }


}
